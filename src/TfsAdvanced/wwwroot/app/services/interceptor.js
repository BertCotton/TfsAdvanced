angular.module('TFS.Advanced')
    .factory('Interceptor', ['$q', '$location', '$window',
        function ($q, $location, $window) {
            var canceller = $q.defer();
            return {
                'request': function (config) {
                    config.timeout = canceller.promise;
                    return config;
                },
                'response': function (response) {
                    return response;
                },
                'responseError': function (rejection) {
                    if (rejection.status < 0)
                        return rejection;
                    if (rejection.status === 403) {
                        canceller.resolve('Unauthorized');
                        var url = $location.absUrl();
                        var baseUrl = url.split("#")[0];
                        $window.location = baseUrl + '/data/Login';
                    }
                    if (rejection.status === 401) {
                        canceller.resolve('Unauthorized');
                        var url = $location.absUrl();
                        var baseUrl = url.split("#")[0];
                        $window.location = baseUrl + '/data/Login';
                    }
                    return $q.reject(rejection);
                }
            }
        }]);