angular.module('TFS.Advanced')
    .controller('UpdateStatusController',
    [
        '$window', '$scope', '$filter', 'NgTableParams', 'updateStatusService',
        function($window, $scope, $filter, NgTableParams, updateStatusService) {
            'use strict';

            $scope.updateStatuses = [];

            $scope.tableParams = new NgTableParams({
                    count: 20,
                    sorting: {
                        updaterName: 'asc'
                    }
                },
                {
                    counts: [20, 50, 100],
                    getData: function(params) {
                        var data = updateStatusService.updateStatuses();

                        var orderedData = params.sorting()
                            ? $filter('orderBy')(data, params.orderBy())
                            : data;
                        
                        var page = orderedData.slice((params.page() - 1) * params.count(),
                            params.page() * params.count());

                        params.total(data.length);
                        return page;
                    }

                });

            $scope.$watchCollection(updateStatusService.updateStatuses,
                function (data) {
                    $scope.tableParams.reload();
                });
        }
    ]);