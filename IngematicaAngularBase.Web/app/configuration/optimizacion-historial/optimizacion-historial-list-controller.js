angular.module('appBase').controller('optimizacionHistorialListController', ['$scope', 'entityUI', 'optimizacionHistorialFactory', 'parameters',
    function ($scope, entityUI, optimizacionHistorialFactory, parameters) {

        var vm = this;
        vm.queryUI = optimizacionHistorialFactory.getQueryUI();

        vm.init = function () {
            vm.mode = parameters.mode;
            vm.rules = parameters.dataRules;
            if (!vm.queryUI.initialized) vm.setDefaultModel();
        }

        vm.setDefaultModel = function () {
            vm.queryUI.clear();
            vm.queryUI.query.order = [{ property: 'nombre', descending: false }];
            vm.queryUI.query.nombre = null;
            vm.queryUI.initialized = true;
        }

        vm.delete = function (id) {
            var deleteUI = optimizacionHistorialFactory.getDeleteUI();
            deleteUI.id = id;
            deleteUI.execute();
        }

        vm.init();
    }]);
