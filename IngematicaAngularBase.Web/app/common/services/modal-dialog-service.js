angular.module('appBase').service('modalDialogService', ['$uibModal', 'constants', function ($uibModal, constants) {

    var modalDialogDefaults = {
        backdrop: 'static',
        keyboard: true,
        modalFade: true,
        templateUrl: 'common/services/modal-dialog.html'
    };

    var dialogModel = {
        cancelButtonText: 'Cancelar',
        actionButtonText: 'Aceptar',
        headerText: constants.applicationName,
        bodyText: 'Esta seguro?',
        showCancel: true
    };

    this.showModalDialog = function (mensaje, model) {
        var tempModalDefaults = angular.copy(modalDialogDefaults);

        model.bodyText = mensaje;

        tempModalDefaults.controller = ['$scope', '$uibModalInstance', function ($scope, $uibModalInstance) {
            $scope.model = model;
            $scope.model.ok = function (result) {
                $uibModalInstance.close(result);
            };
            if (model.showCancel)
                $scope.model.cancel = function (result) {
                    $uibModalInstance.dismiss('cancel');
                };
        }];

        return $uibModal.open(tempModalDefaults);
    };


    this.showModalMessage = function (mensaje) {
        var model = angular.copy(dialogModel);
        model.showCancel = false;
        return this.showModalDialog(mensaje, model)
    };

    this.showModalConfirmation = function (mensaje) {
        var model = angular.copy(dialogModel);
        return this.showModalDialog(mensaje, model)
    };


    this.showModalCanDelete = function () {
        return this.showModalConfirmation("¿Está seguro de querer eliminar este registro?");
    };


    var modalDialogErrorDefaults = {
        backdrop: true,
        keyboard: true,
        modalFade: true,
        templateUrl: 'common/services/modal-dialog-errors.html'
    };

    var dialogModelError = {
        closeButtonText: 'Cerrar',
        actionButtonText: 'Aceptar',
        headerText: constants.applicationName,
        bodyText: 'Por favor corrija los siguientes errores'
    };

    this.showModalFormErrors = function (errors) {
        var model = angular.copy(dialogModelError);
        var modalDialogError = angular.copy(modalDialogErrorDefaults);

        model.items = errors;

        modalDialogError.controller = ['$scope', '$uibModalInstance', function ($scope, $uibModalInstance) {
            $scope.model = model;
            $scope.model.ok = function (result) {
                $uibModalInstance.close(result);
            };
            $scope.model.close = function (result) {
                $uibModalInstance.dismiss('cancel');
            };
        }];

        return $uibModal.open(modalDialogError);
    };



    }]);