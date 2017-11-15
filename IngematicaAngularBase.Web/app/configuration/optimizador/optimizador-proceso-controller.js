angular.module('appBase').controller('optimizadorProcesoController', ['$scope', 'entityUI', 'optimizadorFactory', 'parameters', 'modalDialogService', 'UploadBase', '$timeout', 'blockUI', '$q','handleErrorService',
    function ($scope, entityUI, optimizadorFactory, parameters, modalDialogService, UploadBase, $timeout, blockUI, $q, handleErrorService) {

        var vm = this;

        vm.setDefaultModel = function () {
            vm.optimizacion = {};
            vm.optimizacion.registrarEnHistorial = false;
            vm.optimizacion.optimizarCosto = false;
            vm.optimizacion.muebleList = [];
            vm.optimizacion.archivo = null;
            vm.optimizacion.costoMaximo = null;

            vm.idMueble = null;
        };

        vm.init = function () {
            vm.rules = parameters.dataRules;
            var data = parameters.dataParams;

            vm.muebleList = entityUI.prepareSelectList({ list: data['muebleList'], required: false, nullItem: 'addUpdate' });

            vm.setDefaultModel();
            vm.costoActual = 0;
        };

        vm.operationMueble = function (mode, id) {
            if (mode == 'delete') {

                var obj = entityUI.findEntity({ list: vm.optimizacion.muebleList, prop: 'idMueble', propValue: id });
                entityUI.deleteEntity({ list: vm.optimizacion.muebleList, prop: 'idMueble', propValue: id });
                vm.costoActual -= obj.precio * obj.cantidad;
            }
            else {
                if (vm.idMueble == null) {
                    modalDialogService.showModalFormErrors(["Seleccione un mueble."]);
                    return;
                }
                if (vm.cantidad == null || vm.cantidad == 0){
                    modalDialogService.showModalFormErrors(["Ingrese una cantidad mayor a 0."]);
                    return;
                }

                var obj = entityUI.findEntity({ list: vm.optimizacion.muebleList, prop: 'idMueble', propValue: vm.idMueble });
                if (obj) {
                    modalDialogService.showModalFormErrors(["Ya se ingresó el mueble seleccionado."]);
                    return;
                }
                obj = entityUI.findEntity({ list: vm.muebleList, prop: 'id', propValue: vm.idMueble });

                var mueble = {};
                //mueble.idOptimizacionMueble = entityUI.newId({ list: vm.optimizacion.muebleList, prop: 'idMueble' });
                mueble.idMueble = obj.id;
                mueble.cantidad = vm.cantidad;
                mueble.nombre = obj.desc;
                mueble.precio = obj.decimalData;

                vm.optimizacion.muebleList.push(mueble);

                vm.costoActual += obj.decimalData * vm.cantidad;
                vm.idMueble = null;
                vm.cantidad = null;
            }
        };

        vm.upload = function (files) {
            if (files && files.length) {
                vm.showPg = true;
                for (var i = 0; i < files.length; i++) {
                    var file = files[i];
                    vm.log = file.name;
                    UploadBase.upload({
                        url: '/api/optimizador/uploadFile',
                        file: file
                    }).progress(function (evt) {
                        var progressPercentage = parseInt(100.0 * evt.loaded / evt.total);
                        vm.pg = progressPercentage;
                    }).success(function (data, status, headers, config) {
                        $timeout(function () {
                            vm.optimizacion.archivo = config.file;
                            vm.optimizacion.archivo.path = data.returnData;
                        }, 100);
                    })
                    .catch(function (response) {
                        vm.pg = 0;
                        vm.optimizacion.archivo = null;
                        vm.optimizacion.archivo.path = null;
                        handleErrorService.handleError(handleErrorService.rejectHttpError(response.data, response.status));
                    });
                }
            }
        };

        vm.validate = function () {

            if (!vm.optimizacion.nombre || vm.optimizacion.nombre.length == 0) {
                modalDialogService.showModalFormErrors(["Debe ingresar un nombre a esta optimizacion."]);
                return;
            } else if (!vm.optimizacion.archivo) {
                modalDialogService.showModalFormErrors(["Debe ingresar el plano a ser optimizado."]);
                return;
            } else if (entityUI.isEmpty(vm.optimizacion.muebleList)) {
                modalDialogService.showModalFormErrors(["Debe ingresar los muebles."]);
                return;
            } else if (vm.optimizacion.optimizarCosto && vm.costoActual > vm.optimizacion.costoMaximo){
                modalDialogService.showModalFormErrors(["El costo actual supera el limite marcado, le aconsejamos cambiar alguno de los muebles para abaratar costos."]);
                return;           
            }else if (vm.optimizacion.optimizarCosto) {
                 if (!vm.optimizacion.costoMaximo) {
                    modalDialogService.showModalFormErrors(["Debe ingresar el presupuesto máximo."]);
                    return
                }
            } else {

                blockUI.start();
                optimizadorFactory.generate(vm.optimizacion)
                .then(function (value) {
                    
                    for (var i = 0; i < value.planoArrayList.length; i++) {
                        optimizadorFactory.getBlob(value.planoArrayList[i].path, i).then(function (value2) {
                            var blob = new Blob([value2.data], { type: 'application/octet-stream' });
                            var name = value.planoArrayList[value2.i].path.substring(value.planoArrayList[value2.i].path.length - 23, value.planoArrayList[value2.i].path.length);
                            //var name = value.planoArrayList[value2.i].path.substring(8, value.planoArrayList[value2.i].path.length);
                            if (navigator.msSaveBlob)
                                navigator.msSaveBlob(blob, name);
                        })
                    }

                    for (var j = 0; j < value.muebleArray.length; j++) {
                        optimizadorFactory.getBlobImage(value.muebleArray[j].path, j).then(function (value3) {
                            //var name = "test.jpg";
                            var name = value.muebleArray[value3.j].path.substring(value.muebleArray[value3.j].path.length - 10, value.muebleArray[value3.j].path.length);

                            var blob = b64toBlob(value3.data.base64, null, null);

                            if (navigator.msSaveBlob)
                                navigator.msSaveBlob(blob, name);
                        })
                    }
                })
                .catch(function (error) {
                    handleErrorService.handleErrorConfig(error);
                })
                .finally(function () {
                    blockUI.stop();
                });
            }
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
