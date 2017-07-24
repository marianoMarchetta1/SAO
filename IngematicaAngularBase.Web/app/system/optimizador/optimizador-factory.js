angular.module('appBase').factory('optimizadorFactory', ['$http', '$q', 'handleErrorService', 'entityUI', 'frameworkUI',
    function ($http, $q, handleErrorService, entityUI, frameworkUI) {

        var urlBase = '/api/optimizador';
        var usuarioFactory = {};
        var queryUI = {};
        var deleteUI = {};

        optimizadorFactory.getParamsList = function () {
            var deferred = $q.defer();
            $http.get(urlBase + '/' + 'getParamsList')
                .then(function (response) {
                    deferred.resolve(response.data);
                })
                .catch(function (response) {
                    deferred.reject(handleErrorService.rejectHttpError(response.data, response.status));
                });
            return deferred.promise;
        };

        return optimizadorFactory;
    }]);