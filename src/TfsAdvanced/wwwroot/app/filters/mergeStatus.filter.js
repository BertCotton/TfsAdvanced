angular.module('TFS.Advanced').filter('mergeStatus', function ($sce) {
    return function (result) {
        if (result === undefined)
            return "";
        var text = "";
        var color = "default";
        switch (result) {
            case 0:
                text = "Failed";
                color = "red";
                break;
            case 1:
                text = "Succeeded";
                color = "green";
                break;
            case 2:
                text = "Partially Succeeded";
                color = "red";
                break;
            case 3:
                text = "Cancelled";
                color = "yellow";
                break;
        }
        return $sce.trustAs('html', "<span style='color:" + color + "'>" + text + "</span>");
    };
});