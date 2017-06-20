angular.module('appBase').directive('orderby', function () {
    return {
        scope:
        {
            field: "@",
            title: "@",
            queryUi: "="
        },
        template: '<a href="" >{{title}}</a>',
        restrict: 'E',
        link: function (scope, el, attrs, querymanagerCtrl) {

            el.on('click', function (e) {
                scope.queryUi.orderby(scope.field)
            });


        }
    };
});


angular.module('appBase').directive('pagechanger', function () {
    return { 
        restrict: 'EA',
        link: function (scope, el, attrs) {


            var a = attrs.pageChanged;
            scope.vm[a] = function ()
            {
                var tmpQuery = angular.copy(scope.vm.state.lastQuery);
                tmpQuery.skip = (scope.vm.state.lastQuery.currentPage - 1) * tmpQuery.take;
                scope.vm.list(tmpQuery);
            }
        }
    };
});



angular.module('appBase').directive('orderbyold', function () {
    return {
        scope:
        {
            field: "@",
            title: "@"
            ,
            query: "&"
        },
        template: '<a href="" >{{title}}</a>',
        restrict: 'E'
        ,
        link: function (scope, el, attrs) {

            el.on('click', function (e) {
                var tmpQuery = angular.copy(scope.$parent.vm.state.lastQuery);
                tmpQuery.skip = 0;
                if (tmpQuery.order[0].property == scope.field) {
                    tmpQuery.order[0].descending = !tmpQuery.order[0].descending;
                }
                else {
                    tmpQuery.order[0].property = scope.field;
                    tmpQuery.order[0].descending = false;
                }

                scope.query({ query: tmpQuery });

            });


        }
    };
});




angular.module('appBase').directive('focusMe', ['$timeout', function ($timeout) {
    return {
        link: function (scope, element, attr) {
            attr.$observe('focusMe', function (value) {
                if (value === "true") {
                    $timeout(function () {
                        element[0].focus();
                        if (element[0].type == 'text')
                        {
                            var len = element[0].value.length * 2;
                            element[0].setSelectionRange(len, len);
                        }

                    }, 200);
                }
            });
        }
    };
}]);


angular.module('appBase').directive('validNumber2', function () {
    return {
        require: '?ngModel',
        scope: { numericType2: '@' },
        link: function (scope, element, attrs, ngModelCtrl) {
            element[0].maxLength = 5
            if (!ngModelCtrl) {
                return;
            }

            ngModelCtrl.$parsers.push(function (val) {
                if (angular.isUndefined(val)) {
                    var val = '';
                }

                var clean = val.replace(/[^-0-9\,]/g, '');
                if (clean != val)
                {
                    ngModelCtrl.$setViewValue(ngModelCtrl.$modelValue);
                    ngModelCtrl.$render();
                    return ngModelCtrl.$modelValue;
                }
                    
                var negativeCheck = clean.split('-');
                var decimalCheck = clean.split(',');
                if (!angular.isUndefined(negativeCheck[1])) {
                    negativeCheck[1] = negativeCheck[1].slice(0, negativeCheck[1].length);
                    clean = negativeCheck[0] + '-' + negativeCheck[1];
                    if (negativeCheck[0].length > 0) {
                        clean = negativeCheck[0];
                    }

                }

                if (!angular.isUndefined(decimalCheck[1])) {
                    decimalCheck[1] = decimalCheck[1].slice(0, 2);
                    clean = decimalCheck[0] + ',' + decimalCheck[1];
                }

                if (val !== clean) {
                    ngModelCtrl.$setViewValue(clean);
                    ngModelCtrl.$render();
                }
                return clean;
            });

            element.bind('keypress', function (event) {
                if (event.keyCode === 32) {
                    event.preventDefault();
                }
            });
        }
    };
});


angular.module('appBase').directive('validateNumeric', function () {
    return {
        // restrict to an attribute type.
        restrict: 'A',
        scope: { numericType: '@', precision: '@', allowNegative: '@'},
        // element must have ng-model attribute.
        require: 'ngModel',

        // scope = the parent scope
        // elem = the element the directive is on
        // attr = a dictionary of attributes on the element
        // ctrl = the controller for ngModel.
        link: function (scope, elem, attr, ctrl) {
            var re;
            var re2;
            
            var integerNum;
            var decimalNum;    
            var neg = '';
            if(attr.allowNegative)
                neg = '-?';
            if (attr.numericType && scope.numericType == 'decimal') {
                if (attr.precision) {
                    var originalNum = scope.precision.split(",");
                    integerNum = originalNum[0];
                    decimalNum = originalNum[1];
                }
                else {
                    integerNum = 18;
                    decimalNum = 4;
                }
          
                re = '^' + neg + '[0-9]{1,' + integerNum.toString() + '}(?:\\,[0-9]{1,' + decimalNum.toString() + '})?$';
                re2 = '^' + neg + '[0-9]{1,' + integerNum.toString() + '}(?:\\.[0-9]{1,' + decimalNum.toString() + '})?$';
            }
            else {
                re = '^' + neg + '[0-9]*$';
            }

            // create the regex obj.
            var regex = new RegExp(re);
            var regex2 = new RegExp(re2);
            // add a parser that will process each time the value is 
            // parsed into the model when the user updates it.
            ctrl.$parsers.unshift(function (value) {
                // test and set the validity after update.
                var valid = true;
                if (value != '' && value != null && value != undefined)
                    valid = regex.test(value);               

                if (valid) {
                    if (!(attr.numericType && (scope.numericType == 'decimal' || scope.numericType == 'int64') )) {
                        var a = parseInt(value);
                        if (a && a > 2147483647 || a < -2147483647)
                            valid = false
                    }
                }

                ctrl.$setValidity('validateNumeric', valid);

                // if it's valid, return the value to the model, 
                // otherwise return undefined.
                return valid ? value.replace(',', '.') : '';
            });

            // add a formatter that will process each time the value 
            // is updated on the DOM element.
            ctrl.$formatters.unshift(function (value) {
                // validate.
                var valid = true;
                if (value != '' && value != null && value != undefined)
                    valid = regex2.test(value);

                if (valid) {
                    if (!(attr.numericType && (scope.numericType == 'decimal' || scope.numericType == 'int64') )) {
                        var a = parseInt(value);
                        if (a && a > 2147483647 || a < -2147483647)
                            valid = false
                    }
                }

                ctrl.$setValidity('validateNumeric', valid);

                // return the value or nothing will be written to the DOM.

                var val = value;
                if (value != '' && value != null && value != undefined)
                    val = val.replace('.', ',')
   
                return val;
                
            });
        }
    };
});

angular.module('appBase').directive('validateEmail', function () {
    return {
        // restrict to an attribute type.
        restrict: 'A',

        // element must have ng-model attribute.
        require: 'ngModel',

        // scope = the parent scope
        // elem = the element the directive is on
        // attr = a dictionary of attributes on the element
        // ctrl = the controller for ngModel.
        link: function (scope, elem, attr, ctrl) {

            var re = /^([\w-]+(?:\.[\w-]+)*)@((?:[\w-]+\.)*\w[\w-]{0,66})\.([a-z]{2,6}(?:\.[a-z]{2})?)$/i;;

            // create the regex obj.
            var regex = new RegExp(re);

            // add a parser that will process each time the value is 
            // parsed into the model when the user updates it.
            ctrl.$parsers.unshift(function (value) {
                // test and set the validity after update.
                var valid = true;
                if (value != '' && value != null && value != undefined)                    
                    valid = regex.test(value);

                ctrl.$setValidity('validateEmail', valid);

                // if it's valid, return the value to the model, 
                // otherwise return undefined.
                return valid ? value : undefined;
            });

            // add a formatter that will process each time the value 
            // is updated on the DOM element.
            ctrl.$formatters.unshift(function (value) {
                // validate.
                var valid = true;
                if (value != '' && value != null && value != undefined)
                    valid = regex.test(value);

                ctrl.$setValidity('validateEmail', valid);

                // return the value or nothing will be written to the DOM.
                return value;
            });

            //scope.$parent.$watch(attr.ngModel, function () {

            //    /* When the model is touched before */
            //    if (ctrl.$touched) {
            //        /* When the model is not dirty, Set ngModel to dirty by re-applying its content */
            //        if (!ctrl.$dirty) {
            //            ctrl.$setViewValue(ctrl.$modelValue);
            //        }
            //    }

            //    /* When the model has a value */
            //    if (ctrl.$modelValue) {
            //        /* When the model is not touched before */
            //        if (!ctrl.$touched) {
            //            ctrl.$setTouched();
            //        }
            //    }

            //    return ctrl;
            //});

        }
    };
});


angular.module('appBase').directive('validateCuit', function () {

    function validateCuit(cuit) {
        var re = /^[0-9]*$/;
        var regex = new RegExp(re);

        if (cuit == '' || cuit == null || cuit == undefined || cuit.length != 11 || !regex.test(cuit))
        {
            return false;
        }
        else {
            var mult = [5, 4, 3, 2, 7, 6, 5, 4, 3, 2];
            var total = 0;
            for (var i = 0; i < mult.length; i++) {
                total += parseInt(cuit[i]) * mult[i];
            }
            var mod = total % 11;
            var digito = mod == 0 ? 0 : mod == 1 ? 9 : 11 - mod;
        }
        return digito == parseInt(cuit[10]);
    }

    return {
        // restrict to an attribute type.
        restrict: 'A',
        scope: { validateCuitIf: '&validateCuitIf', val: '=', validateCuitIfWatch: '@validateCuitIfWatch',  validateNumericNocuit: '@'},
        // element must have ng-model attribute.
        require: 'ngModel',

        // scope = the parent scope
        // elem = the element the directive is on
        // attr = a dictionary of attributes on the element
        // ctrl = the controller for ngModel.
        link: function (scope, elem, attr, ctrl) {

            var re = /^[0-9]*$/;
            var regex = new RegExp(re);

            var validateCuitIf = attr.validateCuitIf;
            //var comparisonModel = attr.cuitTipo;
            // add a parser that will process each time the value is 
            // parsed into the model when the user updates it.
            ctrl.$parsers.unshift(function (value) {
                // test and set the validity after update.
                var valid = true;
                if (scope.validateCuitIf()) {
                    var valid = validateCuit(value);
                    ctrl.$setValidity('validateCuit', valid);
                }
                else {
                    var validNumeric = true;
                    if (attr.validateNumericNocuit && value != '' && value != null && value != undefined)
                        validNumeric = regex.test(value);

                    ctrl.$setValidity('validateCuit', validNumeric);
                }

                return valid ? value : '';
            });


            if (attr.validateCuitIfWatch) {
                scope.$parent.$watch(scope.validateCuitIfWatch, function () {
                    if (scope.validateCuitIf())
                    {
                        ctrl.$setValidity('validateCuit', validateCuit(ctrl.$viewValue));
                    }
                    else {
                        var validNumeric = true;
                        if (attr.validateNumericNocuit && value != '' && value != null && value != undefined)
                            validNumeric = regex.test(value);

                        ctrl.$setValidity('validateCuit', validNumeric);
                    }
                    //ctrl.$setViewValue(ctrl.$viewValue);
                });
            }
            // add a formatter that will process each time the value 
            // is updated on the DOM element.
            ctrl.$formatters.unshift(function (value) {
                // validate.
                if (scope.validateCuitIf())
                {
                    ctrl.$setValidity('validateCuit', validateCuit(value));
                }
                else {
                    var validNumeric = true;
                    if (attr.validateNumericNocuit && value != '' && value != null && value != undefined)
                        validNumeric = regex.test(value);

                    ctrl.$setValidity('validateCuit', validNumeric);
                }

                // return the value or nothing will be written to the DOM.
                return value;
            });


        }
    };
});


//angular.module('appBase').directive('validateCuit', function () {

//    function validateCuit(cuit) {
//        if (typeof (cuit) == 'undefined')
//            return true;
//        cuit = cuit.toString().replace(/[-_]/g, "");
//        if (cuit == '')
//            return false;
//        if (cuit.length != 11)
//            return false;
//        else {
//            var mult = [5, 4, 3, 2, 7, 6, 5, 4, 3, 2];
//            var total = 0;
//            for (var i = 0; i < mult.length; i++) {
//                total += parseInt(cuit[i]) * mult[i];
//            }
//            var mod = total % 11;
//            var digito = mod == 0 ? 0 : mod == 1 ? 9 : 11 - mod;
//        }
//        return digito == parseInt(cuit[10]);
//    }

//    return {
//        // restrict to an attribute type.
//        restrict: 'A',
//        scope: { validateCuitIf: '&validateCuitIf', val: '='},
//        // element must have ng-model attribute.
//        require: 'ngModel',

//        // scope = the parent scope
//        // elem = the element the directive is on
//        // attr = a dictionary of attributes on the element
//        // ctrl = the controller for ngModel.
//        link: function (scope, elem, attr, ctrl) {

//            var validateCuitIf = attr.validateCuitIf;
//            //var comparisonModel = attr.cuitTipo;
//            // add a parser that will process each time the value is 
//            // parsed into the model when the user updates it.
//            ctrl.$parsers.unshift(function (value) {
//                // test and set the validity after update.
//                var valid = true;
//                if (scope.validateCuitIf()) {
//                    var valid = validateCuit(value);
//                    ctrl.$setValidity('validateCuit', valid);
//                }
//                else {
//                    ctrl.$setValidity('validateCuit', true);;
//                }

//                return valid ? value : '';
//            });

//            scope.$parent.$watch('vm.cliente.idActorDocumentoTipo', function () {
//                if (scope.validateCuitIf())
//                    ctrl.$setValidity('validateCuit', validateCuit(ctrl.$viewValue));
//                else
//                    ctrl.$setValidity('validateCuit', true);
//                //ctrl.$setViewValue(ctrl.$viewValue);
//            });

//            // add a formatter that will process each time the value 
//            // is updated on the DOM element.
//            ctrl.$formatters.unshift(function (value) {
//                // validate.
//                if (scope.validateCuitIf())
//                    ctrl.$setValidity('validateCuit', validateCuit(value));
//                else
//                    ctrl.$setValidity('validateCuit', true);

//                // return the value or nothing will be written to the DOM.
//                return value;
//            });

          
//        }
//    };
//});

angular.module('appBase').directive('selectRequired2', [ function () {
        return {
            require: 'ngModel',
            link: function (scope, elem, attr, ngModel) {

                // Create validators
                ngModel.$validators.selectRequired = function (modelValue, viewValue) {
                    /* When no value is present */
                   // debugger;
                    var isValid;
                    if (!modelValue || modelValue.length === 0) {
                        isValid = false;
                    } else {
                        isValid = true;
                    }

                    /* Set required validator as this is not a standard html form input */
                    ngModel.$setValidity('required', isValid);

                    /* Set custom validators */
                    ngModel.$setValidity('selectRequired', isValid);

                    /* Return the model so the model is updated in the view */
                    return modelValue;
                };

                // Watch for changes to the model
                scope.$watch(attr.ngModel, function () {

                    /* When the model is touched before */
                    if (ngModel.$touched) {
                        /* When the model is not dirty, Set ngModel to dirty by re-applying its content */
                        if (!ngModel.$dirty) {
                            ngModel.$setViewValue(ngModel.$modelValue);
                        }
                    }

                    /* When the model has a value */
                    if (ngModel.$modelValue) {
                        /* When the model is not touched before */
                        if (!ngModel.$touched) {
                            ngModel.$setTouched();
                        }
                    }

                    return ngModel;
                });

                // Validate on creation
                ngModel.$validate();
            }
        }
    }
]);


angular.module('appBase').directive('selectRequired', [function () {
    return {
        require: 'ngModel',
        scope: { validateIf: '&validateIf', validateIfModel: '@' },
        link: function (scope, elem, attr, ngModel) {

            // Create validators
            ngModel.$validators.selectRequired = function (modelValue, viewValue) {
                /* When no value is present */
                // debugger;

                var isValid;

                if (!attr.validateIf || (attr.validateIf && scope.validateIf())) {
                    if ((!modelValue || modelValue.length === 0))
                        isValid = false;
                    else
                        isValid = true;
                }
                else {
                    isValid = true;
                }
               

                ngModel.$setValidity('required', isValid);
                ngModel.$setValidity('selectRequired', isValid);

                /* Return the model so the model is updated in the view */
                return modelValue;
            };

            if (attr.validateIf) {
                scope.$parent.$watch(attr.validateIfModel, function () {

                    var isValid;
                    if (!attr.validateIf || (attr.validateIf && scope.validateIf())) {
                        if ((!ngModel.$viewValue || ngModel.$viewValue.length === 0))
                            isValid = false;
                        else
                            isValid = true;
                    }
                    else {
                        isValid = true;
                    }

                    ngModel.$setValidity('required', isValid);
                    ngModel.$setValidity('selectRequired', isValid);

                    //ctrl.$setViewValue(ctrl.$viewValue);
                });
            }

            // Watch for changes to the model
            scope.$parent.$watch(attr.ngModel, function () {

                /* When the model is touched before */
                if (ngModel.$touched) {
                    /* When the model is not dirty, Set ngModel to dirty by re-applying its content */
                    if (!ngModel.$dirty) {
                        ngModel.$setViewValue(ngModel.$modelValue);
                    }
                }

                /* When the model has a value */
                if (ngModel.$modelValue) {
                    /* When the model is not touched before */
                    if (!ngModel.$touched) {
                        ngModel.$setTouched();
                    }
                }

                return ngModel;
            });

            // Validate on creation
            ngModel.$validate();
        }
    }
}
]);

//angular.module('appBase').directive('menu', ['$compile', '$document', '$timeout',
//    function ($compile, $document, $timeout) {

//    //var hideSubMenus = function (scope, clickedItem) {
//    //    angular.forEach(scope.menu, function (item, key) {
//    //        if (item.submenu && item.showSubMenu && item !== clickedItem) {
//    //            $timeout(function () {
//    //                item.showSubMenu = false;
//    //            });
//    //        }
//    //    });
//    //};

//    return {
//        restrict: 'E',
//        replace: true,
//        scope: {
//            css: '@class',
//            menu: '='
//        },
//        template: '<ul ng-class="css">' +
//                    '<li ng-repeat="item in menu" ' +
//                        'ng-class="{showSubMenu:item.showSubMenu}">' +
//                      '<a href="" ng-href="{{item.submenu ? \'\' : item.path}}">' +
//                        '{{item.title}}' +
//                        '<i ng-if="item.submenu" class="fa fa-angle-right"></i>' +
//                      '</a>' +
//                      '<menu ng-if="item.showSubMenu" menu="item.submenu"></menu>' +
//                    '</li>' +
//                  '</ul>',
//        compile: function (tElement) {
//            var contents = tElement.contents().remove();
//            var compiled;
//            return {
//                pre: function preLink(scope, iElement) {

//                    //scope.toggleSubMenu = function (s) {
//                    //    if (!s.item.submenu) { return; }
//                    //    hideSubMenus(scope, s.item);
//                    //    s.item.showSubMenu = !s.item.showSubMenu;
//                    //};

//                    // hide all submenus if click is elsewhere
//                    //$document.bind('click', function (event) {
//                    //    var isClickedMenuElementChild = iElement.find(event.target).length > 0;

//                    //    if (isClickedMenuElementChild) { return; }
//                    //    hideSubMenus(scope);
//                    //});

//                },
//                post: function postLink(scope, iElement) {
//                    if (!compiled) {
//                        compiled = $compile(contents);
//                    }
//                    compiled(scope, function (clone) {
//                        iElement.append(clone);
//                    });
//                }
//            };
//        }
//    };
//}]);