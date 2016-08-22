angular.module('TFS.Advanced')
    .controller('BuildDefinitionController',
    [
        '$scope', '$location', '$interval', '$notification', '$filter', 'DTOptionsBuilder', 'buildDefinitionService',
        function ($scope, $location, $interval, $notification, $filter, DTOptionsBuilder, buildDefinitionService) {
            'use strict';

            
            
            $scope.buildDefinitions = [];
            $scope.selectedDefinitions = [];
            $scope.dtOptions = DTOptionsBuilder.newOptions().withOption('order', [1, 'asc']);
            $scope.dtInstance = {};

            $scope.load = function() {
                buildDefinitionService.GET.success(function(data) {
                    $scope.buildDefinitions = data;
                }).then(function() {
                    $("#definitionsTable").dataTable().draw();
                });
            };

            $scope.noneChecked = function () {
                return $filter('filter')($scope.selectedDefinitions,function(def) {
                    return def !== undefined;
                }).length === 0;
            };

            $scope.toggle = function() {
                if ($scope.noneChecked()) {
                    $scope.buildDefinitions.forEach(function(def) {
                        $scope.selectedDefinitions[def.id] = def;
                    });
                } else {
                    $scope.selectedDefinitions = [];
                }
            };

            $scope.isSelected = function (def) {
                return $scope.selectedDefinitions[def.id] !== undefined;
            }

            $scope.select = function (def) {
                if($scope.isSelected(def))
                    $scope.selectedDefinitions[def.id] = undefined;
                else 
                    $scope.selectedDefinitions[def.id] = def.id;
            }

            $scope.queueDefinitions = function() {
                
            }

            $scope.load();
        }
    ]);