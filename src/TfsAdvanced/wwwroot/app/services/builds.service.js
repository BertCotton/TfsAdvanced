/*globals angular */
angular.module('TFS.Advanced').service('buildsService', ['$http', '$q', '$interval', function ($http, $q, $interval) {
    'use strict';

    var cached = undefined;
    var interval = undefined;
    var requesting = false;

    this.get = function () {
        var defer = $q.defer();
        if (cached == undefined)
            builds()
                .then(function () {
                    defer.resolve(cached);
                });
        else
            defer.resolve(cached);
        return defer.promise;
    }

    function builds() {
        var defer = $q.defer();
        if (!requesting) {
            requesting = true;
            $http.get('data/Builds', { cache: false })
                .then(function(response) {
                        cached = response.data || [];
                        return cached;
                    },
                    function(reason) {
                        cached = [];
                        console.log(reason);
                    })
                .then(function() {
                    requesting = false;
                    defer.resolve();
                });
        }
        return defer.promise;
    }

    this.start = function () {
        interval = $interval(builds, 3000);

    };

    this.stop = function() {
        if(interval)
            interval.cancel();
    }
}]);