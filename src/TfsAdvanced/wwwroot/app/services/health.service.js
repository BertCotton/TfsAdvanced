/*globals angular */
angular.module('TFS.Advanced').service('healthService', ['$http', '$q', '$timeout', function ($http, $q, $timeout) {
    'use strict';

    var loadedStatus = {};
    var isLoading = false;

    this.LoadedStatus = function () {
        if (!isLoading)
            Check();
        return loadedStatus;
    }

    function Check() {
        isLoading = true;
        return $http.get("/health/LoadedStatus")
            .then(function (response) {
                loadedStatus = response.data;
                if(!response.data.isLoaded)
                    $timeout(Check, 1000);
            });
    }
}]);