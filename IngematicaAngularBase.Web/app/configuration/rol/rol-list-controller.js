angular.module('appBase').controller('rolListController', ['$scope', 'entityUI', 'frameworkUI', 'rolFactory', 'parameters',
    function ($scope, entityUI, frameworkUI, rolFactory, parameters) {

        var vm = this;

        vm.queryUI = rolFactory.getQueryUI();

        vm.init = function () {
            vm.mode = "list";
            vm.rules = parameters.dataRules;
            if (!vm.queryUI.initialized) vm.setDefaultModel();

            vm.estados = entityUI.createEstadoSelectList();
        };

        vm.delete = function (id) {
            var deleteUI = rolFactory.getDeleteUI();
            deleteUI.id = id;
            deleteUI.execute();
        };

        vm.setDefaultModel = function () {
            vm.queryUI.clear();
            vm.queryUI.query.order = [{ property: 'nombre', descending: false }];
            vm.queryUI.query.activo = null;
            vm.queryUI.initialized = true;
        };

        vm.init();  
}]);




