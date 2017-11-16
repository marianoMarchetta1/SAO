
angular.module('appBase').factory('envioMailFactory', ['$http', '$q', 'handleErrorService', 'entityUI', 'frameworkUI',
    function ($http, $q, handleErrorService, entityUI, frameworkUI) {

        var urlBase = '/api/envioMail';
        var envioMailFactory = {};

        envioMailFactory.send = function (mail) {
            var deferred = $q.defer();
            $http.post(urlBase + '/' + 'send', mail)
                .then(function (response) {
                    deferred.resolve({ list: response.data });
                }).catch(function (response) {
                    deferred.reject(handleErrorService.rejectHttpError(response.data, response.status));
                });
            return deferred.promise;
        };

        return envioMailFactory;
    }]);