/*globals angular */
angular.module('TFS.Advanced').service('updateStatusService', ['$resource', '$q', '$timeout', function ($resource, $q, $timeout) {
    'use strict';

    var cached = [];
    var isRunning = false;
    var isCancelled = false;

    var resource = $resource("/data/UpdateStatus");
    
    this.updateStatuses = function() {
        return cached;
    };

    function updateStatuses() {
        isRunning = true;
        return resource.query(function(data) {
                cached = data || [];
                if(!isCancelled)
                    $timeout(updateStatuses, 10000);
                return data;
        }, function (error) { console.log(error); }).$promise;
    }

    this.start = function () {
        isCancelled = false;
        if (!isRunning) {
            updateStatuses();
        } 
    };

    this.stop = function() {
        isCancelled = true;
        isRunning = false;
    };
}]);