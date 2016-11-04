angular.module('TFS.Advanced').filter('average', function ($sce) {
    return function (numbers) {
        if (numbers === undefined || numbers === null || numbers.length === 0)
            return "";
        var sum = numbers.reduce(function (a, b) { return a + b; });
        return Math.round(((sum / numbers.length) / 1000 / 60)*10)/10 + " mins";

    };
});