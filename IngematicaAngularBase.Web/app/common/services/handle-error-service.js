angular.module('appBase').service('handleErrorService',  ['modalDialogService', function (modalDialogService) {
    this.rejectHttpError = function (data, status) {

        if (data) {
            var errorsList = new Array();
            angular.forEach(data.applicationErrors, function (value, key) {
                for (var i = 0; i < value.length; i++) {

                    errorsList.push(value[i]);
                }
            });

            var result = {
                errorType: status,
                message: data.message,
                errors: errorsList,
                managedException: data.managedException
            }

            return result;
        }
    };

    this.handleErrorForm = function (clientError) {
        if (clientError.managedException)
            if (clientError.errors && clientError.errors.length > 0) {
                modalDialogService.showModalFormErrors(clientError.errors);
            }
            else {
                var errs = [];
                errs.push(clientError.message)
                modalDialogService.showModalFormErrors(errs);
            }
        else
            this.noManagedException(clientError);
    };


    this.handleErrorConfig = function (clientError) {
        if (clientError.managedException)
            if (clientError.errors && clientError.errors.length > 0) {
                modalDialogService.showModalMessage(clientError.errors[0]);
            }
            else {
                modalDialogService.showModalMessage(clientError.message);
            }
        else
            this.noManagedException(clientError);
    };

    this.handleError = function (clientError) {
        if (clientError.managedException)
            if (clientError.errors && clientError.errors.length > 0) {
                modalDialogService.showModalMessage(clientError.errors[0]);
            }
            else {
                modalDialogService.showModalMessage(clientError.message);
            }
        else
            this.noManagedException(clientError);
    };

    this.noManagedException = function (clientError) {
        switch (clientError.errorType) {
            case 500: // Internal Server Error
                modalDialogService.showModalMessage(clientError.message);
                break;
            case 403: // Forbidden
                modalDialogService.showModalMessage(this.getErrorMessage(clientError.errorType));
                break;
            default:
                modalDialogService.showModalMessage(clientError.message);
                break;
        }
    };

    this.getErrorMessage = function (errorType) {
        switch (errorType) {
            case 403:
                return 'No tiene los permisos necesarios para acceder.';
            default:
                return '';
        }
    };

}]);