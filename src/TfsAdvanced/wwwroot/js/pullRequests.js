fetchData();

$("#myCompletedPullRequestPanel").on("click",
    function() {
        $("#myCompletedPullRequestPanel").toggleClass("col-lg-8 col-lg-12");
    });

function fetchData() {
    
    HandlMyPullRequests();
    HandleTeamPullRequests();
    HandleMyCompletedPullRequests();
    
    formatPage();

    document.getElementById("mainPage").addEventListener(updatedPullRequestsEvent, HandleTeamPullRequests, false);
    document.getElementById("mainPage").addEventListener(newCompletedPullRequestsEvent, HandleMyCompletedPullRequests, false);
    document.getElementById("mainPage").addEventListener(newMyPullRequestsEvent, HandlMyPullRequests, false);
    
}

function HandlMyPullRequests() {
    var myPullRequests = JSON.parse(localStorage.getItem("CurrentUserPullRequests"));

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

function HandleTeamPullRequests() {
    var pullRequests = JSON.parse(localStorage.getItem("PullRequests"));
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

function HandleMyCompletedPullRequests() {
    var pullRequests = JSON.parse(localStorage.getItem("CurrentUserCompletedPullRequests"));
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