angular.module('TFS.Advanced')
    .controller('BuildStatisticsController',
    [
        '$window', '$scope', 'buildsService',
        function ($window, $scope, buildsService) {
            'use strict';

            $scope.waitTimes = {};
            $scope.maxWaitTime = 0.0;

            $scope.$watchCollection(buildsService.waitTimes,
                function (waitTimes) {
                    for (var i = 0; i < waitTimes.length; i++) {
                        $scope.maxWaitTime = Math.max($scope.maxWaitTime, waitTimes[i].waitingTime);
                    }

                    $scope.waitTimes = waitTimes;
                });

            $scope.getHeight = function(waitTime) {
                var percent = 1 - (($scope.maxWaitTime - waitTime.waitingTime) / $scope.maxWaitTime);
                var height = percent * 100;
                return height+2;
            };

            $scope.getChartWidth = function () {
                return $scope.waitTimes.length * 2;
            };

            $scope.shouldShowClass = function (index, waitTime) {
                if (index % 100 === 0)
                    return "";
                else
                    return "display:none";
            };

            $scope.convertRunTime = function (runTime) {

                var secs = Math.round((runTime) % 60);
                var mins = Math.trunc(((runTime) / 60));
                var time = "";
                if (mins > 0)
                    time += mins + " mins ";
                if (secs > 0)
                    time += secs + " seconds";

                return  time;
            };

        }
    ]);