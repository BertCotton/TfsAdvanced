angular.module('TFS.Advanced').filter('buildResult', function ($sce) {
    return function (build) {

        if (build.result === undefined)
            return "";

        if (build.status === 0)
            return $sce.trustAs('html',
                "<div class='progress'><div class='progress-bar progress-bar-success progress-bar-striped active' role='progressbar' style='width:100%'></div> </div>");
        var text = "";
        var color = "default";
        switch (build.result) {
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