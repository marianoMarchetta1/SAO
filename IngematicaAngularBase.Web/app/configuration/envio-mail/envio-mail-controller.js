angular.module('appBase').controller('envioMailController', ['$scope', 'entityUI', 'envioMailFactory', 'parameters', 'blockUI','modalDialogService',
    function ($scope, entityUI, envioMailFactory, parameters, blockUI, modalDialogService) {

        var vm = this;

        vm.init = function () {
            vm.mode = parameters.mode;
            vm.rules = parameters.dataRules;
            vm.mail = {};
        }

        vm.send = function(){
            //validar length del comentario
                blockUI.start();
                envioMailFactory.send(vm.mail)
                .then(function (value) {
                    if (value && value.list && value.list == "1"){
                        modalDialogService.showModalMessage('El mail ha sido enviado exitosamente.');
                        vm.mail.nombre = '';
                        vm.mail.comentario = '';
                        }
                        else
                        modalDialogService.showModalFormErrors(["Error al enviar el mail."]);
                })
                .catch(function (error) {
                    handleErrorService.handleErrorConfig(error);
                })
                .finally(function () {
                    blockUI.stop();
                });    
        }
            
        vm.init();
    }]);
