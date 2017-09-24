angular.module('appBase').config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {
    $stateProvider
        .state('app.optimizacionHistorial-list', {
            url: '/optimizacion-historial/optimizacion-historial-list',
            views: {
                'content@': {
                    templateUrl: 'configuration/optimizacion-historial/optimizacion-historial-list.html',
                    controller: 'optimizacionHistorialListController',
                    controllerAs: 'vm',
                    resolve: {
                        parameters: ['$q', '$stateParams', 'blockUI', 'entityUI', 'securityFactory', 'optimizacionHistorialFactory', 'handleErrorService',
                            function ($q, $stateParams, blockUI, entityUI, securityFactory, optimizacionHistorialFactory, handleErrorService) {

                                var queryUI = optimizacionHistorialFactory.getQueryUI();
                                var params = {};
                                var deferred = $q.defer();

                                blockUI.start();

                                securityFactory.can(['OptimizacionHistorial_CanList', 'OptimizacionHistorial_CanDelete'])
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
        });
}]);