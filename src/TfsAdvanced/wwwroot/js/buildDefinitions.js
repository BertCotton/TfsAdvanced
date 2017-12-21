

$(document)
    .ready(function () {

        var charts = [];
        var dataTable = $("#buildsTable").DataTable({
            "order": [3, "asc"],
            "ajax": {
                "url": "/data/BuildDefinitions",
                "dataSrc": function (json) {
                    for (var i = 0, ien = json.length; i < ien; i++) {
                        var buildDefinition = json[i];
                        console.log(buildDefinition);
                        json[i][0] = '<input type="checkbox" name="definitionIds" value="' +
                            buildDefinition.id +
                            '" id="checkbox-' +
                            buildDefinition.id +
                            '" />';
                        json[i][1] = buildDefinition.name;
                        json[i][2] = buildDefinition.folder;
                        json[i][3] = '<a href="' + buildDefinition.url + '" target="_blank">' + buildDefinition.name + "</a>";
                        json[i][4] = buildDefinition.defaultBranch;
                    }


                    return json;
                }
            },
            "drawCallback": function () {
                for (var i = 0; i < charts.length; i++) {

                    var chartData = charts[i];

                    try {

                        var div = document.getElementById('runtime_chart_' + chartData.id);
                        if (div == undefined) {
                            continue;
                        }
                        var chart = new google.visualization.ColumnChart(div);
                        chart.draw(chartData.data,
                            {
                                vAxis: {
                                    gridlines: {
                                        color: 'transparent'
                                    },
                                    textPosition: 'none'
                                },
                                hAxis: { textPosition: 'none' },
                                legend: { position: 'none' },
                                tooltip: {
                                    trigger: 'none'
                                },
                                width: 130,
                                height: 30,
                                bar: {
                                    groupWidth: "10"
                                }


                            });

                    } catch (e) {
                        console.log(chartData);
                        console.log(e);
                    }


                }
            }

        });
        formatPage();
    });

function toggle(select) {
    $("input[type='checkbox']")
        .each(function () {
            var checkBox = $(this);
            if (checkBox.is(':visible')) {
                if (select)
                    checkBox.prop("checked", true);
                else
                    checkBox.prop("checked", false);
            } else {
                checkBox.prop("checked", false);
            }

        });

}

function queueDefinitions() {
    $("input[type='checkbox']:checked")
        .each(function () {
            var checkBox = $(this);
            var buildDefinitionId = checkBox.val();
            checkBox.prop("checked", false);
            $("td", checkBox.closest("tr")).addClass("launching");
            $.post("data/BuildDefinitions/" + buildDefinitionId, function (data) {
                console.log("Launched: ", data);
                var buildDefinitionId = data.definition.id;
                var checkBox = $("#checkbox-" + buildDefinitionId);
                $("td", checkBox.closest("tr")).removeClass("launching");
                if (data.buildNumber === "Failed To Start Build") {
                    $("td.BuildStatus", checkBox.closest("tr"))
                        .html("<span style='color:red'><i class='glyphicon glyphicon-remove'></i> Failed To Launch<span>");
                } else {

                    $("td.BuildStatus", checkBox.closest("tr"))
                        .html("<a href='" +
                            data._links.web.href +
                            "' style='color:blue' target='_blank'><i class='glyphicon glyphicon-fire'></i> Launched<a>");
                }
            });
        });
}
