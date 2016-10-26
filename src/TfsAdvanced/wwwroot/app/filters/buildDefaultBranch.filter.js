angular.module('TFS.Advanced').filter('buildDefaultBranch', function ($sce) {
    return function (branch) {
        if (branch === undefined || branch === null)
            return "";
        if (branch.includes("develop"))
            return "develop";
        if (branch.includes("master"))
            return "master";
        return branch;
    };
});