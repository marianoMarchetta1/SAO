

angular.module('appBase').controller('loginController', ['$scope', '$rootScope', '$state', '$http', 'authServiceFactory', 'securityFactory', 'blockUI', 'handleErrorService','modalDialogService','constants',
function ($scope, $rootScope, $state, $http, authServiceFactory, securityFactory, blockUI, handleErrorService, modalDialogService, constants) {
    var vm = this;

    vm.loginData = {
        userName: "",
        password: "",
        msg: "",
        useRefreshTokens: true
    };

    vm.applicationName = constants.applicationName;
    vm.message = "";

    vm.enviarPassword = function () {
        if (vm.loginData.userName == '' || vm.loginData.userName == null || vm.loginData.userName == undefined) {
            vm.loginData.msg = 'Indique el nombre de usuario.';
            return;
        }
        var myBlockUI = blockUI.instances.get('myBlockUI');
        myBlockUI.start();
        return authServiceFactory.enviarPassword(vm.loginData.userName)
            .then(function () {
                vm.loginData.msgType = 'success';
                vm.loginData.msg = 'Se ha enviado un correo de recuperación a su casilla';
            })
            .catch(function (error) {
                vm.loginData.msgType = 'danger';
                if (error.managedException)
                    vm.loginData.msg = error.message;
                else
                    vm.loginData.msg = 'Ha ocurrido un error con el servidor, por favor reintente mas tarde.';
            })
            .finally(function () {
                myBlockUI.stop();
            });        
    };

    vm.login = function () {


        if (vm.loginData.userName == '' || vm.loginData.password == '')
        {
            vm.loginData.msgType = 'danger';
            vm.loginData.msg = 'El usuario o la contraseña son requeridos.';
            return;
        }

        var myBlockUI = blockUI.instances.get('myBlockUI');
        myBlockUI.start();
        authServiceFactory.login(vm.loginData)
        .then(function (response) {
                $state.go('app.home');
        })
        .catch(function (error) {

            
            vm.loginData.msgType = 'danger';

            if (error) {
                if (error.error == 'invalid_grant')
                    vm.loginData.msg = 'El usuario o la contraseña es incorrecto.';
                else if (error.error == 'internal_error' || error.error == 'invalid_clientId')
                    vm.loginData.msg = 'Ha ocurrido un error con el servidor, por favor reintente mas tarde.';
                else
                    vm.loginData.msg = 'Ha ocurrido un error con el servidor, por favor reintente mas tarde.';
            }
            else {
                vm.loginData.msg = 'Ha ocurrido un error con el servidor, por favor reintente mas tarde.';
            }
        })
        .finally(function () {
            myBlockUI.stop();
        });

    };

}]);