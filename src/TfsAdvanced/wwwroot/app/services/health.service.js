/*globals angular */
angular.module('TFS.Advanced').service('healthService', ['$resource', '$q', '$timeout', function ($resource, $q, $timeout) {
    'use strict';

    var loadedStatus = {};
    var isLoading = false;

    var resource = $resource("/health/LoadedStatus");

    this.LoadedStatus = function () {
        if (!isLoading)
            Check();
        return loadedStatus;
    }

    function Check() {
        isLoading = true;
        return resource.get(function (data) {
                loadedStatus = data;
                if(!response.data.isLoaded)
                    $timeout(Check, 1000);
        }, function (error) { console.log(error); }).$promise;
    }
}]);