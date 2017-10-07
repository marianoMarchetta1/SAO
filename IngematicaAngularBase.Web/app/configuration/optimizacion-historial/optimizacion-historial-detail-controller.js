angular.module('appBase').controller('optimizacionHistorialDetailController', ['$scope', '$state', '$stateParams', 'optimizacionHistorialFactory', 'blockUI', '$uibModal',
    'entityUI', 'parameters', 'modalDialogService', 'UploadBase', 'flowFactory',
    function ($scope, $state, $stateParams, optimizacionHistorialFactory, blockUI, $uibModal, entityUI, parameters, modalDialogService, UploadBase, flowFactory) {

        var vm = this;


        vm.init = function () {
            var entity = parameters.entity;
            vm.mode = parameters.mode;
            vm.imageStrings = [];

            vm.target = {};
            vm.optimizacionHistorial = entity;

            if (vm.optimizacionHistorial.paths && vm.optimizacionHistorial.paths.length > 0)
                modalDialogService.showModalMessage('Los planos optimizados se han generado con exito y se encuentran en: ' + vm.optimizacionHistorial.paths[0]);
            else
                modalDialogService.showModalFormErrors(["Error al generar los archivos."]);
        };

        vm.init();
    }]);