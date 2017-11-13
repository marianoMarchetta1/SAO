
angular.module('appBase').factory('optimizadorFactory', ['$http', '$q', 'handleErrorService', 'entityUI', 'frameworkUI',
    function ($http, $q, handleErrorService, entityUI, frameworkUI) {

        var urlBase = '/api/optimizador';
        var optimizadorFactory = {};
        var queryUI = {};

        optimizadorFactory.getQueryUI = function () {
            if (entityUI.isEmpty(queryUI))
                queryUI = frameworkUI.createQueryUI(optimizadorFactory.listCommand());
            return queryUI;
        };

        optimizadorFactory.listCommand = function () {
            var obj = { query: null };
            obj.execute = function () {
                return optimizadorFactory.proceso(obj.query);
            }
            return obj;
        };

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

        optimizadorFactory.generate = function (optimizacion) {
            var deferred = $q.defer();
            $http.post(urlBase + '/' + 'generate', optimizacion)
                .then(function (response) {
                    deferred.resolve(response.data);
                }).catch(function (response) {
                    deferred.reject(handleErrorService.rejectHttpError(response.data, response.status));
                });
            return deferred.promise;
        };

        optimizadorFactory.getBlob = function (path, i) {
            var deferred = $q.defer();
            $http.post(urlBase + '/' + 'postFileToBlob', {path : path})
                .then(function (response) {
                    deferred.resolve({ data: response.data, i: i });
                }).catch(function (response) {
                    deferred.reject(handleErrorService.rejectHttpError(response.data, response.status));
                });
            return deferred.promise;
        }

        optimizadorFactory.getBlobImage = function (path, j) {
            var deferred = $q.defer();
            $http.post(urlBase + '/' + 'postFileToBlobImage', { path: path })
                .then(function (response) {
                    deferred.resolve({ data: response.data, j : j });
                }).catch(function (response) {
                    deferred.reject(handleErrorService.rejectHttpError(response.data, response.status));
                });
            return deferred.promise;
        }

        //optimizadorFactory.add = function (model) {
        //    var postModel = angular.copy(model);
        //    entityUI.preparePost(postModel);
        //    var deferred = $q.defer();
        //    $http.post(urlBase, postModel)
        //        .then(function (response) {
        //            deferred.resolve(response.data);
        //        }).catch(function (response) {
        //            deferred.reject(handleErrorService.rejectHttpError(response.data, response.status));
        //        });
        //    return deferred.promise;
        //};

        //optimizadorFactory.get = function (id) {
        //    var deferred = $q.defer();
        //    $http.get(urlBase + '/' + id)
        //        .then(function (response) {
        //            deferred.resolve(response.data);
        //        })
        //        .catch(function (response) {
        //            deferred.reject(handleErrorService.rejectHttpError(response.data, response.status));
        //        });
        //    return deferred.promise;
        //};

        return optimizadorFactory;
    }]);