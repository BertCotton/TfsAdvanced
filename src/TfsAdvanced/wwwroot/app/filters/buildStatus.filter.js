angular.module('TFS.Advanced').filter('buildStatus', function ($sce) {
    return function (status) {
        if (status === undefined)
            return "";
        var text = "";
        var color = "default";
        switch(status) {
            case "inProgress":
                text = "In Progress";
                color = "blue";
                break;
            case "completed":
                text = "Completed";
                color = "green";
                break;
            case "cancelling":
                text = "Cancelling";
                color = "orange";
                break;
            case "postponed":
                text = "Postponed";
                color = "orange";
                break;
            case "notStarted":
                text = "Not Started";
                color = "gray";
                break;
            case "all":
                text = "All";
                color = "gray";
                break;
        }
        return $sce.trustAs('html', "<span style='color:" + color + "'>" + text + "</span>");
    };
});