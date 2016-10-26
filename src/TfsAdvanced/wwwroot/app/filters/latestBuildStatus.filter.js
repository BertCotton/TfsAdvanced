angular.module('TFS.Advanced').filter('latestBuildStatus', function ($sce) {
    return function (builds) {
        if (builds === undefined)
            return "";
        var latestFinishedBuild = undefined;
        var latestBuild = undefined;
        for (var i = 0; i < builds.length; i++) {
            var build = builds[i];
            if (latestBuild === undefined || latestBuild.id < build.id)
                latestBuild = build;

            if (build.finishTime && (latestFinishedBuild === undefined || latestFinishedBuild.id < build.id))
                    latestFinishedBuild = build;
        }
        
        var text = "";
        var color = "gray";
        var glyphicon = "glyphicon ";
        if (latestBuild === undefined) {
            text = "Never Built";
            glyphicon += "glyphicon-leaf";
        } else {
            var result = latestBuild.result;
            if (latestFinishedBuild === undefined) {
                text = "Never Finished";
                glyphicon += "glyphicon-warning-sign";
            } else
                result = latestFinishedBuild.result;

            switch (result) {
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
            if (latestFinishedBuild !== undefined && latestBuild.id !== latestFinishedBuild.id) {
                text = text + "<a href='" + latestFinishedBuild.buildUrl + "' target='_blank'> | Currently Building</a>";
            }
                
        }


        return $sce.trustAs('html', "<span style='color:" + color +"' class='" + glyphicon + "'>" + text + "</span>");
    };
});