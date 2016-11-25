/*globals angular */
angular.module('TFS.Advanced').service('pullrequestsService', ['$http', '$q', '$timeout',
    function ($http, $q, $timeout) {
        'use strict';

        var cached = [];
        var isLoaded = false;
        var isRunning = false;
        var isCancelled = false;

        this.pullRequests = function() {
            return cached;
        };

        this.isLoaded = function() {
            return isLoaded;
        };

        function requests() {
            isRunning = true;
            return $http.get('data/PullRequests', { cache: false })
                .success(function(data) {
                    cached = data || [];
                    isLoaded = true;
                    if(!isCancelled)
                        $timeout(requests, 3000);
                    else {
                        isRunning = false;
                    };
                }).error(function(error, status) {
                    isRunning = false;
                });
        }


        this.start = function () {
            if (!isRunning)
                requests();
        };

        this.stop = function() {
            isCancelled = true;
            isRunning = false;
        };
    }]);