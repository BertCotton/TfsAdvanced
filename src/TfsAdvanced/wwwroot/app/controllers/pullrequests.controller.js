angular.module('TFS.Advanced')
    .controller('PullRequestsController',
    [
        '$scope', '$q', '$interval', '$notification', '$filter', 'pullrequestsService',
        function ($scope, $q, $interval, $notification, $filter, pullrequestsService) {
            'use strict';

            $scope.pullRequests = [];
            $scope.IsLoading = true;

            
            $scope.load = function () {
                pullrequestsService.get()
                    .then(function(data) {
                        $scope.pullRequests = $filter('orderBy')(data, "creationDate");
                        $scope.IsLoading = false;
                    });
            };


            $interval($scope.load, 2000);

          

            $scope.load();
        }
    ]);