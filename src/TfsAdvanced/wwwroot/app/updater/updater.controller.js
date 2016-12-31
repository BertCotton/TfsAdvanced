angular.module('TFS.Advanced').controller('UpdaterController',
    ['$scope', '$interval', 'webNotification', "$filter", 'pullrequestsService', 'buildDefinitionService', 'tasksService', 'updateStatusService', 'buildStatisticService',
function ($scope, $interval, webNotification, $filter, pullrequestsService, buildDefinitionService, tasksService, updateStatusService, buildStatisticService) {
            'use strict';

            var isPRsLoaded = false;
            var pullRequestUpdates = [];
            var pullRequests = [];
            var initialPRLoadDone = false;
   

            pullrequestsService.start();
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

    
        }]);