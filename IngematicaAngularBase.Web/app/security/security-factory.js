
angular.module('appBase').factory('securityFactory', ['$http', '$q', 'authServiceFactory', 'entityUI', 'handleErrorService',
    function ($http, $q, authServiceFactory, entityUI, handleErrorService) {

    var urlBase = '/api/security';
    var securityFactory = {};
    var rules = {};
    var apellidoNombre = {};

    securityFactory.clearRules = function () {
        rules = {};
        apellidoNombre = '';
    }


    securityFactory.getRules = function () {
        return rules;
    }
        
    securityFactory.getApellidoNombre = function () {
        return apellidoNombre;
    }


    securityFactory.setRules = function (rol) {
        for (var i = 0; i < rol.reglas.length; i++) {
            rules[rol.reglas[i]] = true;
        }
        apellidoNombre = rol.apellidoNombre;
    }

    securityFactory.get = function () {

        var deferred = $q.defer();

        if (!entityUI.isEmpty(rules))
        {      
          deferred.resolve(rules);
        }
        else {
            $http.post(urlBase + '/rol')
            .then(function (response) {
                securityFactory.setRules(response.data);
                deferred.resolve(response.data);
            })
            .catch(function (response) {
                deferred.reject(handleErrorService.rejectHttpError(response.data, response.status));
            });
        }
        return deferred.promise;

    };

    securityFactory.existsRule = function (rule) {
        return true;
    }

    securityFactory.existsRule2 = function (rule) {
       
        var result = rules[rule];
        if (!result)
            result = rules['*'];
        return (result === true);
    }

    securityFactory.can = function (ruleList) {
        var deferred = $q.defer();
        securityFactory.get()
        .then(function (response) {
            var obj = {};
            var ruleProp;
            for (var i = 0; i < ruleList.length; i++) {
                ruleProp = ruleList[i].replace('_', '');
                ruleProp = ruleProp.charAt(0).toLowerCase() + ruleProp.slice(1);
                obj[ruleProp] =  securityFactory.existsRule2(ruleList[i]);
            }

            deferred.resolve(obj);
        })
        .catch(function (response) {
            deferred.reject(response);
            //deferred.reject(handleErrorService.rejectHttpError(response.data, response.status));
        });

        return deferred.promise;

    }



    return securityFactory;
}]);