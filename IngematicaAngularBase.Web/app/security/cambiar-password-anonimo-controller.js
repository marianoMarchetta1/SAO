angular.module('appBase').controller('cambiarPasswordAnonimoController', ['$scope', '$state', 'authServiceFactory', 'modalDialogService', 'handleErrorService', 'blockUI', 'parameters', 'constants', '$timeout',
function ($scope, $state, authServiceFactory, modalDialogService, handleErrorService, blockUI, parameters, constants, $timeout) {

        var vm = this;
        vm.applicationName = constants.applicationName;

        vm.validate = function () {
            vm.msg = ''; vm.msgType = '';
            if (vm.passwordEntity.newPassword != vm.passwordEntity.newPasswordRepeat) {
                vm.msg = "Las nuevas contraseñas no coinciden.";
                vm.msgType = 'danger';
            }

            if (vm.passwordEntity.newPassword == vm.passwordEntity.password) {
                vm.msg = "La nueva contraseña es idéntica a la actual.";
                vm.msgType = 'danger';
            }

            if (vm.passwordEntity.newPassword.length < 1) {
                vm.msg = "Debe ingresar un valor como nueva contraseña.";
                vm.msgType = 'danger';
            }
        };

        vm.save = function () {
            vm.validate();

            if (vm.msg == '') {
                blockUI.start();
                return authServiceFactory.cambiarPasswordAnonimo(vm.passwordEntity)
                    .then(function () {
                        vm.msgType = 'success';
                        vm.msg = 'El pasword fue modificado satisfactoriamente. Se redireccionara en: ';
                        vm.startCountdown(5);
                    })
                    .catch(function (error) {
                        vm.msg = error.message;
                        vm.msgType = 'danger';
                    })
                    .finally(function () {
                        blockUI.stop();
                    });
            }
        };

        vm.init = function () {
            vm.menuVisible = true;
            vm.msg = '';
            vm.msgType = '';
            vm.passwordEntity = parameters;
            vm.msg = vm.passwordEntity.message;
            
            if (vm.msg != '') {
                vm.menuVisible = false;
                modalDialogService.showModalMessage(vm.msg)
                .result.then(function () { return authServiceFactory.logOut(); })
            }
        }

        vm.startCountdown = function (time) {
            vm.timerCount = time;

            var countDown = function () {
                if (vm.timerCount < 1) {
                    authServiceFactory.logOut();
                } else {
                    vm.countDownLeft = vm.timerCount;
                    vm.timerCount--;
                    $timeout(countDown, 1000);
                }
            };
            vm.countDownLeft = vm.timerCount;
            countDown();
        }

        vm.init();
    }]);
