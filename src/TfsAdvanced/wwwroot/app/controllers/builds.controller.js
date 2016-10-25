angular.module('TFS.Advanced')
    .controller('BuildsController',
    [
        '$scope', '$filter', 'buildsService', 'NgTableParams',
        function($scope, $filter, buildsService, NgTableParams) {
            'use strict';

            $scope.IsLoaded = true;
            
            $scope.tableParams = new NgTableParams({
                count : 20,
                    sorting: {
                        id: 'desc'
                    }
                },
                {
                    counts: [20, 50, 100],
                    getData: function(params) {
                        var data = buildsService.builds();

                        var filters = params.filter();
                        var newFilters = {};
                        for (var key in filters) {
                            if (filters.hasOwnProperty(key)) {
                                switch (key) {
                                case 'project':
                                    angular.extend(newFilters,
                                    {
                                        project: {
                                            name: filters[key]
                                        }
                                    });
                                    break;
                                case 'definition':
                                    angular.extend(newFilters,
                                    {
                                        definition: {
                                            name: filters[key]
                                        }
                                    });
                                    break;
                                default:
                                    newFilters[key] = filters[key];
                                }
                            }
                        }

                        var filteredData = params.filter() ? $filter('filter')(data, newFilters) : data;

                        var orderedData = params.sorting()
                            ? $filter('orderBy')(filteredData, params.orderBy())
                            : filteredData;
                        var page = orderedData.slice((params.page() - 1) * params.count(),
                            params.page() * params.count());

                        params.total(data.length);
                        return page;
                    }

                });

            
            $scope.$watch(buildsService.isLoaded, function(isLoaded) { $scope.IsLoaded = isLoaded; });

            $scope.$watchCollection(buildsService.builds,
                function() {
                    if ($scope.IsLoaded) {
                        $scope.tableParams.reload();
                    }
                });
        }
    ]);