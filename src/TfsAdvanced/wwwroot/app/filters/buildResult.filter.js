angular.module('TFS.Advanced').filter('buildResult', function ($sce) {
    return function (build) {
        if (build.result === undefined)
            return "";

        if (build.status === 0)
            return $sce.trustAs('html',
                "<div class='progress'><div class='progress-bar progress-bar-success progress-bar-striped active' role='progressbar' style='width:100%'></div> </div>");
        var text = "";
        var color = "default";

        var finishedTime;
        var startTime = build.startTime;
        if (build.status === "notStarted")
        {
            text = "Queued";
            color = "blue";
            startTime = build.queueTime;
            finishedTime = new Date();
        }
        else if (build.status === "inProgress") {
            text = "<strong>Building</strong>";
            color = "blue";
            finishedTime = new Date();
        } else {
            finishedTime = new Date(build.finishTime);
            switch (build.result) {
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
            case "canceled":
                text = "Cancelled";
                color = "black";
                break;
            }
        }
        var difference = finishedTime - new Date(startTime).getTime();
        var differenceSeconds = difference / 1000;
        var differenceMinutes = differenceSeconds / 60;
        // Times 100 , divide by 100 to give 2 decimal places after rounding
        var runtime = Math.round(differenceMinutes * 100)/100;
        text += " (" + runtime + " min)";
        return $sce.trustAs('html', "<span style='color:" + color + "'>" + text + "</span>");
    };
});
