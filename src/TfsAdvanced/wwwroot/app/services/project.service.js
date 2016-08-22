/*globals angular */
angular.module('TFS.Advanced').service('ProjectService', ['$http', function ($http) {
    'use strict';
    return {
        GET: $http.get('data/Projects')
    };
}]);