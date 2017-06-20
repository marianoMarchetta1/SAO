

angular.module('appBase').controller('usuarioDetailController', ['$scope', '$state','usuarioFactory','parameters',
    function ($scope, $state, usuarioFactory, parameters) {

    var vm = this;

    vm.init = function () {
        var entity = parameters.entity;
        vm.usuario = entity;
    }

    vm.init();

}]);





