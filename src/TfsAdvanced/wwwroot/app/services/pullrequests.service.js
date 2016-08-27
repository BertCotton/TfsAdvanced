/*globals angular */
angular.module('TFS.Advanced').service('pullrequestsService', ['$http', '$q', '$interval', function ($http, $q, $interval) {
    'use strict';

    var cached = undefined;
    var interval = undefined;
    var requesting = false;
    var isLoaded = false;

    this.pullRequest = function () {
        return cached;
    }

    this.isLoaded = function() {
        return isLoaded;
    }

    function requests() {
        var deferred = $q.defer();
        if (!requesting) {
            requesting = true;
            $http.get('data/PullRequests', { cache: false })
                .then(function (response) {
                    cached = response.data || [];
                    deferred.resolve();
                    isLoaded = true;
                    requesting = false;
                });
                
        }
        return deferred.promise;
    }


    this.start = function () {
        interval = $interval(requests, 3000);

    };

    this.stop = function () {
        if (interval)
            interval.cancel();
    }
}]);