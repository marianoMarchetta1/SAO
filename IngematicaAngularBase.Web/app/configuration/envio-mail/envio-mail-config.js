angular.module('appBase').config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {
    $stateProvider
        .state('app.envio-mail', {
            url: '/envio-mail/send',
            views: {
                'content@': {
                    templateUrl: 'configuration/envio-mail/envio-mail.html',
                    controller: 'envioMailController',
                    controllerAs: 'vm',
                    resolve: {
                        parameters: ['$q', '$stateParams', 'blockUI', 'entityUI', 'securityFactory', 'envioMailFactory', 'handleErrorService',
                            function ($q, $stateParams, blockUI, entityUI, securityFactory, envioMailFactory, handleErrorService) {

                                var params = {};
                                var deferred = $q.defer();

                                blockUI.start();

                                securityFactory.can(['EnvioMail_CanSend'])
                                    .then(function (items) {
                                        params.dataRules = items;
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