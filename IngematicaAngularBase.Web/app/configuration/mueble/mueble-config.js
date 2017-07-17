angular.module('appBase').config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {
    $stateProvider
        .state('app.mueble-list', {
            url: '/mueble/list',
            views: {
                'content@': {
                    templateUrl: 'configuration/mueble/mueble-list.html',
                    controller: 'muebleListController',
                    controllerAs: 'vm',
                    resolve: {
                        parameters: ['$q', '$stateParams', 'blockUI', 'entityUI', 'securityFactory', 'muebleFactory', 'handleErrorService',
                            function ($q, $stateParams, blockUI, entityUI, securityFactory, muebleFactory, handleErrorService) {

                                var queryUI = muebleFactory.getQueryUI();
                                var params = {};
                                var deferred = $q.defer();

                                blockUI.start();

                                securityFactory.can(['Mueble_CanList', 'Mueble_CanAdd', 'Mueble_CanEdit', 'Mueble_CanDelete'])
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
        .state('app.mueble-add', {
            url: '/mueble/add',
            views: {
                'content@': {
                    templateUrl: 'configuration/mueble/mueble-add-update.html?',
                    controller: 'muebleAddOrUpdateController',
                    controllerAs: 'vm',
                    resolve: {
                        parameters: ['$q', '$stateParams', 'blockUI', 'muebleFactory','flowFactory',
                            function ($q, $stateParams, blockUI, muebleFactory, flowFactory) {
                                return { mode: 'add' };
                            }]
                    }
                }
            }
        })
        .state('app.mueble-update', {
            url: '/mueble/update/:id',
            views: {
                'content@': {
                    templateUrl: 'configuration/mueble/mueble-add-update.html?',
                    controller: 'muebleAddOrUpdateController',
                    controllerAs: 'vm',
                    resolve: {
                        parameters: ['$q', '$stateParams', 'blockUI', 'muebleFactory', 'handleErrorService',
                            function ($q, $stateParams, blockUI, muebleFactory, handleErrorService) {

                                blockUI.start();
                                var deferred = $q.defer();

                                muebleFactory.get($stateParams.id)
                                    .then(function (dataEntity) {
                                        deferred.resolve({ entity: dataEntity, mode: 'update' });
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
        .state('app.mueble-detail', {
            url: '/mueble/detail/:id',
            views: {
                'content@': {
                    templateUrl: 'configuration/mueble/mueble-detail.html',
                    controller: 'muebleDetailController',
                    controllerAs: 'vm',
                    resolve: {
                        parameters: ['$q', '$stateParams', 'blockUI', 'muebleFactory', 'handleErrorService',
                            function ($q, $stateParams, blockUI, muebleFactory, handleErrorService) {

                                blockUI.start();
                                var deferred = $q.defer();

                                muebleFactory.get($stateParams.id)
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

}]);