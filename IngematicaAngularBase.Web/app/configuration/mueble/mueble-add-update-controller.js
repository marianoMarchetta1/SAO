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

        vm.upload = function () {

           
        };

		
		document.getElementById("file-field").onchange = function() {
            var reader = new FileReader();

            reader.onload = function (e) {
                var fileText = e.target.result;
                var parser = new DxfParser();
                var dxf = null;
                try {
                    dxf = parser.parseSync(fileText);
                    //outputElement.innerHTML = JSON.stringify(dxf, null, 4);
                } catch (err) {
                    return console.error(err.stack);
                }
            };

            var outputElement = document.getElementById('output');
            var csvFileInput = document.getElementById('file-field');
            var csvFile = csvFileInput.files[0];

            reader.readAsText(csvFile);
            

		};

        vm.init();
    }]);





