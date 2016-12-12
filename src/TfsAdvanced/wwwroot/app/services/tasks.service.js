/*globals angular */
angular.module('TFS.Advanced').service('tasksService', ['$resource', '$q', '$timeout', function ($resource, $q, $timeout) {
    'use strict';

    var cachedPools = [];
    var cachedJobRequests = [];
    var isRunning = false;
    var isCancelled = false;

    var poolsResource = $resource("data/Pools");
    var jobRequestsResource = $resource("data/Jobrequests");

    this.pools = function() {
        return cachedPools;
    };

    this.jobRequests = function() {
        return cachedJobRequests;
    }

    function pools() {
        return poolsResource.query(function (data) {
                cachedPools = data || [];
                if(!isCancelled)
                    $timeout(pools, 100000);
                return data;
        }, function (error) { console.log(error); }).$promise;
    }

    function jobRequests()
    {
        return jobRequestsResource.query(function (data) {
                cachedJobRequests = data;
                $timeout(jobRequests, 2000);
                return data;
        }, function (error) { console.log(error); }).$promise;
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