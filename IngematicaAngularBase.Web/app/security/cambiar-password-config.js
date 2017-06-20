angular.module('appBase').config(['$stateProvider', '$urlRouterProvider', 'versionProvider', function ($stateProvider, $urlRouterProvider, versionProvider) {
    $stateProvider
    .state('app.cambiar-password', {
        url: '/security/cambiar-password',
        views: {
            'content@': {
                templateUrl: 'security/cambiar-password.html?' + versionProvider.versionGuid,
                controller: 'cambiarPasswordController',
                controllerAs: 'vm'            
            }
        }
    });
}])