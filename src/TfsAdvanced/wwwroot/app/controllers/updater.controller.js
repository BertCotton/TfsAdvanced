angular.module('TFS.Advanced').controller('UpdaterController',['$scope', '$interval', '$notification', "$filter", 'buildsService', 'pullrequestsService',
        function ($scope, $interval, $notification, $filter, buildsService, pullrequestsService) {
            'use strict';

            var isLoadingPRs = false;
            var isLoadingBuilds = false;
            var pullRequestUpdates = [];

            pullrequestsService.start();
            buildsService.start();

            $interval(function() {
                loadPullRequests();
                loadBuilds();
            }, 2000);


            var pullRequests = [];
            var initialPRLoadDone = false;
            function loadPullRequests() {
                if (isLoadingPRs) {
                    return;
                }
                isLoadingPRs = true;
                pullrequestsService.get().then(function (data) {

                    if (data === null || data === undefined) {
                        isLoadingPRs = false;
                        return;
                    }

                    var prUpdates = [];
                    data.forEach(function (pr) {
                        if(pr.pullRequestId)
                            prUpdates[pr.pullRequestId] = pr;
                    });

                    if (initialPRLoadDone) {
                        prUpdates.forEach(function (pr) {
                                if (pr.pullRequestId && pullRequestUpdates[pr.pullRequestId] === undefined) {
                                    newPrNotification(pr);
                            }
                        });

                        pullRequests.forEach(function (pr) {
                            if (pr.pullRequests && prUpdates[pr.pullRequestId] === undefined)
                                removedPrNotification(pr);
                        });
                    }

                    pullRequestUpdates = prUpdates;
                    pullRequests = data;

                    initialPRLoadDone = true;
                    isLoadingPRs = false;

                });
            }

            function newPrNotification(pr) {
                $notification('New PR From  ' + pr.createdBy.displayName,
                {
                    body: pr.title + " | " + pr.repository.name,
                    icon: 'images/site.png',
                    focusWindowOnClick: true,
                    delay: 10000
                });
            }

            function removedPrNotification(pr) {
                $notification('Completed PR From  ' + pr.createdBy.displayName,
                {
                    body: pr.title + " | " + pr.repository.name,
                    icon: 'images/site.png',
                    focusWindowOnClick: true,
                    delay: 10000
                });
            }

            var latestBuildId = 0;
            var builds = [];

            function loadBuilds() {
                if (isLoadingBuilds)
                    return;
                isLoadingBuilds = true;
                buildsService.get().then(function (data) {
                    builds = $filter('orderBy')(data, "id", true);
                    var updatedBuilds = $filter('filter')(data,
                        function (build) {
                            return build.id > latestBuildId;
                        }) || [];

                    

                    if (latestBuildId > 0) {
                        updatedBuilds.forEach(function(build) {
                            if (build.status > 1 && build.result === 0)
                                newFailedBuildNotification(build);
                        });
                    }

                    if (builds.length > 0)
                        latestBuildId = builds[0].id;

                    isLoadingBuilds = false;
                });
            }

            function newFailedBuildNotification(build) {
                console.log(build);
                $notification('Failed Build',
                {
                    body: "(" + build.project.name + ") " + build.definition.name,
                    icon: 'images/site.png',
                    focusWindowOnClick: true,
                    delay: 10000
                });
            }

            loadPullRequests();
            loadBuilds();
}]);