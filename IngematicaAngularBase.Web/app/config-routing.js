angular.module('appBase').config(['$stateProvider', '$urlRouterProvider', 'versionProvider', function ($stateProvider, $urlRouterProvider, versionProvider) {
    $urlRouterProvider.otherwise(function ($injector, $location) {
        var $state = $injector.get("$state");
        var authServiceFactory = $injector.get("authServiceFactory");
        if (authServiceFactory.authentication.isAuth)
            if ($location.$$path == '' || $location.$$path == '/')
                $state.go("app.home", {}, {
                    location: false
                });

            else
                $state.go("pagenotfound", {}, {
                    location: false
                });
        else
            $state.go("appan.login");
    });

    $urlRouterProvider.when('', '/home');


    $stateProvider
    .state('pagenotfound', {
        url: '/pagenotfound',
        views: {
            'header': {
                templateUrl: 'anonymous-layout/anonymous-header.html?' + versionProvider.versionGuid
            },
            'content': {
                templateUrl: 'anonymous-layout/pagenotfound.html?' + versionProvider.versionGuid
            },
            'footer': {
                templateUrl: 'anonymous-layout/anonymous-footer.html?' + versionProvider.versionGuid
            }
        }
    })
    .state('app', {
        abstract: true,
        views: {
            'header': {
                templateUrl: 'layout/header.html?' + versionProvider.versionGuid,
                controller: 'headerController',
                resolve: {

                    parameters: ['$q', '$stateParams', 'blockUI', 'securityFactory', function ($q, $stateParams, blockUI, securityFactory) {
                        var myBlockUI = blockUI.instances.get('myBlockUI');
                        if (myBlockUI._refs == 0)
                            myBlockUI = blockUI;
                        myBlockUI.start();
                        return securityFactory.get()
                        .catch(function (error) {
                            return $q.reject('');
                        })
                        .finally(function () {
                            myBlockUI.stop();
                        });

                    }]
                }

            },
            'content': {
                templateUrl: 'layout/content.html?' + versionProvider.versionGuid
            },
            'footer': {
                templateUrl: 'layout/footer.html?' + versionProvider.versionGuid
            }
        }
    })
    .state('app.home', {
        url: '/home',
        views: {
            'content@': {
                templateUrl: 'home/home.html?' + versionProvider.versionGuid,
                controller: 'indexController'
            }
        }
    })
    .state('appan', {
        url: '/auth',
        abstract: true,
        views: {
            'header': {
                templateUrl: 'anonymous-layout/anonymous-header.html?' + versionProvider.versionGuid
            },
            'content': {
                templateUrl: 'anonymous-layout/anonymous-content.html?' + versionProvider.versionGuid
            },
            'footer': {
                templateUrl: 'anonymous-layout/anonymous-footer.html?' + versionProvider.versionGuid
            }
        }
    })
    .state('appan.login', {
        url: '/login',
        views: {
            'content@': {
                templateUrl: 'security/login.html?' + versionProvider.versionGuid
            }
        }
    })
    .state('appan.resetpassExitoso', {
        url: '/resetpassexitoso',
        views: {
            'content@': {
                templateUrl: 'security/cambiar-password-exito.html?' + versionProvider.versionGuid
            }
        }
    })
    .state('appan.resetpassError', {
        url: '/resetpasserror',
        views: {
            'content@': {
                templateUrl: 'security/cambiar-password-exito.html?' + versionProvider.versionGuid
            }
        }
    })
    .state('appan.cambiarPasswordAnonimo', {
        url: '/cambiarpasswordanonimo?guid',
        views: {
            'content@': {
                templateUrl: 'security/cambiar-password-anonimo.html?' + versionProvider.versionGuid,
                controller: 'cambiarPasswordAnonimoController',
                controllerAs: 'vm',
                resolve: {
                    parameters: ['$q', '$stateParams', 'blockUI', 'entityUI', 'authServiceFactory', 'handleErrorService',
                        function ($q, $stateParams, blockUI, entityUI, authServiceFactory, handleErrorService) {

                            //Busco en la base el GUID asociado.
                            //CASO 1: Devuelvo la entidad que se encontro en la base.
                            //CASO 2: No existe el guid enviado -> ERROR.
                            //CASO 3: Expiro el tiempo de cambio para el password -> ERROR.

                            var deferred = $q.defer();

                            blockUI.start();

                            authServiceFactory.getUserByGuid($stateParams.guid)
                            .then(function (data) {
                                params = data.user;
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
}])