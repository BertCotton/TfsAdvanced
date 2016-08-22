angular.module('TFS.Advanced')
    .controller('WorkItemsController',
    [
        '$scope', '$interval', '$filter', 'DTOptionsBuilder', 'workItemService', function ($scope, $interval, $filter, DTOptionsBuilder, workItemService) {
            'use strict';

            $scope.workItems = [];
            $scope.IsLoading = true;

            $scope.dtOptions = DTOptionsBuilder.newOptions().withOption('order', [1, 'asc']);

            function load() {
                return workItemService.workItems().then(function(data) {
                    $scope.workItems = data;
                    $scope.IsLoading = false;
                });

            };

            load();
        }
    ]);