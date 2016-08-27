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

            var isLoadedWatch = undefined;

            
            $scope.load = function () {
                $scope.RawPullRequests = pullrequestsService.pullRequests();
                filterPullRequests($scope.RawPullRequests);
                $scope.IsLoading = false;

            };

            isLoadedWatch = $scope.$watch(pullrequestsService.isLoaded,
                function (isLoaded) {
                    if (isLoaded === true) {
                        $scope.load();
                        isLoadedWatch();
                    }
                });

            $scope.$watchCollection(pullrequestsService.pullRequests,
                function (data) {
                    if(data)
                        $scope.load();
                });


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