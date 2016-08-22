/*globals angular */
var app = angular.module('TFS.Advanced', [
    'ngAnimate',
    'ngCookies',
    'ngSanitize',
    'ngResource',
    'ui.router',
    'ui.bootstrap',
    'notification',
    'datatables'
]);

app.config(function ($stateProvider, $urlRouterProvider) {
    'use strict';

    $stateProvider.state('pullRequests', {
        url: '/',
        templateUrl: 'views/pullrequests.html'
    })
    .state('BuildDefinitions', {
        url: '/buildDefinitions',
        templateUrl: 'views/buildDefinitions.html'
    })
    .state('Builds', {
        url: '/builds',
        templateUrl: 'views/builds.html'
    })
    .state("WorkItems",
        {
            url: '/workItems',
            templateUrl: 'views/workItems.html'
        });

    $urlRouterProvider.otherwise('/');
});
