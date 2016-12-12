/*globals angular */
angular.module('TFS.Advanced').service('buildStatisticService', ['$resource', '$q', '$timeout', function ($resource, $q, $timeout) {
    'use strict';

    var cachedStatistics = [];
    var isLoaded = false;
    var isRunning = false;
    var isCancelled = false;
    var daysBackStatistics = 7;
    var statisticsTimeout = undefined;

    var resource = $resource("/data/builds/Statistics?NumberOfDaysBack=:daysBackStatistics");

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
        return resource.query({ daysBackStatistics: daysBackStatistics }, function (data) {
                cachedStatistics = data;
                statisticsTimeout = $timeout(getStatistics, 10000);
                return data;
        }, function (error) { console.log(error); }).$promise;
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