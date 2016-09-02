angular.module('TFS.Advanced')
    .controller('BuildsController',
    [
        '$scope', '$filter', 'DTOptionsBuilder', 'buildsService', 'ProjectService',
            function($scope, $filter, DTOptionsBuilder, buildsService,ProjectService) {
            'use strict';

            $scope.SelectedProjectId = "-1";
            $scope.builds = [];
            $scope.IsLoaded = true;
            $scope.projects = [];

            $scope.dtOptions = DTOptionsBuilder.newOptions().withOption('order', [0, 'desc']);

            $scope.$watch(buildsService.isLoaded,
                function(isLoaded) {
                    $scope.IsLoaded = isLoaded;
                });

            $scope.$watchCollection(buildsService.builds,
                function (data) {
                    if ($scope.IsLoaded) {
                        $scope.RawBuilds = data;
                        filterBuilds(data);
                    }

                });

            ProjectService.GET.success(function (data) {
                $scope.projects = [{ "id": "-1", "name": "Any" }].concat(data);
            });

            function filterBuilds(data) {
                if (angular.isArray(data)) {
                    $scope.builds = $filter('filter')(data,
                        function (record) {
                            
                            return $scope.SelectedProjectId === "-1" ||
                                $scope.SelectedProjectId === record.project.id;
                        });
                } else {
                    console.log("Response not an array:", data);
                }
            }

            $scope.UpdateSelectedProject = function () {
                $scope.SelectedProjectId = this.SelectedProjectId;
                filterBuilds($scope.RawBuilds);
            };
}]);