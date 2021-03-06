﻿angular.module('appBase').controller('optimizacionHistorialDetailController', ['$scope', '$state', '$stateParams', 'optimizacionHistorialFactory', 'blockUI', '$uibModal',
    'entityUI', 'parameters', 'modalDialogService', 'UploadBase', 'flowFactory',
    function ($scope, $state, $stateParams, optimizacionHistorialFactory, blockUI, $uibModal, entityUI, parameters, modalDialogService, UploadBase, flowFactory) {

        var vm = this;


        vm.init = function () {
            var entity = parameters.entity;
            vm.mode = parameters.mode;
            vm.imageStrings = [];

            vm.target = {};
            vm.optimizacionHistorial = entity;

            for (var i = 0; i < vm.optimizacionHistorial.paths.length; i++) {
                optimizacionHistorialFactory.getBlob(vm.optimizacionHistorial.paths[i], i).then(function (value2) {
                    var blob = new Blob([value2.data], { type: 'application/octet-stream' });
                    var name = vm.optimizacionHistorial.paths[value2.i].substring(vm.optimizacionHistorial.paths[value2.i].length - 23, vm.optimizacionHistorial.paths[value2.i].length);
                    //var name = value.planoArrayList[value2.i].path.substring(8, value.planoArrayList[value2.i].path.length);
                    if (navigator.msSaveBlob)
                        navigator.msSaveBlob(blob, name);
                })
            }

            for (var j = 0; j < vm.optimizacionHistorial.pathsImages.length; j++) {
                optimizacionHistorialFactory.getBlobImage(vm.optimizacionHistorial.pathsImages[j], j).then(function (value3) {
                    //var name = "test.jpg";
                    var name = vm.optimizacionHistorial.pathsImages[value3.j].substring(vm.optimizacionHistorial.pathsImages[value3.j].length - 10, vm.optimizacionHistorial.pathsImages[value3.j].length);

                    var blob = b64toBlob(value3.data.base64, null, null);

                    if (navigator.msSaveBlob)
                        navigator.msSaveBlob(blob, name);
                })
            }

            //if (vm.optimizacionHistorial.paths && vm.optimizacionHistorial.paths.length > 0)
            //    modalDialogService.showModalMessage('Los planos optimizados se han generado con exito y se encuentran en: ' + vm.optimizacionHistorial.paths[0]);
            //else
            //    modalDialogService.showModalFormErrors(["Error al generar los archivos."]);
        };

        function b64toBlob(b64Data, contentType, sliceSize) {
            contentType = contentType || '';
            sliceSize = sliceSize || 512;

            var byteCharacters = atob(b64Data);
            var byteArrays = [];

            for (var offset = 0; offset < byteCharacters.length; offset += sliceSize) {
                var slice = byteCharacters.slice(offset, offset + sliceSize);

                var byteNumbers = new Array(slice.length);
                for (var i = 0; i < slice.length; i++) {
                    byteNumbers[i] = slice.charCodeAt(i);
                }

                var byteArray = new Uint8Array(byteNumbers);

                byteArrays.push(byteArray);
            }

            var blob = new Blob(byteArrays, { type: contentType });
            return blob;
        }

        vm.init();
    }]);