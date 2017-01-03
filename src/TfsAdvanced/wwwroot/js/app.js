function formatPage() {
    $(".datetime-pretty").each(function (idx, elem) {
        $(elem).text($.format.prettyDate($(elem).text()));
    });
}