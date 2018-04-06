var local = undefined;

$(function () {
    local = {
        viewModel: new viewModel()
    };

    ko.applyBindings(local.viewModel, document.getElementById("dashboards"));

    LoadData();
});

function LoadData() {
    $.getJSON("/data/Dashboard", function (data) {
        var viewModel = local.viewModel;
        UnionArrays(viewModel.builds, data["builds"], "id");
        UnionArrays(viewModel.releases, data["releases"], "requestId");

        SortByKeyDesc(viewModel.builds, "id");
        SortByKeyDesc(viewModel.releases, "requestId");

        setTimeout(LoadData, 2000);
    });
}




var viewModel = function () {
    self = this;
    self.builds = ko.observableArray([]);
    self.releases = ko.observableArray([]);

    self.isNewBuild = function (build) {
        // 10 minutes
        return (new Date(build.finishedDate).getTime() > (new Date().getTime() - 600000));
    };

    self.isNewRelease = function (release) {
        // 10 minutes
        return (new Date(release.finishedTime).getTime() > (new Date().getTime() - 600000));
    }

};
