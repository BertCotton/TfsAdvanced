angular.module('TFS.Advanced')
    .controller('BuildsController',
    [
        '$scope', '$filter', 'buildsService', 'NgTableParams',
        function($scope, $filter, buildsService, NgTableParams) {
            'use strict';

            var groupState = {};
            $scope.IsLoaded = true;
            $scope.groupExpanded = {};

            $scope.tableParams = new NgTableParams({
                    sorting: {
                        id: 'desc'
                    },
                    group: 'definition.name'
                },
                {
                    counts: [],
                    showGroupPanel: false,
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

                        params.count(orderedData.length);
                        params.total(orderedData.length);
                        return page;
                    }
                });

            $scope.toggleGroup = function(group) {
                groupState[group.value] = !group.$hideRows;
                group.$hideRows = !group.$hideRows;
            }

            $scope.groupState = function(group) {
                if (groupState[group.value] === undefined)
                    group.$hideRows = true;
                else
                    group.$hideRows = groupState[group.value];
            };

            $scope.$watch(buildsService.isLoaded, function(isLoaded) { $scope.IsLoaded = isLoaded; });

            $scope.$watchCollection(buildsService.builds,
                function() {
                    if ($scope.IsLoaded) {
                        $scope.tableParams.reload();
                    }
                });

            $scope.getLatestBuild = function(buildDefinition) {
                return $filter('orderBy')(buildDefinition.data, "id", true)[0];
            };
        }
    ]);