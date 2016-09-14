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
                .then(function(response) {
                    cached = response.data || [];
                    isLoaded = true;
                    if(!isCancelled)
                        $timeout(requests, 3000);
                    return response;
                });
        }


        this.start = function () {
            if (!isRunning)
                requests();
            else
                console.log("Pull Request Service Started multiple times.");

        };

        this.stop = function() {
            isCancelled = true;
            isRunning = false;
        };
    }]);