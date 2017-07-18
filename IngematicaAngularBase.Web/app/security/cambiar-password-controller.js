angular.module('appBase').controller('cambiarPasswordController', ['$scope', '$state', 'authServiceFactory','modalDialogService','handleErrorService','blockUI',
function ($scope, $state, authServiceFactory, modalDialogService, handleErrorService, blockUI) {

        var vm = this;

        vm.validate = function () {
            if (vm.passwordEntity.newPassword != vm.passwordEntity.newPasswordRepeat) {
                modalDialogService.showModalFormErrors(["Las nuevas contraseñas no coinciden."]);
                return false;
            }

            if (vm.passwordEntity.newPassword == vm.passwordEntity.password) {
                modalDialogService.showModalFormErrors(["La nueva contraseña es idéntica a la actual."]);
                return false;
            }

            return true;
        };

        vm.save = function () {
            blockUI.start();
            return authServiceFactory.cambiarPassword(vm.passwordEntity)
                .then(function () {
                    modalDialogService.showModalMessage('La operación se ha realizado con éxito.')
                        .result.then(function () { return authServiceFactory.logOut(); })
                })
                .catch(function (error) {
                    handleErrorService.handleError(error);
                })
                .finally(function () {
                    blockUI.stop();
                });
        };

        vm.init = function () {
            vm.passwordEntity = {};
        }

        vm.init();
    }]);
