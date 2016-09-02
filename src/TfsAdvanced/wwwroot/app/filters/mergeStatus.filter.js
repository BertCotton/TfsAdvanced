angular.module('TFS.Advanced').filter('mergeStatus', function ($sce) {
    return function (result) {
        if (result === undefined)
            return "";
        var text = "";
        var color = "default";
        switch (result) {
            case "failed":
                text = "Failed";
                color = "red";
                break;
            case "succeeded":
                text = "Succeeded";
                color = "green";
                break;
            case "partiallySucceeded":
                text = "Partially Succeeded";
                color = "red";
                break;
            case "cancelled":
                text = "Cancelled";
                color = "yellow";
                break;
        }
        return $sce.trustAs('html', "<span style='color:" + color + "'>" + text + "</span>");
    };
});