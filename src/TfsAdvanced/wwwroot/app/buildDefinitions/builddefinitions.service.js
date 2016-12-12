/*globals angular */
angular.module('TFS.Advanced').service('buildDefinitionService', ['$resource', '$q', '$timeout', function ($resource, $q, $timeout) {
        'use strict';
        var cached = [];
        var isLoaded = false;
        var isRunning = false;
        var isCancelled = false;

        var resource = $resource("data/BuildDefinitions");

        this.buildDefintions = function () {
            return cached;
        };

        this.isLoaded = function() {
            return isLoaded;
        };

        this.startBuild = function() {
            return resource.post(function (data) {
                    return data;
            }).$promise;
        };

        function buildDefintions() {
            isRunning = true;
            return resource.query(function (data) {
                    cached = data || [];
                    isLoaded = true;
                    if (!isCancelled)
                        $timeout(buildDefintions, 10000);
                    return data;
            }, function (error) { console.log(error); }).$promise;
        }

        this.start = function() {
            isCancelled = false;
            if (!isRunning)
                buildDefintions();
            else {
                console.log("Build Defintion Request Service Started Multiple Times.");
            }
        };

        this.stop = function() {
            isCancelled = true;
            isRunning = false;
        };

    }
]);