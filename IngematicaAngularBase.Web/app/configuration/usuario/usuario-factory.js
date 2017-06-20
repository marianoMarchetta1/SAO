
angular.module('appBase').factory('usuarioFactory', ['$http', '$q', 'handleErrorService', 'entityUI', 'frameworkUI',
    function ($http, $q, handleErrorService, entityUI, frameworkUI) {

        var urlBase = '/api/usuario';
        var usuarioFactory = {};
        var queryUI = {};
        var deleteUI = {};

        usuarioFactory.getQueryUI = function () {
            if (entityUI.isEmpty(queryUI))
                queryUI = frameworkUI.createQueryUI(usuarioFactory.listCommand());
            return queryUI;
        };

        usuarioFactory.getDeleteUI = function () {
            if (entityUI.isEmpty(deleteUI))
                deleteUI = frameworkUI.createDeleteUI(usuarioFactory.deleteCommand(), usuarioFactory.getQueryUI());
            return deleteUI;
        };

        usuarioFactory.listCommand = function () {
            var obj = { query: null };
            obj.execute = function () {
                return usuarioFactory.list(obj.query);
            }
            return obj;
        };

        usuarioFactory.list = function (query) {
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

        usuarioFactory.add = function (model) {
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

        usuarioFactory.update = function (model) {
            var postModel = angular.copy(model);
            entityUI.preparePost(postModel);
            var deferred = $q.defer();
            $http.put(urlBase + '/' + model.idUsuario, postModel)
            .then(function (response) {
                deferred.resolve(response.data);
            })
            .catch(function (response) {
                deferred.reject(handleErrorService.rejectHttpError(response.data, response.status));
            });
            return deferred.promise;
        };

        usuarioFactory.deleteCommand = function () {
            var obj = { id: null };
            obj.execute = function () {
                return usuarioFactory.delete(obj.id);
            }
            return obj;
        };

        usuarioFactory.delete = function (id) {
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

        usuarioFactory.get = function (id) {
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

        usuarioFactory.getInitParamsList = function () {
            var deferred = $q.defer();
            $http.get(urlBase + '/' + 'getInitParamsList')
            .then(function (response) {
                deferred.resolve(response.data);
            })
            .catch(function (response) {
                deferred.reject(handleErrorService.rejectHttpError(response.data, response.status));
            });
            return deferred.promise;
        };

        usuarioFactory.getParamsAddOrUpdate = function () {
            var deferred = $q.defer();
            $http.get(urlBase + '/' + 'getInitParamsAddOrUpdate')
            .then(function (response) {
                deferred.resolve(response.data);
            })
            .catch(function (response) {
                deferred.reject(handleErrorService.rejectHttpError(response.data, response.status));
            });
            return deferred.promise;
        };

        return usuarioFactory;
    }]);