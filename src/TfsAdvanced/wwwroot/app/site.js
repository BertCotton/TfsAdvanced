/*globals angular */
var app = angular.module('TFS.Advanced', [
    'ngAnimate',
    'ngCookies',
    'ngSanitize',
    'ngResource',
    'ngRoute',
    'ui.router',
    'ui.bootstrap',
    'notification',
    'ngTable',
    "angular-appinsights",
    'nvd3'
]);

app.config(function ($stateProvider, $urlRouterProvider, $routeProvider, $httpProvider, insightsProvider) {
    'use strict';

    $stateProvider.state('pullRequests', {
        url: '/pullRequests',
        templateUrl: 'views/pullrequests.html'
    })
    .state('buildDefinitions', {
        url: '/buildDefinitions',
        templateUrl: 'views/buildDefinitions.html'
    })
    .state("buildStatistics",
    {
        url: '/buildStatistics',
        templateUrl: 'views/buildStatistics.html'
    })
    .state("updateStatus",
    {
        url: '/updateStatus',
        templateUrl: 'views/updateStatus.html'
    })
    .state('builds', {
        url: '/builds',
        templateUrl: 'views/builds.html'
    })
    .state('jobRequests',
        {
            url: '/jobRequests',
            templateUrl: 'views/jobRequests.html'
        });

    insightsProvider.start('61137fb3-e654-4fb7-88d3-242de0edf9d6');

    $urlRouterProvider.otherwise('/pullRequests');
    $httpProvider.interceptors.push("Interceptor");

    
});


app.controller("MainController",
    ['$scope', 'healthService',
    function ($scope, healthService) {
        $scope.IsLoaded = false;
        $scope.LoadedPercent = 1;

        var watcher = $scope.$watch(healthService.LoadedStatus,
            function (loadedStatus) {
                console.log(loadedStatus);
                $scope.IsLoaded = loadedStatus.isLoaded;
                $scope.LoadedPercent = (loadedStatus.loadedPercent * 100.0);
                $scope.LoadedPercent = Math.max(1, $scope.LoadedPercent);
                    
                if ($scope.IsLoaded) {
                    watcher();
                }
            });
    }]);