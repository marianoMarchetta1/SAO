angular.module('appBase').directive('customSubmit', ['$timeout', 'blockUI', 'handleErrorService', '$state', 'modalDialogService',
function ($timeout, blockUI, handleErrorService, $state, modalDialogService) {
    return {
        restrict: 'A',
        scope: {
            validate: '&validate',
            add: '&add',
            update: '&update',
            list: '&list',
            mode: '=',
            customSubmit: '&customSubmit',
            onSubmit: '@',
            returnState: '@',
            returnStateParam: '=',
            saveAndNew: '=',
            setDefaultModel: '&setDefaultModel',
            onFinishedSubmit: '&onFinishedSubmit'
        },
        link: function (scope, element, attributes) {
            
            var obj = {};
            obj.clear = function () {
                var form = scope.$parent.$eval(attributes.name);
                if (attributes.setDefaultModel)
                    scope.setDefaultModel();
                form.$setPristine();
                $timeout(function () {
                    element[0][0].focus();
                });

            }

            obj.submit = function () {

                var form = scope.$parent.$eval(attributes.name);

                ['input', 'textarea', 'select'].forEach(function (e) {
                    element.find(e).removeClass('ng-pristine');
                });


                var errors = new Array();
                angular.forEach(form, function (formElement, fieldName) {
                    // If the fieldname starts with a '$' sign, it means it's an Angular
                    // property or function. Skip those items.
                    if (fieldName[0] === '$') return;
                  
                                        

                    if (formElement.$error) {

                        var fieldMsg = fieldName;
                        //var message;
                        //var ctrl = document.querySelectorAll('[name=' + fieldName + ']');
                        //var lblAttr = ctrl[0].getAttribute("lblMessage");                        
                        //if (lblAttr) {
                        //    ctrl = document.querySelectorAll('[name=' + lblAttr + ']');
                        //    if (ctrl.length > 0)
                        //        fieldMsg = ctrl[0].innerHTML;
                        //}

                        if (formElement.$error.required === true)
                            errors.push("El campo '" + fieldMsg + "' es requerido.")
                        if (formElement.$error.maxlength === true)
                            errors.push("El campo '" + fieldMsg + "' excede el tamaño permitido.")

                        if (!(formElement.$error.required === true) && formElement.$error.validateEmail === true)
                            errors.push("El campo '" + fieldMsg + "' no es válido.")

                        if (!(formElement.$error.required === true) && formElement.$error.validateNumeric === true)
                            errors.push("El campo '" + fieldMsg + "' no es válido.")

                        if (!(formElement.$error.required === true) && formElement.$error.validateCuit === true)
                            errors.push("El campo '" + fieldMsg + "' no es válido.")

                        if (!(formElement.$error.required === true) && formElement.$error.date === true)
                            errors.push("El campo '" + fieldMsg + "' no es válido.")

                        if (formElement.$error.editable === true && formElement.$error.parse === true)
                            errors.push("El campo '" + fieldMsg + "' no es válido.")
                    }
                    formElement.$pristine = false;
                    formElement.$dirty = true;
                });

                //  scope.$parent.$apply();

                // Do not continue if the form is invalid.
                if (form.$invalid) {

                    var modalInstance = modalDialogService.showModalFormErrors(errors);

                    modalInstance.result.then(function () {
                        //Timeout ??
                        element[0].getElementsByClassName('ng-invalid')[0].focus();
                    });
                    return false;
                }


                if (attributes.validate && !scope.validate())
                    return false;


                if (attributes.add && scope.mode == 'add') {

                    blockUI.start();
                    scope.add().then(function (data) {
                        blockUI.stop();
                        modalDialogService.showModalMessage('La operación se ha realizado con éxito.')
                        .result.then(function () {


                            if (attributes.saveAndNew && scope.saveAndNew) {
                                if (attributes.setDefaultModel)
                                    scope.setDefaultModel();
                                form.$setPristine();
                                element[0][0].focus();
                            }
                            else {
                                if (attributes.onFinishedSubmit)
                                    scope.onFinishedSubmit();
                                if (attributes.returnState && !attributes.returnStateParam)
                                    $state.go(scope.returnState);
                                else if (attributes.returnState && attributes.returnStateParam)
                                    $state.go(scope.returnState, scope.returnStateParam);
                            }
                        });

                    })
                    .catch(function (clientError) {
                        blockUI.stop();
                        handleErrorService.handleErrorForm(clientError);
                    });

                }
                else if (attributes.update && scope.mode == 'update') {
                    blockUI.start();
                    scope.update().then(function (data) {
                        blockUI.stop();
                        modalDialogService.showModalMessage('La operación se ha realizado con éxito.')
                        .result.then(function () {
                            if (attributes.onFinishedSubmit)
                                scope.onFinishedSubmit();
                            if (attributes.returnState && !attributes.returnStateParam)
                                $state.go(scope.returnState);
                            else if (attributes.returnState && attributes.returnStateParam)
                                $state.go(scope.returnState, scope.returnStateParam);
                        });

                    })
                    .catch(function (clientError) {
                        blockUI.stop();
                        handleErrorService.handleErrorForm(clientError);
                    });

                }
                else if (attributes.list && scope.mode == 'list') {
                    scope.list();

                }
                else if (attributes.customSubmit) {
                    scope.customSubmit();
                }



            }

            scope.$parent.$on(attributes.name, function (event, args) {
                switch (args) {
                    case 'clear':
                        obj.clear();
                        break;
                    case 'submit':
                        obj.submit();
                        break;
                }
                
            });
            
            scope.$parent.$on(attributes.onSubmit, function () {
                obj.submit();
            });

        }
    };
}]);