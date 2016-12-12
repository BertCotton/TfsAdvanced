/*globals angular */
angular.module('TFS.Advanced').service('projectService', ['$resource', function ($resource) {
    'use strict';

    var resource = $resource("data/Projects");

    this.getProject = function() {
        return resource.query(function (data) { return data; }, function (error) { console.log(error); }).$promise;
    }
        
}]);