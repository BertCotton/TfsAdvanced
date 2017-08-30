function formatPage() {
    $(".datetime-pretty").each(function (idx, elem) {
        $(elem).text($.format.prettyDate($(elem).text()));
    });

    $(".datetime-shorttime").each(function (idx, elem) {
        $(elem).text($.format.date($(elem).text(), "MMMM dd HH:mm a"));
    });
}