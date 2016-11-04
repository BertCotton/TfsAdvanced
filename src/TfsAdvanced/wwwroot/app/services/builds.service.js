/*globals angular */
angular.module('TFS.Advanced').service('buildsService', ['$http', '$q', '$timeout', function ($http, $q, $timeout) {
    'use strict';

    var cached = [];
    var cachedWaitTimes = [];
    var isLoaded = false;
    var isRunning = false;
    var isCancelled = false;

    this.builds = function() {
        return cached;
    };

    this.waitTimes = function() {
        return cachedWaitTimes;
    };

    this.isLoaded = function() {
        return isLoaded;
    };

    function builds() {
        isRunning = true;
        return $http.get('data/Builds', { cache: false })
            .then(function(response) {
                cached = response.data || [];
                isLoaded = true;
                if(!isCancelled)
                    $timeout(builds, 3000);
                return response;
            });
    }

    function waitTimes()
    {
        return $http.get("data/Builds/WaitTimes")
            .then(function (response) {
                cachedWaitTimes = response.data;
                $timeout(waitTimes, 3000);
                return response;
            });
    }

    this.start = function () {
        isCancelled = false;
        if (!isRunning) {
            builds();
            waitTimes();
        } else {
            console.log("Builds Request Service Started Multiple Times.");
        }
    };

    this.stop = function() {
        isCancelled = true;
        isRunning = false;
    };
}]);