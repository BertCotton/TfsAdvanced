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
    'datatables',
    'ngTable',
    "angular-appinsights"
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
    .state('builds', {
        url: '/builds',
        templateUrl: 'views/builds.html'
    });

    insightsProvider.start('61137fb3-e654-4fb7-88d3-242de0edf9d6');

    $urlRouterProvider.otherwise('/pullRequests');
    $httpProvider.interceptors.push("Interceptor");
});
