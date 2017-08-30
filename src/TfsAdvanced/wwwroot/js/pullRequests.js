fetchData();

$("#myCompletedPullRequestPanel").on("click",
    function() {
        $("#myCompletedPullRequestPanel").toggleClass("col-lg-8 col-lg-12");
    });

function fetchData() {
    var pullRequests = JSON.parse(localStorage.getItem("PullRequests"));
    var myPullRequests = JSON.parse(localStorage.getItem("CurrentUserPullRequests"));
    var myCompletedPullRequests = JSON.parse(localStorage.getItem("CurrentUserCompletedPullRequests"));

    HandlMyPullRequests(myPullRequests);
    HandleTeamPullRequests(pullRequests);
    HandleMyCompletedPullRequests(myCompletedPullRequests);
    

    formatPage();
    window.setTimeout(fetchData, 1000);
}

function HandlMyPullRequests(myPullRequests) {

    if (!myPullRequests)
        return;
    else if (myPullRequests.length === 0) {
        $("#myPullRequests").hide();
    } else {
        $("#myPullRequests").show();
        $("#myPullRequestCount").html(myPullRequests.length);
        sortByDate(myPullRequests);
        $("#myPullRequestsTable").html($("#myPullRequestTemplate").tmpl(myPullRequests));
    }
}

function HandleTeamPullRequests(pullRequests) {

    if(!pullRequests)
        return;
    if (pullRequests.length === 0) {
        $("#pullRequestHeader").hide();
        $("#pullRequests").hide();
        $("#NoPullRequests").show();
    } else {
        $("#pullRequestCount").html(pullRequests.length);
        $("#pullRequestHeader").show();
        $("#pullRequests").show();
        $("#NoPullRequests").hide();
        sortByDate(pullRequests);
        $("#pullRequests").html($("#pullRequestTemplate").tmpl(pullRequests));
    }
}

function sortByDate(pullRequests, reverse = false) {
    if (!pullRequests)
        return;
    pullRequests.sort(function (a, b) {
        var aDate = new Date(a.CreatedDate).getTime();
        var bDate = new Date(b.CreatedDate).getTime();
        if (aDate === bDate)
            return 0;
        if (aDate > bDate)
            return reverse ? -1 : 1;
        else
            return reverse ? 1 : -1;
    });
}

function HandleMyCompletedPullRequests(pullRequests) {
    if(!pullRequests)
        return;
    else if (pullRequests.length === 0) {
        $("#myCompletedPullRequestsTable").hide();
    } else {
        $("#myCompletedPullRequestsTable").show();
        sortByDate(pullRequests, true);
        $("#myCompletedPullRequestsTable").html($("#completedPullRequestTemplate").tmpl(pullRequests));
    }
}