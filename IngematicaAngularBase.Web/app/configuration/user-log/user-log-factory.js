
angular.module('appBase').factory('userLogFactory', ['$http', '$q', 'handleErrorService', 'entityUI', 'frameworkUI',
    function ($http, $q, handleErrorService, entityUI, frameworkUI) {

        var urlBase = '/api/userLog';
        var userLogFactory = {};
        var queryUI = {};
        var deleteUI = {};

        userLogFactory.getQueryUI = function () {
            if (entityUI.isEmpty(queryUI))
                queryUI = frameworkUI.createQueryUI(userLogFactory.listCommand());
            return queryUI;
        };

        userLogFactory.getDeleteUI = function () {
            if (entityUI.isEmpty(deleteUI))
                deleteUI = frameworkUI.createDeleteUI(userLogFactory.deleteCommand(), userLogFactory.getQueryUI());
            return deleteUI;
        };

        userLogFactory.listCommand = function () {
            var obj = { query: null };
            obj.execute = function () {
                return userLogFactory.list(obj.query);
            }
            return obj;
        };

        userLogFactory.list = function (query) {
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

        userLogFactory.deleteCommand = function () {
            var obj = { id: null };
            obj.execute = function () {
                return userLogFactory.delete(obj.id);
            }
            return obj;
        };

        userLogFactory.delete = function (id) {
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

        return userLogFactory;
    }]);