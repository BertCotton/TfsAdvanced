angular.module('TFS.Advanced')
    .controller('JobRequestsController',
    [
        '$window', '$scope', '$location', '$interval', '$notification', '$filter', 'NgTableParams', 'tasksService',
        function ($window, $scope, $location, $interval, $notification, $filter, NgTableParams, tasksService) {
            'use strict';

            

            $scope.tableParams = new NgTableParams({
                count: 20,
                page: 1,
                sorting: {
                    id: 'name'
                }
            },
               {
                   counts: [20,50,100],
                   getData: function (params) {
                       var data = tasksService.jobRequests();
                       
                       var filters = params.filter();
                       var newFilters = {};
                       for (var key in filters) {
                           if (filters.hasOwnProperty(key)) {
                               console.log(key);
                               switch (key) {
                                   case 'definition':
                                       angular.extend(newFilters,
                                       {
                                           definition: {
                                               name: filters[key]
                                           }
                                       });
                                       break;
                                   default:
                                       newFilters[key] = filters[key];
                               }
                           }
                       }
                       var filteredData = params.filter() ? $filter('filter')(data, newFilters) : data;
                       var orderedData = params.sorting()
                           ? $filter('orderBy')(filteredData, params.orderBy())
                           : filteredData;
                       var page = orderedData.slice((params.page() - 1) * params.count(),
                           params.page() * params.count());

                       params.total(filteredData.length);
                       return page;
                   }
               });

            $scope.$watchCollection(tasksService.jobRequests,
                function () {
                    $scope.tableParams.reload();
                });

            $scope.getResultColor = function (jobRequest) {
                
                if (jobRequest.planType === "build") {
                    if (jobRequest.owner.status === "completed") {
                        switch (jobRequest.owner.result) {
                        case "succeeded":
                            return {"color": 'green'};
                        case "failed":
                            return { "color": 'red' };
                        case "abandoned":
                            return { "color": 'gray' };
                        case "canceled":
                            return { "color": 'orange' };
                        }
                    } else
                        return { "color": 'blue' };
                } else {
                    
                    if (jobRequest.finishTime) {
                        switch(jobRequest.result)
                        {
                            case "succeeded":
                                return { "color": 'green' };
                            case "failed":
                                return { "color": 'red' };
                            case "abandoned":
                                return { "color": 'gray' };
                            case "canceled":
                                return { "color": 'orange' };
                        }
                    }
                    else
                        return { "color": 'blue' };
                }
            };
        }
    ]);