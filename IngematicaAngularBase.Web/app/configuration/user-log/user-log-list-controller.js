angular.module('appBase').controller('userLogListController', ['$scope', 'entityUI', 'userLogFactory', 'parameters',
    function ($scope, entityUI, userLogFactory, parameters) {

        var vm = this;
        vm.queryUI = userLogFactory.getQueryUI();

        vm.init = function () {
            vm.mode = parameters.mode;
            vm.rules = parameters.dataRules;
            if (!vm.queryUI.initialized) vm.setDefaultModel();
        }

        vm.setDefaultModel = function () {
            vm.queryUI.clear();
            vm.queryUI.query.order = [{ property: 'usuario', descending: false }];
            vm.queryUI.query.usuario = null;
            vm.queryUI.initialized = true;
        }

        vm.delete = function (id) {
            var deleteUI = userLogFactory.getDeleteUI();
            deleteUI.id = id;
            deleteUI.execute();
        }

        vm.init();
    }]);
