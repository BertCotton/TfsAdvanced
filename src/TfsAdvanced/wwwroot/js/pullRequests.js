fetchData();

function fetchData() {
    var pullRequests = JSON.parse(localStorage.getItem("PullRequests"));
    if (!pullRequests || pullRequests.length === 0) {
        $("#pullRequestHeader").hide();
        $("#pullRequests").hide();
        $("#NoPullRequests").show();
        return;
    }
    
    $("#pullRequestHeader").show();
    $("#pullRequests").show();
    $("#NoPullRequests").hide();
    pullRequests.sort(function (a, b) {
        var aDate = new Date(a.CreatedDate).getTime();
        var bDate = new Date(b.CreatedDate).getTime();
        if (aDate === bDate)
            return 0;
        if (aDate > bDate)
            return 1;
        else
            return -1;

    });
    $("#pullRequests").html($("#pullRequestTemplate").tmpl(pullRequests));
    formatPage();
    window.setTimeout(fetchData, 1000);
}
