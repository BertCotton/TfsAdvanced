angular.module('TFS.Advanced')
    .controller('PullRequestsController',
    [
        '$scope', '$filter', 'pullrequestsService', 'ProjectService',
        function ($scope, $filter, pullrequestsService, ProjectService) {
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
                $scope.projects = [{ "id": "-1", "name": "[Teams Filter]" }].concat(data);
            });

            function filterPullRequests(data) {
                if (angular.isArray(data)) {
                    var prs = $filter('orderBy')(data, "creationDate");
                    $scope.pullRequests = $filter('filter')(prs,
                        function (record) {
                            return $scope.SelectedProject === "-1" ||
                                $scope.SelectedProject === record.repository.project.id;
                        });
                } else {
                    console.log("Response not an array:", data);
                }
            }

            $scope.GetRequiredReviewerCount = function (pullRequest) {
                for (var index in pullRequest.repository.policyConfigurations) {
                    if (pullRequest.repository.policyConfigurations[index].type.displayName === "Minimum number of reviewers")
                        return pullRequest.repository.policyConfigurations[index].settings.minimumApproverCount;
                }
                return undefined;

            }

            $scope.getReviewersCount = function (pullRequest) {
                var count = 0;
                var createdBy = pullRequest.createdBy;
                for (var index in pullRequest.reviewers) {
                    var reviewer = pullRequest.reviewers[index];
                    if (reviewer.isContainer === false && reviewer.id !== createdBy.id && reviewer.vote === 10) {
                        count++;
                    }
                }
                return count;
            };

            $scope.RequiresBuilds = function(pullRequest) {
                for (var index in pullRequest.repository.policyConfigurations) {
                    if (pullRequest.repository.policyConfigurations[index].type.displayName === "Build")
                        return true;
                }
                return false;
            }

            $scope.HasEnoughReviewers = function (pullRequest) {
                var requiredReviewers = $scope.GetRequiredReviewerCount(pullRequest);
                var reviewersCount = $scope.getReviewersCount(pullRequest);
                if(requiredReviewers)
                    return reviewersCount >= requiredReviewers;
                return undefined;
            }

            $scope.UpdateSelectedProject = function () {
                filterPullRequests($scope.RawPullRequests);
            };
        }
    ]);