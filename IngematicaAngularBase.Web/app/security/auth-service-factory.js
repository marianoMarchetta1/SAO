angular.module('appBase').factory('authServiceFactory', ['$rootScope', 'httpBuffer', '$http', '$q', 'localStorageService', 'constants', '$state', '$injector', 'handleErrorService',
function ($rootScope, httpBuffer, $http, $q, localStorageService, constants, $state, $injector, handleErrorService) {

    var authServiceFactory = {};
    var serviceBase = constants.apiServiceBaseUri;

    authServiceFactory.authentication = {
        isAuth: false,
        userName: "",
        useRefreshTokens: true
    };

    authServiceFactory.loginConfirmed = function (data, configUpdater) {
        var updater = configUpdater || function(config) {return config;};
        //$rootScope.$broadcast('event:auth-loginConfirmed', data);
        httpBuffer.retryAll(updater);
    }

    authServiceFactory.loginCancelled = function (data, reason) {
        httpBuffer.rejectAll(reason);
        //$rootScope.$broadcast('event:auth-loginCancelled', data);
    }

    authServiceFactory.login = function (loginData) {

        var data = "grant_type=password&username=" + loginData.userName + "&password=" + loginData.password;

        if (loginData.useRefreshTokens) {
            data = data + "&client_id=" + constants.clientId;
        }

        var deferred = $q.defer();

        $http.post(serviceBase + 'token', data, { headers: { 'Content-Type': 'application/x-www-form-urlencoded' } })
        .success(function (response) {
            var authorizationData = { token: response.access_token, userName: loginData.userName, refreshToken: null, useRefreshTokens: false };

            if (loginData.useRefreshTokens) {
                authorizationData.refreshToken = response.refresh_token;
                authorizationData.useRefreshTokens = true;
            }

            localStorageService.set('authorizationData', authorizationData);

            authServiceFactory.authentication.isAuth = true;
            authServiceFactory.authentication.userName = loginData.userName;
            authServiceFactory.authentication.useRefreshTokens = loginData.useRefreshTokens;

            deferred.resolve(response);

        }).error(function (err, status) {
            authServiceFactory.logOut();
            deferred.reject(err);
        });

        return deferred.promise;

    }

    authServiceFactory.logOut = function () {
        
        localStorageService.remove('authorizationData');

        authServiceFactory.authentication.isAuth = false;
        authServiceFactory.authentication.userName = "";
        authServiceFactory.authentication.useRefreshTokens = false; 
        var securityFactory = $injector.get('securityFactory');
        securityFactory.clearRules();
        $state.go('appan.login');
    }


    authServiceFactory.refreshToken = function () {
        var deferred = $q.defer();
        var authData = localStorageService.get('authorizationData');
        if (authData) {

            if (authData.useRefreshTokens) {

                var data = "grant_type=refresh_token&refresh_token=" + authData.refreshToken + "&client_id=" + constants.clientId;

                localStorageService.remove('authorizationData');

                $http.post(serviceBase + 'token', data, { headers: { 'Content-Type': 'application/x-www-form-urlencoded' } }).success(function (response) {

                    localStorageService.set('authorizationData', { token: response.access_token, userName: response.userName, refreshToken: response.refresh_token, useRefreshTokens: true });

                    deferred.resolve(response);

                }).error(function (err, status) {
                    deferred.reject(err);
                });
            }
        }

        return deferred.promise;
    };



    authServiceFactory.fillAuthData = function () {

        var authData = localStorageService.get('authorizationData');
        if (authData) {
            authServiceFactory.authentication.isAuth = true;
            authServiceFactory.authentication.userName = authData.userName;
            authServiceFactory.authentication.useRefreshTokens = authData.useRefreshTokens;
        }

    };

    authServiceFactory.cambiarPasswordAnonimo = function (model) {
        var deferred = $q.defer();
        $http.post('/api/usuario/cambiarPasswordAnonimo', model)
         .then(function (response) {
             deferred.resolve(response.data);
         }).catch(function (response) {
             deferred.reject(handleErrorService.rejectHttpError(response.data, response.status));
         });
        return deferred.promise;
    };

    authServiceFactory.cambiarPassword = function (model) {
        var deferred = $q.defer();
        $http.post('/api/usuario/cambiarPassword', model)
         .then(function (response) {
             deferred.resolve(response.data);
         }).catch(function (response) {
             deferred.reject(handleErrorService.rejectHttpError(response.data, response.status));
         });
        return deferred.promise;
    };

    authServiceFactory.enviarPassword = function (username) {
        var deferred = $q.defer();
        $http.post('/api/usuario/enviarPassword', '"' + username + '"')
         .then(function (response) {
             deferred.resolve(response.data);
         }).catch(function (response) {
             deferred.reject(handleErrorService.rejectHttpError(response.data, response.status));
         });
        return deferred.promise;
    };

    authServiceFactory.getUserByGuid = function (guid) {
        var deferred = $q.defer();
        $http.get('/api/usuario/getUserByGuid?guid=' + guid)
         .then(function (response) {
             deferred.resolve(response.data);
         }).catch(function (response) {
             deferred.reject(handleErrorService.rejectHttpError(response.data, response.status));
         });
        return deferred.promise;
    };


    return authServiceFactory;
}])