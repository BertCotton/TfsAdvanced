/*globals angular */
angular.module('TFS.Advanced').service('pullrequestsService', ['$http', '$q', '$interval', function ($http, $q, $interval) {
    'use strict';

    var cached = undefined;
    var interval = undefined;
    var requesting = false;

    this.get = function () {
        var defer = $q.defer();
        if (cached == undefined)
            requests()
                .then(function() {
                    defer.resolve(cached);
                });
        else {
            defer.resolve(cached);
        }
            
        return defer.promise;
    }

    function requests() {
        var defer = $q.defer();
        if (!requesting) {
            requesting = true;
            $http.get('data/PullRequests', { cache: false })
                .then(function(response) {
                        cached = response.data || [];
                        return cached;
                    },
                    function(reason) {
                        cached = [];
                        console.log(reason);
                    })
                .then(function () {
                    requesting = false;
                    return defer.resolve();
                });
        }
        return defer.promise;
    }


    this.start = function () {
        interval = $interval(requests, 3000);

    };

    this.stop = function () {
        if (interval)
            interval.cancel();
    }
}]);