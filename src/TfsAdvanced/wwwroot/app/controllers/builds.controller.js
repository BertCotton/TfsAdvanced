angular.module('TFS.Advanced')
    .controller('BuildsController',
    [
        '$scope', '$interval', '$filter', 'DTOptionsBuilder', 'buildsService', function ($scope, $interval, $filter, DTOptionsBuilder, buildsService) {
            'use strict';

            $scope.builds = [];
            $scope.IsLoading = true;

            $scope.dtOptions = DTOptionsBuilder.newOptions().withOption('order', [0, 'desc']);

            function load() {
                buildsService.get()
                    .then(function(data) {
                        $scope.builds = data;
                        $scope.IsLoading = false;
                    });


            };

            load();
            $interval(load, 1000);
        }
    ]);