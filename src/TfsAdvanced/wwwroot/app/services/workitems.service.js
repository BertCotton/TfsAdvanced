/*globals angular */
angular.module('TFS.Advanced').service('workItemService', ['$http', '$q', function ($http, $q) {
    'use strict';

    var cached = undefined;

    this.cached = function () {
        var defer = $q.defer();
        if (cached == undefined)
            this.workItems()
                .then(function() {
                    defer.resolve(cached);
                });
        else
            defer.resolve(cached);
        return defer.promise;
    }

    this.workItems = function () {

        return $http.get('data/WorkItem', { cache: false })
            .then(function (response) {
                cached = response.data || [];
                return cached;
            },
                function (reason) {
                    cached = [];
                    console.log(reason);
                });
    }
}]);