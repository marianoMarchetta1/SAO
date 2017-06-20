angular.module('appBase').factory('httpBuffer', ['$injector', function($injector) {

    var httpBuffer = {};
    var buffer = [];
    var doAction = false;
    var $http;
    
    function retryHttpRequest(config, deferred) {
        function successCallback(response) {
            deferred.resolve(response);
        }
        function errorCallback(response) {
            deferred.reject(response);
        }
        $http = $http || $injector.get('$http');
        $http(config).then(successCallback, errorCallback);
    }


    httpBuffer.getItems = function() {
        return buffer;
    }
 
    httpBuffer.append = function(config, deferred) {
        buffer.push({
            config: config,
            deferred: deferred
        });
    }

    httpBuffer.rejectAll = function(reason) {
        if (reason) {
            for (var i = 0; i < buffer.length; ++i) {
                buffer[i].deferred.reject(reason);
            }
        }
        buffer = [];
    }

    httpBuffer.clear = function () {
        buffer = [];
        doAction = false;
    }


    httpBuffer.retryAll = function() {
        if (!doAction) {
            doAction = true;
            for (var i = 0; i < buffer.length; ++i) {
                retryHttpRequest(buffer[i].config, buffer[i].deferred);
            }
            httpBuffer.clear();
        }
    }

    return httpBuffer;
}]);