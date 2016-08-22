angular.module('TFS.Advanced').filter('voteStatus', function ($sce) {
    return function (reviewer) {

        
        if (reviewer === undefined || reviewer.vote === undefined)
            return "<span></span>";
        var color = "default";
        var status = "No Reponse";
        switch (reviewer.vote) {
            case -10:
                status = "Rejected";
                color = "red";
                break;
            case -5:
                status = "Waiting For Author";
                color = "orange";
                break;
            case 0:
                status = "No Response";
                color = "gray";
                break;
            case 5:
                status = "Approved With Suggestions";
                color = "yellow";
                break;
            case 10:
                status = "Approved";
                color = "green";
                break;
        }
        var span = "<span style='color:" + color + "'><b>" + status + "</b></span><br/>" + reviewer.displayName;
        return $sce.trustAs('html', span);
    };
});