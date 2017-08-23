$.get({
    url: "data/Projects",
    success: function (data) {
        var selector = $(".projectSelector");
        $(data)
            .each(function (index) {
                var project = data[index];
                selector.append("<option  value='" + project.id + "'>" + project.name + "</option>");
            });
    }
});

fetchData();

function fetchData() {
    $.get("data/PullRequests",
        function (data) {
            console.log(data);
            if (data.length === 0) {
                $("#pullRequestHeader").hide();
                $("#pullRequests").hide();
                $("#NoPullRequests").show();
                return;
            }

            $("#pullRequestHeader").show();
            $("#pullRequests").show();
            $("#NoPullRequests").hide();
            data.sort(function (a, b) {
                var aDate = new Date(a.createdDate).getTime();
                var bDate = new Date(b.createdDate).getTime();
                if (aDate === bDate)
                    return 0;
                if (aDate > bDate)
                    return 1;
                else
                    return -1;

            });
            $("#pullRequests").html($("#pullRequestTemplate").tmpl(data));
            formatPage();
            window.setTimeout(fetchData, 1000);
        });
};
