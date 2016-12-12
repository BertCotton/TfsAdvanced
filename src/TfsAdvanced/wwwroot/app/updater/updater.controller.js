angular.module('TFS.Advanced').controller('UpdaterController',
    ['$scope', '$interval', 'webNotification', "$filter", 'buildsService', 'pullrequestsService', 'buildDefinitionService', 'tasksService', 'updateStatusService', 'buildStatisticService',
function ($scope, $interval, webNotification, $filter, buildsService, pullrequestsService, buildDefinitionService, tasksService, updateStatusService, buildStatisticService) {
            'use strict';

            var isBuildsLoaded = true;
            var isPRsLoaded = false;
            var pullRequestUpdates = [];
            var pullRequests = [];
            var initialPRLoadDone = false;
            var latestBuildId = 0;
            var builds = [];


            pullrequestsService.start();
            buildsService.start();
            buildDefinitionService.start();
            tasksService.start();
            updateStatusService.start();
            buildStatisticService.start();


            $scope.$watch(pullrequestsService.isLoaded,
                function(isLoaded) {
                    isPRsLoaded = isLoaded;
                });

            $scope.$watchCollection(pullrequestsService.pullRequests,
                function (data) {
                    if (data === undefined || !isPRsLoaded)
                        return;
                    var prUpdates = [];
                    data.forEach(function(pr) {
                        if (pr.pullRequestId)
                            prUpdates[pr.pullRequestId] = pr;
                    });

                    
                    if (initialPRLoadDone) {
                        prUpdates.forEach(function(pr) {
                            if (pr.pullRequestId && pullRequestUpdates[pr.pullRequestId] === undefined) {
                                newPrNotification(pr);
                            }
                        });

                        pullRequests.forEach(function(pr) {
                            if (pr.pullRequests && prUpdates[pr.pullRequestId] === undefined)
                                removedPrNotification(pr);
                        });
                    }

                    pullRequestUpdates = prUpdates;
                    pullRequests = data;
                    initialPRLoadDone = true;
                });

            function newPrNotification(pr) {
                webNotification.showNotification('New PR From  ' + pr.createdBy.displayName,
                {
                    body: pr.title + " | " + pr.repository.name,
                    icon: 'images/site.png',
                    focusWindowOnClick: true,
                    autoClose: 10000
                });
            }

            function removedPrNotification(pr) {
                webNotification.showNotification('Completed PR From  ' + pr.createdBy.displayName,
                {
                    body: pr.title + " | " + pr.repository.name,
                    icon: 'images/site.png',
                    focusWindowOnClick: true,
                    autoClose: 10000
                });
            }

            $scope.$watch(buildsService.IsLoading,
                function (isLoaded) {
                    isBuildsLoaded = isLoaded;
                });


            $scope.$watchCollection(buildsService.builds,
                function(data) {
                    if (data === undefined || !isBuildsLoaded)
                        return;

                    builds = $filter('orderBy')(data, "id", true);
                    var updatedBuilds = $filter('filter')(data,
                            function(build) {
                                return build.id > latestBuildId;
                            }) ||
                        [];

                    if (latestBuildId > 0) {
                        updatedBuilds.forEach(function(build) {
                            if (build.status === "completed" && build.result === "failed")
                                newFailedBuildNotification(build);
                        });
                    }

                    if (builds.length > 0)
                        latestBuildId = builds[0].id;

                });

            function newFailedBuildNotification(build) {
                console.log(build);
                webNotification.showNotification('Failed Build',
                {
                    body: "(" + build.project.name + ") " + build.definition.name,
                    icon: 'images/site.png',
                    focusWindowOnClick: true,
                    autoClose: 10000
                });
            }

        }]);