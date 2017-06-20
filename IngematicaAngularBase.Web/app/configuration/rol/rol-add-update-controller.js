angular.module('appBase').controller('rolAddOrUpdateController', ['$q', '$scope', '$state', '$stateParams', 'rolFactory', 'securityFactory', 'blockUI', '$uibModal', 'entityUI', 'parameters',
    function ($q, $scope, $state, $stateParams, rolFactory, securityFactory, blockUI, $uibModal, entityUI, parameters) {

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

        vm.changeModulo = function () {
            if (vm.idModulo == null)
                vm.checkedAll = vm.rol.rolRegla.every(function (y, _, _2) { return y.checked; });
            else
                vm.checkedAll = vm.rol.rolRegla.filter(function (x, _, _2) { return x.idModulo == vm.idModulo; })
                                             .every(function (y, _, _2) { return y.checked; });

            for (var i = 0; i < vm.rol.rolRegla.length; i++) {
                if (vm.rol.rolRegla[i].idModulo == vm.idModulo || vm.idModulo == null)
                    vm.rol.rolRegla[i].visible = true;
                else
                    vm.rol.rolRegla[i].visible = false;
            }
        };

        vm.checkAll = function () {
            for (var i = 0; i < vm.rol.rolRegla.length; i++) {
                if (vm.rol.rolRegla[i].visible)
                    vm.rol.rolRegla[i].checked = vm.checkedAll;
            }
        };

        vm.init = function () {
            var entity = parameters.entity;
            var data = parameters.params;
            vm.mode = parameters.mode;
            vm.idModulo = null;
            vm.moduloList = entityUI.prepareSelectList({ list: data['moduloList'], required: false, nullItem: 'filter' });

            if (vm.mode == 'add')
                vm.setDefaultModel();

            vm.rol = entity;
            vm.checkedAll = true;

            for (var i = 0; i < vm.rol.rolRegla.length; i++) {
                vm.rol.rolRegla[i].visible = true;
                if (!vm.rol.rolRegla[i].checked)
                    vm.checkedAll = false;
            }

            var deferred = $q.defer();
            securityFactory.can(['Auditoria_CanList'])
                .then(function (rlz) {
                    vm.rules = rlz;
                    deferred.resolve(rlz);
                })
                .catch(function (error) {
                    handleErrorService.handleError(error);
                });
        };

        vm.init();
}]);





