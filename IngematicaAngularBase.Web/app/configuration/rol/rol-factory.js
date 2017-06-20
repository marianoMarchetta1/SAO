
angular.module('appBase').factory('rolFactory', ['$http', '$q', 'handleErrorService', 'entityUI', 'frameworkUI',
    function ($http, $q, handleErrorService,entityUI, frameworkUI) {

        var urlBase = '/api/rol';
        var rolFactory = {};
        var queryUI = {};
        var deleteUI = {};

        rolFactory.getQueryUI = function () {
            if (entityUI.isEmpty(queryUI))
                queryUI = frameworkUI.createQueryUI(rolFactory.listCommand());
            return queryUI;
        };

        rolFactory.getDeleteUI = function () {
            if (entityUI.isEmpty(deleteUI))
                deleteUI = frameworkUI.createDeleteUI(rolFactory.deleteCommand(), rolFactory.getQueryUI());
            return deleteUI;
        };

        rolFactory.listCommand = function () {
            var obj = { query: null };
            obj.execute = function () {
                return rolFactory.list(obj.query);
            }
            return obj;
        };

        rolFactory.list = function (query) {
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

        rolFactory.add = function (model) {
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

        rolFactory.update = function (model) {
            var postModel = angular.copy(model);
            entityUI.preparePost(postModel);
            var deferred = $q.defer();
            $http.put(urlBase + '/' + model.idRol, postModel)
            .then(function (response) {
                deferred.resolve(response.data);
            })
            .catch(function (response) {
                deferred.reject(handleErrorService.rejectHttpError(response.data, response.status));
            });
            return deferred.promise;
        };

        rolFactory.deleteCommand = function () {
            var obj = { id: null };
            obj.execute = function () {
                return rolFactory.delete(obj.id);
            }
            return obj;
        };

        rolFactory.delete = function (id) {
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

        rolFactory.get = function (id) {
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

        rolFactory.getParamsAddOrUpdate = function () {
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

        rolFactory.getNew = function () {
            var deferred = $q.defer();
            $http.get(urlBase + '/' + 'getNew')
            .then(function (response) {
                deferred.resolve(response.data);
            })
            .catch(function (response) {
                deferred.reject(handleErrorService.rejectHttpError(response.data, response.status));
            });
            return deferred.promise;
        };

        rolFactory.getParamsAddUpdate = function () {
            var deferred = $q.defer();
            $http.get(urlBase + '/' + 'getParamsAddUpdate')
            .then(function (response) {
                deferred.resolve(response.data);
            })
            .catch(function (response) {
                deferred.reject(handleErrorService.rejectHttpError(response.data, response.status));
            });
            return deferred.promise;
        };

        return rolFactory;
    }]);