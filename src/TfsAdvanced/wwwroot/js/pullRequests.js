

var local = undefined;
var isLoaded = false;

$(function () {
    local = {
        viewModel: new viewModel()
    };
    
    ko.applyBindings(local.viewModel, document.getElementById("pullRequests"));
    fetchData();
});

$("#myCompletedPullRequestPanel").on("click",
    function() {
        $("#myCompletedPullRequestPanel").toggleClass("col-lg-8 col-lg-12");
    });

function fetchData() {

    if (localStorage.getItem("HasUpdate") === "true" || isLoaded == false) {
        HandlMyPullRequests();
        HandleTeamPullRequests();
        HandleMyCompletedPullRequests();

        formatPage();
        localStorage.setItem("HasUpdate", "false");
        loaded = true;
    }
    window.setTimeout(fetchData, 500);

}

// Every minute reformat so that the dates keep updated
setTimeout(formatPage, 60000);

function HandlMyPullRequests() {

    var myPullRequests = JSON.parse(localStorage.getItem("CurrentUserPullRequests"));

    if (!myPullRequests)
        return;
    else if (myPullRequests.length === 0) {
        $("#myPullRequests").hide();
    } else {
        $("#myPullRequests").show();
        $("#myPullRequestCount").html(myPullRequests.length);
        var viewModel = local.viewModel;
        UnionArrays(viewModel.myPullRequests, myPullRequests, "Id");
        SortByKeyDesc(viewModel.myPullRequests, "CreatedDate");
    }
}

function HandleTeamPullRequests(event) {
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
        var viewModel = local.viewModel;
        UnionArrays(viewModel.teamPullRequests, pullRequests, "Id");
        SortByKeyDesc(viewModel.teamPullRequests, "CreatedDate");
    }
}

function HandleMyCompletedPullRequests() {
    var pullRequests = JSON.parse(localStorage.getItem("CurrentUserCompletedPullRequests"));
    if (!pullRequests)
        return;
    else if (pullRequests.length === 0) {
        $("#myCompletedPullRequestsTable").hide();
    } else {
        $("#myCompletedPullRequestsTable").show();
        var viewModel = local.viewModel;
        UnionArrays(viewModel.myCompletedPullRequests, pullRequests, "Id");
        SortByKeyDesc(viewModel.myCompletedPullRequests, "ClosedDate");
    }
}



var viewModel = function () {
    self = this;
    self.teamPullRequests = ko.observableArray([]);
    self.myPullRequests = ko.observableArray([]);
    self.myCompletedPullRequests = ko.observableArray([]);
}