angular.module('appBase').config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {
    $stateProvider
        .state('app.userLog-list', {
            url: '/user-log/user-log-list',
            views: {
                'content@': {
                    templateUrl: 'configuration/user-log/user-log-list.html',
                    controller: 'userLogListController',
                    controllerAs: 'vm',
                    resolve: {
                        parameters: ['$q', '$stateParams', 'blockUI', 'entityUI', 'securityFactory', 'userLogFactory', 'handleErrorService',
                            function ($q, $stateParams, blockUI, entityUI, securityFactory, userLogFactory, handleErrorService) {

                                var queryUI = userLogFactory.getQueryUI();
                                var params = {};
                                var deferred = $q.defer();

                                blockUI.start();

                                securityFactory.can(['UserLog_CanList', 'UserLog_CanDelete'])
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