angular.module('appBase').controller('rolAddOrUpdateController', ['$scope', '$state', '$stateParams', 'rolFactory', 'blockUI', '$uibModal', 'entityUI', 'parameters',
    function ($scope, $state, $stateParams, rolFactory, blockUI, $uibModal, entityUI, parameters) {

        var vm = this;

        vm.add = function () {
            return rolFactory.add(vm.rol);
        };

        vm.update = function () {
            return rolFactory.update(vm.rol);
        };

        vm.setDefaultModel = function (saveAndNew) {
            if (saveAndNew) {
                vm.rol.nombre = null;
                vm.rol.rolRegla.forEach(function (element) {
                    element.checked = false;
                });
            }
            else
                vm.rol = {};
            vm.rol.activo = true;
            vm.rol.interno = false;
        };

        vm.init = function () {
            var entity = parameters.entity;
            vm.mode = parameters.mode;
            if (vm.mode == 'add')
                vm.setDefaultModel();
            vm.rol = entity;
        };

        vm.init();
    }]);





