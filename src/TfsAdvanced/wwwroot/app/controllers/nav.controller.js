angular.module('TFS.Advanced')
    .controller('NavController',
    [
        '$scope', '$location', function ($scope, $location) {
            'use strict';
            $scope.isActive = function (route) {
                return route === $location.path();
            }

        }
    ]);