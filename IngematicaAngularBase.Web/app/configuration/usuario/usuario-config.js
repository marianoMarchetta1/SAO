angular.module('appBase').config(['$stateProvider', '$urlRouterProvider', 'versionProvider', function ($stateProvider, $urlRouterProvider, versionProvider) {
    $stateProvider
    .state('app.usuario-list', {
        url: '/usuario/list',
        views: {
            'content@': {
                templateUrl: 'configuration/usuario/usuario-list.html?' + versionProvider.versionGuid,
                controller: 'usuarioListController',
                controllerAs: 'vm',
                resolve: {
                    parameters: ['$q', '$stateParams', 'blockUI', 'entityUI', 'securityFactory', 'usuarioFactory', 'handleErrorService',
                        function ($q, $stateParams, blockUI, entityUI, securityFactory, usuarioFactory, handleErrorService) {

                            var queryUI = usuarioFactory.getQueryUI();
                            var params = {};
                            var deferred = $q.defer();

                            blockUI.start();
                        
                            securityFactory.can(['Usuario_CanList', 'Usuario_CanAdd', 'Usuario_CanEdit', 'Usuario_CanDelete'])
                            .then(function (items) {
                                params.dataRules = items;
                                return $q.all([usuarioFactory.getInitParamsList(false).then(function (dataParams) { return dataParams; }),
                                                queryUI.refresh().then(function (dataList) { return dataList; })]);
                            })
                            .then(function (value) {
                                params.dataParams = value[0];
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
    .state('app.usuario-add', {
        url: '/usuario/add',
        views: {
            'content@': {
                templateUrl: 'configuration/usuario/usuario-add-update.html?' + versionProvider.versionGuid,
                controller: 'usuarioAddOrUpdateController',
                controllerAs: 'vm',
                resolve: {
                    parameters: ['$q', '$stateParams', 'blockUI', 'usuarioFactory','handleErrorService',
                        function ($q, $stateParams, blockUI, usuarioFactory, handleErrorService) {

                        blockUI.start();
                        var deferred = $q.defer();

                        usuarioFactory.getParamsAddOrUpdate()
                        .then(function (dataParams) {
                            deferred.resolve({ params: dataParams, entity: {}, mode: 'add' });
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
    .state('app.usuario-update', {
        url: '/usuario/update/:id',
        views: {
            'content@': {
                templateUrl: 'configuration/usuario/usuario-add-update.html?' + versionProvider.versionGuid,
                controller: 'usuarioAddOrUpdateController',
                controllerAs: 'vm',
                resolve: {
                    parameters: ['$q', '$stateParams', 'blockUI', 'usuarioFactory', 'handleErrorService',
                        function ($q, $stateParams, blockUI, usuarioFactory, handleErrorService) {

                        blockUI.start();
                        var deferred = $q.defer();

                        $q.all([
                         usuarioFactory.getParamsAddOrUpdate().then(function (dataParams) { return dataParams; }),
                         usuarioFactory.get($stateParams.id).then(function (dataEntity) { return dataEntity; })
                        ])
                        .then(function (values) {
                            deferred.resolve( { params: values[0], entity: values[1], mode: 'update' });
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
    .state('app.usuario-detail', {
        url: '/usuario/detail/:id',
        views: {
            'content@': {
                templateUrl: 'configuration/usuario/usuario-detail.html?' + versionProvider.versionGuid,
                controller: 'usuarioDetailController',
                controllerAs: 'vm',
                resolve: {
                    parameters: ['$q', '$stateParams', 'blockUI', 'usuarioFactory', 'handleErrorService',
                        function ($q, $stateParams, blockUI, usuarioFactory, handleErrorService) {

                        blockUI.start();
                        var deferred = $q.defer();

                        usuarioFactory.get($stateParams.id)
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