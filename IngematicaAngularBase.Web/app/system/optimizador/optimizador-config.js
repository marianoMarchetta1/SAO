angular.module('appBase').config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {
    $stateProvider
        .state('app.optimizador-proceso', {
            url: '/optimizador/proceso',
            views: {
                'content@': {
                    templateUrl: 'system/optimizador/optimizador-proceso.html',
                    controller: 'optimizadorProcesoController',
                    controllerAs: 'vm',
                    resolve: {
                        parameters: ['$q', '$stateParams', 'blockUI', 'entityUI', 'securityFactory', 'optimizadorFactory', 'handleErrorService',
                            function ($q, $stateParams, blockUI, entityUI, securityFactory, optimizadorFactory, handleErrorService) {

                                var params = {};
                                var deferred = $q.defer();

                                blockUI.start();

                                securityFactory.can(['Optimizador_CanAdd'])
                                    .then(function (items) {
                                        params.dataRules = items;
                                        return $q.all([optimizadorFactory.getParamsList(false).then(function (dataParams) { return dataParams; })]);
                                    })
                                    .then(function (value) {
                                        params.dataParams = value[0];
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
}])