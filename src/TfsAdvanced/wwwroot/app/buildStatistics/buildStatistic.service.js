/*globals angular */
angular.module('TFS.Advanced').service('buildStatisticService', ['$http', '$q', '$timeout', function ($http, $q, $timeout) {
    'use strict';

    var cachedStatistics = [];
    var isLoaded = false;
    var isRunning = false;
    var isCancelled = false;
    var daysBackStatistics = 7;
    var statisticsTimeout = undefined;

    this.statistics = function() {
        return cachedStatistics;
    };

    this.isLoaded = function() {
        return isLoaded;
    };

    this.daysBack = function (daysBack) {
        daysBackStatistics = daysBack;
        if (statisticsTimeout) 
            $timeout.cancel(statisticsTimeout);
        getStatistics();
        
    };

    function getStatistics() {
        return $http.get("data/Builds/Statistics?NumberOfDaysBack=" + daysBackStatistics)
            .then(function (response) {
                cachedStatistics = response.data;
                statisticsTimeout = $timeout(getStatistics, 10000);
                return response;
            });
    }

    this.start = function () {
        isCancelled = false;
        if (!isRunning) {
            getStatistics();
        } 
    };

    this.stop = function() {
        isCancelled = true;
        isRunning = false;
    };
}]);