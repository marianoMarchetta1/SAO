angular.module('appBase').controller('muebleAddOrUpdateController', ['$scope', '$state', '$stateParams', 'muebleFactory', 'blockUI', '$uibModal',
    'entityUI', 'parameters', 'modalDialogService', 'UploadBase','flowFactory',
    function ($scope, $state, $stateParams, muebleFactory, blockUI, $uibModal, entityUI, parameters, modalDialogService, UploadBase, flowFactory) {

        var vm = this;    

        vm.add = function () {
            return muebleFactory.add(vm.mueble);                
        };

        vm.update = function () {
            return muebleFactory.update(vm.mueble);
        };

        vm.validate = function () {
            if (!vm.mueble.imagen) {
                modalDialogService.showModalFormErrors(["Debe ingresar una imagen."]);
                return false;
            } else {
                return true;
            }

            //TODO: Validar, si tiene radio que esten ingresados, sino que esten ingresados largo y ancho
        };

        vm.setDefaultModel = function () {
            vm.mueble = {};
            vm.mueble.activo = true;
            vm.mueble.poseeRadio = false;
            vm.mueble.imagen = {};
        };

        vm.init = function () {
            var entity = parameters.entity;
            vm.mode = parameters.mode; 
            vm.imageStrings = [];

            vm.target = {};

            if (vm.mode == 'add')
                vm.setDefaultModel();
            else if (vm.mode == 'update') {
                vm.mueble = entity;
            }
        };


        vm.imageStrings = [];
        vm.processFiles = function (files) {
            angular.forEach(files, function (flowFile, i) {
               var fileReader = new FileReader();
                fileReader.onload = function (event) {
                    var uri = event.target.result;
                    vm.imageStrings[i] = uri;
                    vm.mueble.imagen = uri;
                    
                };
               fileReader.readAsDataURL(flowFile.file);
           });
        };

        vm.init();
    }]);






		//document.getElementById("file-field").onchange = function() {
  //          var reader = new FileReader();

  //          reader.onload = function (e) {
  //              var fileText = e.target.result;
  //              var parser = new DxfParser();
  //              var dxf = null;
  //              try {
  //                  dxf = parser.parseSync(fileText);
  //                  outputElement.innerHTML = JSON.stringify(dxf, null, 4);
  //              } catch (err) {
  //                  return console.error(err.stack);
  //              }
  //          };

  //          var outputElement = document.getElementById('output');
  //          var csvFileInput = document.getElementById('file-field');
  //          var csvFile = csvFileInput.files[0];

  //          reader.readAsText(csvFile);


		//};