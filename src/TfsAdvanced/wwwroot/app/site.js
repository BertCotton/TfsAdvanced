/*globals angular */
var app = angular.module('TFS.Advanced', [
    'ngAnimate',
    'ngCookies',
    'ngSanitize',
    'ngResource',
    'ngRoute',
    'ui.router',
    'ui.bootstrap',
    'angular-web-notification',
    'ngTable',
    "angular-appinsights",
    'nvd3'
]);

app.config(function ($locationProvider, $routeProvider, $httpProvider, insightsProvider) {
    'use strict';

    $locationProvider.html5Mode({
        enabled: true,
        requireBase: false
    });

    $routeProvider
        .when("/pullRequests", { templateUrl: 'views/pullrequests.html' })
        .when("/buildDefinitions", { templateUrl: 'views/buildDefinitions.html' })
        .when("/buildStatistics", { templateUrl: 'views/buildStatistics.html' })
        .when("/updateStatus", { templateUrl: 'views/updateStatus.html' })
        .when("/jobRequests", { templateUrl: 'views/jobRequests.html' });


    insightsProvider.start('61137fb3-e654-4fb7-88d3-242de0edf9d6');

    $httpProvider.interceptors.push("Interceptor");

    
});


app.controller("MainController",
    ['$scope', 'healthService',
    function ($scope, healthService) {
        $scope.IsLoaded = false;
        $scope.LoadedPercent = 1;

        var watcher = $scope.$watch(healthService.LoadedStatus,
            function (loadedStatus) {
                $scope.IsLoaded = loadedStatus.isLoaded;
                $scope.LoadedPercent = (loadedStatus.loadedPercent * 100.0);
                $scope.LoadedPercent = Math.max(1, $scope.LoadedPercent);
                    
                if ($scope.IsLoaded) {
                    watcher();
                }
            });
    }]);