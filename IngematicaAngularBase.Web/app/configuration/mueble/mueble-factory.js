
angular.module('appBase').factory('muebleFactory', ['$http', '$q', 'handleErrorService', 'entityUI', 'frameworkUI',
    function ($http, $q, handleErrorService, entityUI, frameworkUI) {

        var urlBase = '/api/mueble';
        var muebleFactory = {};
        var queryUI = {};
        var deleteUI = {};

        muebleFactory.getQueryUI = function () {
            if (entityUI.isEmpty(queryUI))
                queryUI = frameworkUI.createQueryUI(muebleFactory.listCommand());
            return queryUI;
        };

        muebleFactory.getDeleteUI = function () {
            if (entityUI.isEmpty(deleteUI))
                deleteUI = frameworkUI.createDeleteUI(muebleFactory.deleteCommand(), muebleFactory.getQueryUI());
            return deleteUI;
        };

        muebleFactory.listCommand = function () {
            var obj = { query: null };
            obj.execute = function () {
                return muebleFactory.list(obj.query);
            }
            return obj;
        };

        muebleFactory.list = function (query) {
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

        muebleFactory.add = function (model) {
            var postModel = angular.copy(model);
            entityUI.preparePost(postModel);
            var deferred = $q.defer();
            $http.post(urlBase, postModel)
                .then(function (response) {
                    deferred.resolve(response.data);
                }).catch(function (response) {
                    deferred.reject(handleErrorService.rejectHttpError(response.data, response.status));
                });
            return deferred.promise;
        };

        muebleFactory.update = function (model) {
            var postModel = angular.copy(model);
            entityUI.preparePost(postModel);
            var deferred = $q.defer();
            $http.put(urlBase + '/' + model.idMueble, postModel)
                .then(function (response) {
                    deferred.resolve(response.data);
                })
                .catch(function (response) {
                    deferred.reject(handleErrorService.rejectHttpError(response.data, response.status));
                });
            return deferred.promise;
        };

        muebleFactory.deleteCommand = function () {
            var obj = { id: null };
            obj.execute = function () {
                return muebleFactory.delete(obj.id);
            }
            return obj;
        };

        muebleFactory.delete = function (id) {
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

        muebleFactory.get = function (id) {
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

        return muebleFactory;
    }]);