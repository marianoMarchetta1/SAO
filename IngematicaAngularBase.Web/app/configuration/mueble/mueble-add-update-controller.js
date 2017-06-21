angular.module('appBase').controller('muebleAddOrUpdateController', ['$scope', '$state', '$stateParams', 'muebleFactory', 'blockUI', '$uibModal', 'entityUI', 'parameters',
    function ($scope, $state, $stateParams, muebleFactory, blockUI, $uibModal, entityUI, parameters) {

        var vm = this;

        vm.add = function () {
            return muebleFactory.add(vm.mueble);
        };

        vm.update = function () {
            return muebleFactory.update(vm.mueble);
        };

        vm.setDefaultModel = function () {
            vm.mueble = {};
            vm.mueble.activo = true;
        };

        vm.init = function () {
            var entity = parameters.entity;
            vm.mode = parameters.mode; 

            if (vm.mode == 'add')
                vm.setDefaultModel();
            else if (vm.mode == 'update')
                vm.mueble = entity;
        };

        vm.init();
    }]);





