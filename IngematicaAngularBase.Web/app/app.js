angular.module('appBase', ['ui.router', 'LocalStorageModule', 'ui.bootstrap', 'blockUI', 'ngFileUpload', 'ngSanitize', 'ui.codemirror'])
.run(['$rootScope', '$location', '$state', '$stateParams', '$timeout', 'authServiceFactory',
function ($rootScope, $location, $state, $stateParams, $timeout, authServiceFactory) {

    authServiceFactory.fillAuthData();

    $rootScope.$on("$stateChangeStart",
        function (event, toState, toParams, fromState, fromParams) {

            if (toState.name !== 'appan.login' &&
                toState.name !== 'appan.resetpassExitoso' &&
                toState.name !== 'appan.resetpassError' &&
                toState.name !== 'appan.cambiarPasswordAnonimo' &&
                !authServiceFactory.authentication.isAuth) {
                event.preventDefault();
                $state.go('appan.login');
            }
        });
}])
.config(['$httpProvider', function ($httpProvider) {

    $httpProvider.interceptors.push('authInterceptorFactory');
}])
.config(['blockUIConfig', function (blockUIConfig) {
    blockUIConfig.autoBlock = false;
    blockUIConfig.message = 'Cargando...';
}])
.config(['uibDatepickerPopupConfig', function (uibDatepickerPopupConfig) {
    uibDatepickerPopupConfig.datepickerPopup = 'dd/MM/yyyy';
    uibDatepickerPopupConfig.clearText = "Borrar";
    uibDatepickerPopupConfig.currentText = "Hoy";
    uibDatepickerPopupConfig.closeText = "Cerrar";
}])
.config(['uibPaginationConfig', function (uibPaginationConfig) {
    uibPaginationConfig.maxSize = 9;
    uibPaginationConfig.firstText = 'Primera';
    uibPaginationConfig.previousText = "Anterior";
    uibPaginationConfig.nextText = 'Siguiente';
    uibPaginationConfig.lastText = "Ultima";
    uibPaginationConfig.rotate = false;
    uibPaginationConfig.boundaryLinks = true;
}])
.constant('constants', {
    apiServiceBaseUri: document.location.protocol + '//' + document.location.host + '/',
    clientId: 'IngematicaAngularBase',
    applicationName: 'SAO',
    clientName: 'Ingematica Angular Base'
})
.provider('version', function () {
    this.versionGuid = '1111111';

    this.$get = function () {
        var versionGuid = this.versionGuid;
        return {
            getVersionGuid: function () {
                return versionGuid
            }
        }
    };

    this.setVersionGuid = function (versionGuid) {
        this.versionGuid = versionGuid;
    };
})
.config(function (versionProvider) {
    versionProvider.setVersionGuid('@version');
});