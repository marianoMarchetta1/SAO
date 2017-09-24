'use strict';
angular.module('appBase').controller('headerController', ['$scope', '$location', '$state', 'authServiceFactory', 'securityFactory','constants',
    function ($scope, $location, $state, authServiceFactory, securityFactory, constants) {

    function createMenu() {
        var menuList = {};
        function menu() {
            this.items = [];
            this.parent = null;
            this.visible = false;
            this.canRule = function (rule) {
                return rule == '*' ? true : securityFactory.existsRule2(rule);
            };
            this.back = function () {
                return this.parent;
            }; 
            this.addItem = function (id, title, rule, action) {

                var item = new menu();
                item.id = id;
                item.title = title;
                item.rule = rule;
                item.parent = this;
                item.action = action;
                if (rule && item.canRule(rule)) {
                    item.visible = true;
                    this.visible = true;
                }

                this.items.push(item);
                menuList[id] = item;
                return item;
            };
        };

        var obj = new menu();
        obj.elements = menuList;
        return obj;
    }


    $scope.init = function () {
        $scope.clientName = constants.clientName;
        $scope.authentication = authServiceFactory.authentication;
        $scope.initMenu();
    }


    $scope.initMenu = function () {

        securityFactory.get()
        .then(function () {
            $scope.apellidoNombre = securityFactory.getApellidoNombre();
            $scope.menu = createMenu();
            $scope.menu
                .addItem("optimizador", "Optimizador", "")
                .addItem("optimizador", "Optimizador", "Optimizador_CanAdd", "app.optimizador-proceso").back()
                .addItem("optimizacionHistorial", "Historial De Optimizaciòn", "OptimizacionHistorial_CanList", "app.optimizacionHistorial-list").back()
                .back()
            .addItem("configuracion", "Configuracion", "")
                .addItem("mueble", "Mueble", "Mueble_CanList", "app.mueble-list").back()
            .back()
            .addItem("seguridad", "Seguridad", "")
                .addItem("usuarios", "Usuarios", "Usuario_CanList", "app.usuario-list").back()
                .addItem("roles", "Roles", "Rol_CanList", "app.rol-list").back()
                .addItem("cambiarPassword", "Cambiar Password", "*", "app.cambiar-password").back();
        });


    }


    $scope.logOut = function () {
        authServiceFactory.logOut();
        $state.go('appan.login');
    }


    $scope.routeTo = function (url) {
        $state.go(url);
    }

    $scope.init();
}]);