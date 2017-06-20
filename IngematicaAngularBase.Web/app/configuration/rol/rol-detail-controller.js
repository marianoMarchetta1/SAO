

angular.module('appBase').controller('rolDetailController', ['$scope', '$state', 'rolFactory','parameters',
    function ($scope, $state, rolFactory, parameters) {

    var vm = this;

    vm.init = function () {
        var entity = parameters.entity;
        vm.rol = entity;
    }

    vm.init();
}]);





