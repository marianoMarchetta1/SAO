angular.module('appBase').controller('usuarioListController', ['$scope', 'entityUI', 'frameworkUI', 'usuarioFactory', 'parameters',
    function ($scope, entityUI, frameworkUI, usuarioFactory, parameters) {

    var vm = this;
    vm.queryUI = usuarioFactory.getQueryUI();

    vm.init = function () {
        vm.mode = "list";
        vm.rules = parameters.dataRules;

        if (!vm.queryUI.initialized) vm.setDefaultModel();

        var data = parameters.dataParams;
        vm.roles = entityUI.prepareSelectList({ list: data['rolList'], required: false, nullItem: 'filter' });
        vm.estados = entityUI.createEstadoSelectList();
    };
       
    vm.delete = function (id) {
        var deleteUI = usuarioFactory.getDeleteUI();
        deleteUI.id = id;
        deleteUI.execute();
    };
    
    vm.setDefaultModel = function () {
        vm.queryUI.clear();
        vm.queryUI.query.order = [{ property: 'nombre', descending: false }];
        vm.queryUI.query.idRol = entityUI.getSelectItemOrDefault({ list: vm.roles, itemId: vm.queryUI.query.idRol, required: false });
        vm.queryUI.query.activo = null;
        vm.queryUI.initialized = true;
    };

    vm.init();

}]);




