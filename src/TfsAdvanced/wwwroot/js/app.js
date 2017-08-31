function formatPage() {
    $(".datetime-pretty").each(function (idx, elem) {
        $(elem).text($.format.prettyDate($(elem).text()));
    });

    $(".datetime-shorttime").each(function (idx, elem) {
        $(elem).text($.format.date($(elem).text(), "MMMM dd h:mm a"));
    });
}

var newPullRequestsEvent = new Event('NewPullRequests');
var updatedPullRequestsEvent = new Event("UpdatedPullRequests");
var newCompletedPullRequestsEvent = new Event("NewCompletedPullRequests");
var newMyPullRequestsEvent = new Event("NewMyPullRequests");