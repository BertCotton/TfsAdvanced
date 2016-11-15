angular.module('TFS.Advanced').filter('latestBuildStatus', function ($sce) {
    return function (latestBuild) {

        var text = "";
        var color = "gray";
        var glyphicon = "glyphicon ";
        if (latestBuild === undefined || latestBuild === null) {
            text = "Never Built";
            glyphicon += "glyphicon-leaf";
        }
        else if (latestBuild.finishTime === undefined) {
            text = "Currently Building";
            glyphicon += "";
            color = "blue";
        } else {
            switch (latestBuild.result) {
            case "failed":
                text = "Failed";
                glyphicon += "glyphicon-thumbs-down";
                color = "red";
                break;
            case "succeeded":
                text = "Succeeded";
                glyphicon += "glyphicon-thumbs-up";
                color = "green";
                break;
            case "partiallySucceeded":
                text = "Partially Succeeded";
                glyphicon += "glyphicon-fire";
                color = "orange";
                break;
            case "canceled":
                text = "Cancelled";
                glyphicon += "glyphicon-certificate";
                color = "orange";
                break;
            }

            text = "<a href='" + latestBuild.buildUrl + "' target='_blank'>" + text + "</a>";
        }


        return $sce.trustAs('html', "<span style='color:" + color +"' class='" + glyphicon + "'>" + text + "</span>");
    };
});