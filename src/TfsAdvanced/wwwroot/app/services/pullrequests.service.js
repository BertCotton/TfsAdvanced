/*globals angular */
angular.module('TFS.Advanced').service('pullrequestsService', ['$http', '$q', '$interval', function ($http, $q, $interval) {
    'use strict';

    var cachedDeferred = undefined;
    var cachedPromise = undefined;
    var requestDeferred = undefined;
    var cached = undefined;
    var interval = undefined;
    var requesting = false;

    this.get = function () {
        if (cachedDeferred === undefined)
            cachedDeferred = $q.defer();
        
        if (cached) {
            cachedDeferred.resolve(cached);
        }
        else {
            requests()
                .then(function () {
                    console.log("Resolved");
                    cachedDeferred.resolve(cached);
                });
        }
            
        if (cachedPromise == undefined)
            cachedPromise = cachedDeferred.promise;
        return cachedPromise;
    }

    function requests() {
        if (requestDeferred === undefined)
            requestDeferred  = $q.defer();
        if (!requesting) {
            requesting = true;
            $http.get('data/PullRequests', { cache: false })
                .then(function (response) {
                    cached = response.data || [];
                    requesting = false;
                    requestDeferred.resolve(cached);
                    return cached;
                });
                
        }
        return requestDeferred.promise;
    }


    this.start = function () {
        interval = $interval(requests, 3000);

    };

    this.stop = function () {
        if (interval)
            interval.cancel();
    }
}]);