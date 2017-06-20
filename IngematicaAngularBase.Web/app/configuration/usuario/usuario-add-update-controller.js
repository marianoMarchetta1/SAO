angular.module('appBase').controller('usuarioAddOrUpdateController', ['$scope', '$state', '$stateParams', 'usuarioFactory', 'blockUI', '$uibModal', 'entityUI', 'parameters',
    function ($scope, $state, $stateParams, usuarioFactory, blockUI, $uibModal, entityUI, parameters) {

        var vm = this;
    
        vm.add = function () {
            return usuarioFactory.add(vm.usuario);
        };

        vm.update = function () {
            return usuarioFactory.update(vm.usuario);
        };

        vm.setDefaultModel = function () {
            vm.usuario = {};
            vm.usuario.idRol = entityUI.getSelectItemOrDefault({ list: vm.roles, itemId: vm.usuario.idRol, required: false });
            vm.usuario.activo = true;
            vm.usuario.interno = false;
        };

        vm.init = function () {
            var params = parameters.params;
            var entity = parameters.entity;
            vm.mode = parameters.mode;

            vm.roles = entityUI.prepareSelectList({ list: params['rolList'], required: false, itemId: entity.idRol, itemDesc: entity.rol, nullItem: 'addUpdate' });
            if (vm.mode == 'add')
                vm.setDefaultModel();
            else if (vm.mode == 'update')
                vm.usuario = entity;
        };

        vm.init();
}]);





