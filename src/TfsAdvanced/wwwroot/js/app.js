function formatPage() {
    var longDateFormat = 'dd/MM/yyyy hh:mm:ss a';

    $(".datetime").each(function (idx, elem) {
        $(elem).text($.format.date($(elem).text(), longDateFormat));
    });
    $(".datetime-pretty").each(function (idx, elem) {
        $(elem).text($.format.prettyDate($(elem).text()));
    });
}