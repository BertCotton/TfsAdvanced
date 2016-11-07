angular.module('TFS.Advanced')
    .controller('BuildStatisticsController',
    [
        '$window', '$scope', '$filter', 'buildsService',
        function ($window, $scope, $filter, buildsService) {
            'use strict';

            $scope.statistics = [];
            $scope.maxWaitTime = 0;
            $scope.maxRunTime = 0;
            $scope.waitTimeLabels = [];
            $scope.waitTimeSeries = [];
            $scope.waitTimes = [];
            $scope.data = {};

            $scope.options = {
                chart: {
                    type: "boxPlotChart",
                    height: 450,
                    maxBoxWidth : 75,
                    margin: {
                        top: 20,
                        right: 20,
                        bottom: 100,
                        left: 100
                    },
                    x: function (d) {
                        return $filter('date')(d.label);
                    },
                    yDomain: [0, $scope.maxWaitTime],
                    xAxis: {
                        axisLabel: "",
                        rotateLabels: -45
                    },
                    yAxis: {
                        axisLabel: "Queue Time"
                    }
                }
            };
            

            $scope.$watchCollection(buildsService.statistics,
                function (statistics) {
                    $scope.statistics = statistics;
                    var waitTimes = [];
                    var waitTimeLabels = [];


                    $scope.maxWaitTime = 0;
                    for(var index in statistics)
                    {
                        var stat = statistics[index];
                        $scope.maxWaitTime = Math.max($scope.maxWaitTime, stat.queueTimeMax);
                        $scope.maxRunTime = Math.max($scope.maxRunTime, stat.runTimeMax);
                        waitTimes.push({
                            label: new Date(stat.day),
                            values: {
                                Q1: stat.queueTimesLowerPercentile,
                                Q2: stat.queueTimeAverage,
                                Q3: stat.queueTimesUpperPercentile,
                                whisker_low: stat.queueTimeMin,
                                whisker_high: stat.queueTimeMax,
                                outliers: [stat.queueTimeAverage]
                            }
                        });
                    
                        waitTimeLabels.push(new Date(stat.day));
                    }
                    $scope.options.chart.yDomain = [0, $scope.maxWaitTime];
                    $scope.data = waitTimes;
                    $scope.waitTimeLabels = waitTimeLabels;
                });

            
            
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

        }]);