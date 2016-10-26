angular.module('TFS.Advanced')
    .controller('BuildDefinitionController',
    [
        '$window', '$scope', '$location', '$interval', '$notification', '$filter', 'NgTableParams', 'buildDefinitionService',
        function ($window, $scope, $location, $interval, $notification, $filter, NgTableParams, buildDefinitionService) {
            'use strict';

            $scope.IsLaunching = false;
            $scope.buildDefinitions = [];
            $scope.selectedDefinitions = [];
            
            $scope.tableParams = new NgTableParams({
                count: 20,
                page: 1,
                sorting: {
                    id: 'name'
                }
            },
               {
                   counts: [20,50,100],
                   getData: function (params) {
                       var data = buildDefinitionService.buildDefintions();

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
                                   default:
                                       newFilters[key] = filters[key];
                               }
                           }
                       }
                       var filteredData = params.filter() ? $filter('filter')(data, newFilters) : data;
                       $scope.buildDefinitions = data;
                       var orderedData = params.sorting()
                           ? $filter('orderBy')(filteredData, params.orderBy())
                           : filteredData;
                       var page = orderedData.slice((params.page() - 1) * params.count(),
                           params.page() * params.count());

                       params.total(data.length);
                       return page;
                   }
               });

            $scope.$watch(buildDefinitionService.isLoaded, function (isLoaded) { $scope.IsLoaded = isLoaded; });

            $scope.$watchCollection(buildDefinitionService.buildDefintions,
                function () {
                    if ($scope.IsLoaded && $scope.buildDefinitions.length === 0) {
                        $scope.tableParams.reload();
                    }
                });


            $scope.noneChecked = function () {
                return $filter('filter')($scope.selectedDefinitions, function (def) {
                    return def !== undefined;
                }).length === 0;
            };

            $scope.toggle = function () {
                if ($scope.noneChecked()) {
                    $scope.buildDefinitions.forEach(function (def) {
                        $scope.selectedDefinitions[def.id] = def;
                    });
                } else {
                    $scope.selectedDefinitions = [];
                }
            };

            $scope.isSelected = function(def) {
                return $scope.selectedDefinitions[def.id] !== undefined;
            };

            $scope.select = function(def) {
                if ($scope.isSelected(def))
                    $scope.selectedDefinitions[def.id] = undefined;
                else
                    $scope.selectedDefinitions[def.id] = def.id;
            };

            $scope.queueDefinitions = function () {
                var submitIds = [];
                
                $scope.selectedDefinitions.forEach(function(def) {
                    var defId = $scope.selectedDefinitions[def];
                    if (defId)
                        submitIds.push(defId);
                });
                buildDefinitionService.startBuild(submitIds).then(function (builds) {
                    $scope.IsLaunching = true;
                    $scope.selectedDefinitions = [];
                    for (var i = 0; i < builds.length; i++) {
                        var build = builds[i];
                        
                        var buildDefinitionId = build.definition.id;
                        console.log("Finding build definition for id ", buildDefinitionId);
                        var buildDefinition = undefined;
                        for (var j = 0; j < $scope.buildDefinitions.length; j++) {
                            
                            buildDefinition = $scope.buildDefinitions[j];
                            if (buildDefinition.id === buildDefinitionId) {
                                break;
                            }
                        }

                        if (buildDefinition !== undefined) {
                            buildDefinition.launched = true;
                            buildDefinition.latestBuilds.push(build);
                        }
                        $scope.IsLaunching = false;
                    }
                });
            }
        }
    ]);