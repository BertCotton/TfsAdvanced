/*globals angular */
angular.module('TFS.Advanced').service('tasksService', ['$http', '$q', '$timeout', function ($http, $q, $timeout) {
    'use strict';

    var cachedPools = [];
    var cachedJobRequests = [];
    var isRunning = false;
    var isCancelled = false;

    this.pools = function() {
        return cachedPools;
    };

    this.jobRequests = function() {
        return cachedJobRequests;
    }

    function pools() {
        return $http.get('data/Pools', { cache: false })
            .then(function(response) {
                cachedPools = response.data || [];
                if(!isCancelled)
                    $timeout(pools, 100000);
                return response;
            });
    }

    function jobRequests()
    {
        return $http.get("data/JobRequests")
            .then(function (response) {
                cachedJobRequests = response.data;
                $timeout(jobRequests, 2000);
                return response;
            });
    }

    this.start = function () {
        isCancelled = false;
        if (!isRunning) {
            pools();
            jobRequests();
        }
    };

    this.stop = function() {
        isCancelled = true;
        isRunning = false;
    };
}]);