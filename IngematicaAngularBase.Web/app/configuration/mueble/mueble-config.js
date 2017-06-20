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
        });

}]);