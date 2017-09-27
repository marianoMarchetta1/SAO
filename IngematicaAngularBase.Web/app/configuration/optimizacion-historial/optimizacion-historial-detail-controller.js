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

        };

        vm.init();
    }]);