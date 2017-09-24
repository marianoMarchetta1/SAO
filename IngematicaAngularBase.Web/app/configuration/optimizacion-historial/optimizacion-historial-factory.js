
angular.module('appBase').factory('optimizacionHistorialFactory', ['$http', '$q', 'handleErrorService', 'entityUI', 'frameworkUI',
    function ($http, $q, handleErrorService, entityUI, frameworkUI) {

        var urlBase = '/api/optimizacionHistorial';
        var optimizacionHistorialFactory = {};
        var queryUI = {};
        var deleteUI = {};

        optimizacionHistorialFactory.getQueryUI = function () {
            if (entityUI.isEmpty(queryUI))
                queryUI = frameworkUI.createQueryUI(optimizacionHistorialFactory.listCommand());
            return queryUI;
        };

        optimizacionHistorialFactory.getDeleteUI = function () {
            if (entityUI.isEmpty(deleteUI))
                deleteUI = frameworkUI.createDeleteUI(optimizacionHistorialFactory.deleteCommand(), optimizacionHistorialFactory.getQueryUI());
            return deleteUI;
        };

        optimizacionHistorialFactory.listCommand = function () {
            var obj = { query: null };
            obj.execute = function () {
                return optimizacionHistorialFactory.list(obj.query);
            }
            return obj;
        };

        optimizacionHistorialFactory.list = function (query) {
            var postQUery = angular.copy(query);
            entityUI.preparePost(postQUery);
            var deferred = $q.defer();
            $http.post(urlBase + '/' + 'list', postQUery)
                .then(function (response) {
                    deferred.resolve({ list: response.data, query: postQUery });
                }).catch(function (response) {
                    deferred.reject(handleErrorService.rejectHttpError(response.data, response.status));
                });
            return deferred.promise;
        };

        optimizacionHistorialFactory.deleteCommand = function () {
            var obj = { id: null };
            obj.execute = function () {
                return optimizacionHistorialFactory.delete(obj.id);
            }
            return obj;
        };

        optimizacionHistorialFactory.delete = function (id) {
            var deferred = $q.defer();
            $http.delete(urlBase + '/' + id)
                .then(function (response) {
                    deferred.resolve(response.data);
                })
                .catch(function (response) {
                    deferred.reject(handleErrorService.rejectHttpError(response.data, response.status));
                });
            return deferred.promise;
        };

        optimizacionHistorialFactory.get = function (id) {
            var deferred = $q.defer();
            $http.get(urlBase + '/' + id)
                .then(function (response) {
                    deferred.resolve(response.data);
                })
                .catch(function (response) {
                    deferred.reject(handleErrorService.rejectHttpError(response.data, response.status));
                });
            return deferred.promise;
        };

        return optimizacionHistorialFactory;
    }]);