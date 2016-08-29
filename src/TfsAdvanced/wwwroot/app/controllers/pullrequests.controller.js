angular.module('TFS.Advanced')
    .controller('PullRequestsController',
    [
        '$scope', '$notification', '$filter', 'pullrequestsService', 'ProjectService',
        function ($scope, $notification, $filter, pullrequestsService, ProjectService) {
            'use strict';

            $scope.SelectedProject = "-1";
            $scope.pullRequests = [];
            $scope.RawPullRequests = [];
            $scope.IsLoading = true;


            $scope.$watch(pullrequestsService.isLoaded,
                function (isLoaded) {

                    if (isLoaded) {
                        filterData(pullrequestsService.pullRequests());
                        $scope.IsLoading = false;
                    }
                });

            $scope.$watchCollection(pullrequestsService.pullRequests,
                function (data) {
                    if ($scope.IsLoading) {
                        return;
                    }
                    filterData(data);

                });
            function filterData(data) {
                if (data === undefined || data === null)
                    return;
                    $scope.RawPullRequests = data;
                    filterPullRequests(data);
                
            }

            ProjectService.GET.success(function (data) {
                $scope.projects = [{"id": "-1", "name": "Any"}].concat(data);
            });

            function filterPullRequests(data)
            {
                if (angular.isArray(data)) {
                    var prs = $filter('orderBy')(data, "creationDate");
                    $scope.pullRequests = $filter('filter')(prs,
                        function(record) {
                            return $scope.SelectedProject === "-1" ||
                                $scope.SelectedProject === record.repository.project.id;
                        });
                } else {
                    console.log("Response not an array:", data);
                }
            }
            
            $scope.UpdateSelectedProject = function () {
                filterPullRequests($scope.RawPullRequests);
            };

        }
    ]);