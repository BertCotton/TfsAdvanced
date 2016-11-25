/*globals angular */
angular.module('TFS.Advanced').service('updateStatusService', ['$http', '$q', '$timeout', function ($http, $q, $timeout) {
    'use strict';

    var cached = [];
    var isRunning = false;
    var isCancelled = false;
    
    this.updateStatuses = function() {
        return cached;
    };

    function updateStatuses() {
        isRunning = true;
        return $http.get('data/UpdateStatus', { cache: false })
            .then(function(response) {
                cached = response.data || [];
                if(!isCancelled)
                    $timeout(updateStatuses, 10000);
                return response;
            });
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