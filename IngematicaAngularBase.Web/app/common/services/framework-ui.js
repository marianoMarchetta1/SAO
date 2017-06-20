angular.module('appBase').service('frameworkUI', ['$http', '$q', 'entityUI', 'handleErrorService', 'blockUI', 'modalDialogService',
   function ($http, $q, entityUI, handleErrorService, blockUI, modalDialogService) {

       this.createDeleteUI = function (deleteCommand, queryUI) {
           var obj = {};
           obj.deleteCommand = deleteCommand;
           obj.queryUI = queryUI;

           obj.execute = function () {
               modalDialogService.showModalCanDelete()
                .result.then(function () {
                    blockUI.start();
                    obj.deleteCommand.id = obj.id;
                    obj.deleteCommand.execute()
                    .then(function () {
                        return obj.queryUI.refresh();
                    })
                    .catch(function (error) {
                        handleErrorService.handleError(error);
                    })
                    .finally(function () {
                        blockUI.stop();
                    });
                });
           }
           return obj;
       }

       this.createDatetimePicker = function () {
           var obj = {};
           obj.opened = false;
           obj.dateOptions = { formatYear: 'yy', startingDay: 1 };
           obj.open = function ($event) {
               this.opened = true;
           }
           return obj;
       }



       this.createQueryUI = function (command) {

           var obj = { list: {}, lastQuery: {} };
           obj.takeList = [20, 50, 100];
           obj.defaultTake = 20;
           var defaultQuery = { skip: 0, take: obj.defaultTake, currentPage: 1 };
           obj.query = angular.copy(defaultQuery);
           obj.command = command;
           obj.initialized = false;

           obj.clear = function () {
               obj.query = angular.copy(defaultQuery);
               obj.query.take = obj.defaultTake;
               obj.list = {};
               obj.lastQuery = {};
               obj.initialized = false;
           }

           obj.isListEmpty = function () {
               return entityUI.isEmpty(this.list) || this.list.data.length == 0;
           }

           obj.isCleared = function () {
               return (this.list && this.list.data && this.list.data.length == 0);
           }

           obj.isLastQueryDirty = function () {
               return (!entityUI.isEmpty(queryUI.lastQuery));
           }

           obj.sucess = function (list, query, trigger) {
               if (query)
                   this.lastQuery = angular.copy(query);
               else
                   this.lastQuery = angular.copy(this.query);
               this.list = list;
               if (this.lastQuery.skip == 0)
                   this.lastQuery.currentPage = 1;
               if (this.onSuccess)
                   this.onSuccess(trigger);
           }

           obj.orderby = function (field) {
               var tmpQuery = angular.copy(this.lastQuery);
               tmpQuery.skip = 0;;

               if (tmpQuery.order[0].property == field) {
                   tmpQuery.order[0].descending = !tmpQuery.order[0].descending;
               }
               else {
                   tmpQuery.order[0].property = field;
                   tmpQuery.order[0].descending = false;
               }

               this.command.query = tmpQuery;
               this.adapter(this.command.execute(), 'orderby');
           }

           obj.pageChanged = function () {
               var tmpQuery = angular.copy(this.lastQuery);
               tmpQuery.skip = (this.lastQuery.currentPage - 1) * tmpQuery.take;
               this.command.query = tmpQuery;
               this.adapter(this.command.execute(), 'pageChanged');
           }

           obj.refresh = function () {
               var deferred = $q.defer();

               if (entityUI.isEmpty(this.lastQuery)) {
                   deferred.resolve(null);
                   return deferred.promise;
               }
               blockUI.start();
               this.lastQuery.take = this.query.take;
               this.command.query = this.lastQuery;
               this.adapter(this.command.execute(), 'refresh').then(function (data) {
                   deferred.resolve(data);
               })
               .catch(function (error) {
                   deferred.reject(error);
               })
               .finally(function () {
                   blockUI.stop();
               });

               return deferred.promise;
           }

           obj.search = function () {
               this.command.query = this.query;
               blockUI.start();
               this.adapter(this.command.execute(), 'search')
               .catch(function (error) {
                   handleErrorService.handleError(error);
               })
               .finally(function () {
                   blockUI.stop();
               });
           }

           obj.searchQuery = function () {
               this.command.query = this.query;
               return this.adapter(this.command.execute(), 'search')
           }

           obj.adapter = function (queryPromise, trigger) {
               var deferred = $q.defer();
               var obj2 = this;
               queryPromise.then(function (data) {
                   obj2.sucess(data.list, data.query, trigger);
                   deferred.resolve();
               })
               .catch(function (error) {
                   deferred.reject(error);
               });

               return deferred.promise;
           }

           return obj;
       }






   }]);

angular.module('appBase').directive('broadcast', function () {
    return {
        scope:
        {
            broadcast: "@",
            broadcastArg: "@",
        },
        restrict: 'A',
        link: function (scope, el, attrs) {

            el.on('click', function (e) {
                scope.$parent.$broadcast(scope.broadcast, scope.broadcastArg);
            });


        }
    };
});


angular.module('appBase').directive('goState', ['$state', function ($state) {
    return {
        scope:
        {
            goState: "@",
        },
        restrict: 'A',
        link: function (scope, el, attrs) {

            el.on('click', function (e) {
                $state.go(scope.goState);
            });


        }
    };
}]);