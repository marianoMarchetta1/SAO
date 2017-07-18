

angular.module('appBase').controller('rolDetailController', ['$scope', '$state', 'rolFactory', 'parameters',
    function ($scope, $state, rolFactory, parameters) {

        var vm = this;

        vm.cancel = function () {
            $state.go('app.rol-list');
        }


        vm.init = function () {
            var entity = parameters.entity;
            vm.rol = entity;
        }

        vm.init();
    }]);





