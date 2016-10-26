/*globals angular */
angular.module('TFS.Advanced').service('buildDefinitionService', ['$http', '$q', '$timeout', function ($http, $q, $timeout) {
        'use strict';
        var cached = [];
        var isLoaded = false;
        var isRunning = false;
        var isCancelled = false;

        this.buildDefintions = function () {
            return cached;
        };

        this.isLoaded = function() {
            return isLoaded;
        };

        this.startBuild = function(data) {
            return $http.post('data/BuildDefinitions', data).then(function(response) {
                    return response.data;
                });
        };

        function buildDefintions() {
            isRunning = true;
            return $http.get('data/BuildDefinitions', { cache: false })
                .then(function(response) {
                    cached = response.data || [];
                    isLoaded = true;
                    if (!isCancelled)
                        $timeout(buildDefintions, 3000);
                    return response;
                });
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