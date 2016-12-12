/*globals angular */
angular.module('TFS.Advanced').service('buildsService', ['$resource', '$q', '$timeout', function ($resource, $q, $timeout) {
    'use strict';

    var cached = [];
    var isLoaded = false;
    var isRunning = false;
    var isCancelled = false;

    var resource = $resource("data/Builds");
    
    this.builds = function() {
        return cached;
    };

    this.isLoaded = function() {
        return isLoaded;
    };

    function builds() {
        isRunning = true;
        return resource.query(function (data) {
                cached = data || [];
                isLoaded = true;
                if(!isCancelled)
                    $timeout(builds, 10000);
                return data;
        }, function (error) { console.log(error); }).$promise;
    }

    this.start = function () {
        isCancelled = false;
        if (!isRunning) {
            builds();
        }
    };

    this.stop = function() {
        isCancelled = true;
        isRunning = false;
    };
}]);