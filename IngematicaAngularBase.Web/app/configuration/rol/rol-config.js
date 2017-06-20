angular.module('appBase').config(['$stateProvider', '$urlRouterProvider', 'versionProvider', function ($stateProvider, $urlRouterProvider, versionProvider) {
    $stateProvider
    .state('app.rol-list', {
        url: '/rol/list',
        views: {
            'content@': {
                templateUrl: 'configuration/rol/rol-list.html?' + versionProvider.versionGuid,
                controller: 'rolListController',
                controllerAs: 'vm',
                resolve: {
                    parameters: ['$q', '$stateParams', 'blockUI', 'entityUI', 'securityFactory', 'rolFactory', 'handleErrorService',
                        function ($q, $stateParams, blockUI, entityUI, securityFactory, rolFactory, handleErrorService) {

                            var queryUI = rolFactory.getQueryUI();
                            var params = {};
                            var deferred = $q.defer();

                            blockUI.start();

                            securityFactory.can(['Rol_CanList', 'Rol_CanAdd', 'Rol_CanEdit', 'Rol_CanDelete'])
                            .then(function (items) {
                                params.dataRules = items;
                                return queryUI.refresh();
                            })
                            .then(function (value) {
                                params.mode = 'list';
                                deferred.resolve(params);
                            })
                            .catch(function (error) {
                                handleErrorService.handleErrorConfig(error);
                                deferred.reject();
                            })
                            .finally(function () {
                                blockUI.stop();
                            });

                            return deferred.promise;
                        }]
                }
            }
        }
    })
    .state('app.rol-add', {
        url: '/rol/add',
        views: {
            'content@': {
                templateUrl: 'configuration/rol/rol-add-update.html?' + versionProvider.versionGuid,
                controller: 'rolAddOrUpdateController',
                controllerAs: 'vm',
                resolve: {
                    parameters: ['$q', '$stateParams', 'blockUI', 'rolFactory', 'handleErrorService',
                        function ($q, $stateParams, blockUI, rolFactory, handleErrorService) {

                        blockUI.start();
                        var deferred = $q.defer();

                        rolFactory.getNew()
                        .then(function (dataEntity) {
                            deferred.resolve( { entity: dataEntity, mode: 'add' });
                        })
                        .catch(function (error) {
                            handleErrorService.handleErrorConfig(error);
                            deferred.reject();
                        })
                        .finally(function () {
                            blockUI.stop();
                        });

                        return deferred.promise;
                    }]
                }
            }
        }
    })
    .state('app.rol-update', {
        url: '/rol/update/:id',
        views: {
            'content@': {
                templateUrl: 'configuration/rol/rol-add-update.html?' + versionProvider.versionGuid,
                controller: 'rolAddOrUpdateController',
                controllerAs: 'vm',
                resolve: {
                    parameters: ['$q', '$stateParams', 'blockUI', 'rolFactory', 'handleErrorService',
                        function ($q, $stateParams, blockUI, rolFactory, handleErrorService) {

                            blockUI.start();
                            var deferred = $q.defer();

                            $q.all([
                             rolFactory.getParamsAddUpdate(true).then(function (dataParams) { return dataParams; }),
                             rolFactory.get($stateParams.id).then(function (dataEntity) { return dataEntity; })
                            ])
                            .then(function (values) {
                                deferred.resolve({ params: values[0], entity: values[1], mode: 'update' });
                            })
                            .catch(function (error) {
                                handleErrorService.handleErrorConfig(error);
                                deferred.reject();
                            })
                            .finally(function () {
                                blockUI.stop();
                            });

                            return deferred.promise;
                        }]
                }
            }
        }
    })
    .state('app.rol-detail', {
        url: '/rol/detail/:id',
        views: {
            'content@': {
                templateUrl: 'configuration/rol/rol-detail.html?' + versionProvider.versionGuid,
                controller: 'rolDetailController',
                controllerAs: 'vm',
                resolve: {
                    parameters: ['$q', '$stateParams', 'blockUI', 'rolFactory', 'handleErrorService',
                        function ($q, $stateParams, blockUI, rolFactory, handleErrorService) {

                        blockUI.start();
                        var deferred = $q.defer();

                        rolFactory.get($stateParams.id)
                        .then(function (dataEntity) {
                            deferred.resolve({ entity: dataEntity, mode: 'detail' });
                        })
                        .catch(function (error) {
                            handleErrorService.handleErrorConfig(error);
                            deferred.reject();
                        })
                        .finally(function () {
                            blockUI.stop();
                        });

                        return deferred.promise;
                    }]
                }
            }
        }
    });
}])