angular.module('appBase').controller('optimizadorProcesoController', ['$scope', 'entityUI', 'optimizadorFactory', 'parameters', 'modalDialogService', 'UploadBase', '$timeout', 'blockUI', '$q','handleErrorService',
    function ($scope, entityUI, optimizadorFactory, parameters, modalDialogService, UploadBase, $timeout, blockUI, $q, handleErrorService) {

        var vm = this;

        vm.setDefaultModel = function () {
            vm.optimizacion = {};
            vm.optimizacion.registrarEnHistorial = false;
            vm.optimizacion.optimizarCosto = false;
            vm.optimizacion.muebleList = [];
            vm.optimizacion.archivo = null;

            vm.idMueble = null;
        };

        vm.init = function () {
            vm.rules = parameters.dataRules;
            var data = parameters.dataParams;

            vm.muebleList = entityUI.prepareSelectList({ list: data['muebleList'], required: false, nullItem: 'addUpdate' });

            vm.setDefaultModel();
        };

        vm.operationMueble = function (mode, id) {
            if (mode == 'delete') {
                entityUI.deleteEntity({ list: vm.optimizacion.muebleList, prop: 'idMueble', propValue: id });
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

                vm.optimizacion.muebleList.push(mueble);

                vm.idMueble = null;
                vm.cantidad == null;
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
            } else if (entityUI.isEmpty(vm.optimizacion.muebleList)){
                modalDialogService.showModalFormErrors(["Debe ingresar los muebles."]);
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
                    var a;
                    a = 0; //cargar el path resultado en una variable para mostrarlo
                })
                .catch(function (error) {
                    handleErrorService.handleErrorConfig(error);
                })
                .finally(function () {
                    blockUI.stop();
                });
            }
        };

        vm.init();
    }]);
