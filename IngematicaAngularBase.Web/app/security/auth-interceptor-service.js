angular.module('appBase').factory('authInterceptorFactory', ['$q', '$injector', '$location', 'localStorageService',
function ($q, $injector, $location, localStorageService) {

    var authInterceptorServiceFactory = {};

    authInterceptorServiceFactory.request = function (config) {

        config.headers = config.headers || {};
       
        var authData = localStorageService.get('authorizationData');
        if (authData) {
            config.headers.Authorization = 'Bearer ' + authData.token;
        }

        return config;
    }

    authInterceptorServiceFactory.responseError = function (rejection) {

        if (rejection.status === 401) {
            var authServiceFactory = $injector.get('authServiceFactory');
            var httpBuffer = $injector.get('httpBuffer');
            var authData = localStorageService.get('authorizationData');

            if (true) {
                var deferred = $q.defer();
                httpBuffer.append(rejection.config, deferred);
                if (httpBuffer.getItems().length > 1) 
                    return deferred.promise;

                if (authData) {
                    authServiceFactory.refreshToken().then(function (response) {

                        httpBuffer.retryAll();
                    },
                     function (err) {
                         httpBuffer.rejectAll('401');
                         authServiceFactory.logOut();
                     });

                    return deferred.promise;
                }
            }
            else {
                authServiceFactory.logOut();
            }
        }
        return $q.reject(rejection);
    }

    return authInterceptorServiceFactory;
}]);


