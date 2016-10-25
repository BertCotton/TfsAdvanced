angular.module('TFS.Advanced').filter('latestBuildStatus', function ($sce) {
    return function (builds) {
        if (builds === undefined)
            return "";
        var latestFinishedBuild = undefined;
        var latestBuild = undefined;
        for (var i = 0; i < builds.length; i++) {
            var build = builds[i];
            if (latestBuild === undefined) {
                latestBuild = build;
                if (build.finishTime)
                    latestFinishedBuild = build;
            } else if (latestBuild.id < build.id) {
                latestBuild = build[i];
                if (build.finishTime)
                    latestFinishedBuild = build;
            }
        }
        if (latestBuild !== latestFinishedBuild) {
            console.log("LatestBuild: ", latestBuild);
        }
        var text = "";
        var color = "default";
        if (latestFinishedBuild === undefined) {
            text = "Never Build";
            color = "gray";
        } else {
         

            var status = latestFinishedBuild.status;
            if (status === undefined)
                return "";
            
            switch (status) {
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

            if (latestBuild.id !== latestFinishedBuild.id) {
                text = "(Currently Building)";
            }
        }

        
        return $sce.trustAs('html', "<span style='color:" + color + "'>" + text + "</span>");
    };
});