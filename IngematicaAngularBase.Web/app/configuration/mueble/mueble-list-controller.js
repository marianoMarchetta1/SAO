angular.module('appBase').controller('muebleListController', ['$scope', 'entityUI', 'muebleFactory', 'parameters',
    function ($scope, entityUI, muebleFactory, parameters) {

        var vm = this;
        vm.queryUI = muebleFactory.getQueryUI();

        vm.init = function () {
            vm.mode = parameters.mode;
            vm.rules = parameters.dataRules;
            if (!vm.queryUI.initialized) vm.setDefaultModel();
        }

        vm.setDefaultModel = function () {
            vm.queryUI.clear();
            vm.queryUI.query.order = [{ property: 'nombre', descending: false }, { Property: 'activo', descending: false }];
            vm.queryUI.query.activo = null;
            vm.queryUI.query.nombre = null;
            vm.queryUI.initialized = true;
        }

        vm.delete = function (id) {
            var deleteUI = muebleFactory.getDeleteUI();
            deleteUI.id = id;
            deleteUI.execute();
        }

        vm.init();
    }]);
