/*globals angular */
angular.module('TFS.Advanced').service('buildsService', ['$http', '$q', '$timeout', function ($http, $q, $timeout) {
    'use strict';

    var cached = [];
    var isLoaded = false;
    var isRunning = false;
    var isCancelled = false;

    this.builds = function() {
        return cached;
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

    this.start = function () {
        isCancelled = false;
        if (!isRunning)
            builds();
        else {
            console.log("Builds Request Service Started Multiple Times.");
        }

    };

    this.stop = function() {
        isCancelled = true;
        isRunning = false;
    };
}]);