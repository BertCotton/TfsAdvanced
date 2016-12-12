/*globals angular */
angular.module('TFS.Advanced').service('pullrequestsService', ['$resource', '$q', '$timeout',
    function ($resource, $q, $timeout) {
        'use strict';

        var cached = [];
        var isLoaded = false;
        var isRunning = false;
        var isCancelled = false;

        var resource = $resource('data/PullRequests');

        this.pullRequests = function() {
            return cached;
        };

        this.isLoaded = function() {
            return isLoaded;
        };

        function requests() {
            isRunning = true;
            return resource.query(function (data) {
                    cached = data || [];
                    isLoaded = true;
                    if(!isCancelled)
                        $timeout(requests, 3000);
                    else {
                        isRunning = false;
                    };
            }, function (error) { console.log(error); }).$promise;
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