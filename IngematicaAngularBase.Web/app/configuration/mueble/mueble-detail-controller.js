angular.module('appBase').controller('muebleDetailController', ['$scope', '$state', '$stateParams', 'muebleFactory', 'blockUI', '$uibModal',
    'entityUI', 'parameters', 'modalDialogService', 'UploadBase', 'flowFactory',
    function ($scope, $state, $stateParams, muebleFactory, blockUI, $uibModal, entityUI, parameters, modalDialogService, UploadBase, flowFactory) {

        var vm = this;


        vm.init = function () {
            var entity = parameters.entity;
            vm.mode = parameters.mode;
            vm.imageStrings = [];

            vm.target = {};
            vm.mueble = entity;
            
        };

        vm.init();
 }]);