angular.module('appBase').controller('optimizadorProcesoController', ['$scope', 'optimizadorFactory', 'entityUI', 'parameters',
    function ($scope, optimizadorFactory, entityUI, parameters) {

        var vm = this;

        vm.setDefaultModel = function () {
            vm.optimizacion = {};
            vm.optimizacion.registrarEnHistorial = false;
            vm.optimizacion.optimizarCosto = false;
            vm.optimizacion.muebleList = [];
        };

        vm.init = function () {
            vm.rules = parameters.dataRules;
            var data = parameters.dataParams;

            vm.muebles = entityUI.prepareSelectList({ list: data['muebleList'], required: false, nullItem: 'addUpdate' });

            vm.setDefaultModel();
        };

        vm.init();
    }]);





