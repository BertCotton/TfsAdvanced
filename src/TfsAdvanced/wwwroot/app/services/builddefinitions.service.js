/*globals angular */
angular.module('TFS.Advanced').service('buildDefinitionService', ['$http', function ($http) {
    'use strict';
    return {
        
        GET: $http.get('data/BuildDefinitions'),
        POST: function(data) {
            return $http.POST('data/BuildDefinitions', data);
        }
}
}]);