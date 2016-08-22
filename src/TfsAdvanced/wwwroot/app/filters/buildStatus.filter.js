angular.module('TFS.Advanced').filter('buildStatus', function ($sce) {
    return function (status) {
        if (status === undefined)
            return "";
        var text = "";
        var color = "default";
        switch(status) {
            case 0:
                text = "In Progress";
                color = "blue";
                break;
            case 1:
                text = "Completed";
                color = "green";
                break;
            case 2:
                text = "Cancelling";
                color = "orange";
                break;
            case 3:
                text = "Postponed";
                color = "yellow";
                break;
            case 4:
                text = "Not Started";
                color = "gray";
                break;
            case 5:
                text = "All";
                color = "gray";
                break;
        }
        return $sce.trustAs('html', "<span style='color:" + color + "'>" + text + "</span>");
    };
});