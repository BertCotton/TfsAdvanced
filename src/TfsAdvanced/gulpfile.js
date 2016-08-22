/// <binding Clean='clean' ProjectOpened='concat:js' />
"use strict";

var gulp = require("gulp");
var clean = require("gulp-clean");
var concat = require("gulp-concat");
var debug = require("gulp-debug");
var watch = require("gulp-watch");
var uglify = require("gulp-uglify");

gulp.task("clean:js", function (cb) {
    return gulp.src("./wwwroot/js/**/*.js")
        .pipe(clean());
});

gulp.task("clean:css", function (cb) {
    return gulp.src("./wwwroot/css/**/*.css")
        .pipe(clean());
});

gulp.task("clean", ["clean:js", "clean:css"]);

gulp.task("concat:js",
    ["clean:js"],
    function () {
        return gulp.src([
                "./node_modules/jquery/dist/jquery.js",
                "./node_modules/bootstrap/dist/js/bootstrap.js",
                "./node_modules/datatables.net/js/jquery.dataTables.js",
                "./node_modules/angular/angular.js",
                "./node_modules/angular-bootstrap/ui-bootstrap.js",
                "./node_modules/angular-cookies/angular-cookies.js",
                "./node_modules/angular-sanitize/angular-sanitize.js",
                "./node_modules/angular-resource/angular-resource.js",
                "./node_modules/angular-route/angular-route.js",
                "./node_modules/angular-animate/angular-animate.js",
                "./node_modules/angular-ui-router/release/angular-ui-router.js",
                "./node_modules/angular-sanitize/angular-sanitize.js",
                "./node_modules/angular-datatables/dist/angular-datatables.js",
                "./node_modules/angular-notification/angular-notification.js",
                "./wwwroot/app/site.js",
                "./wwwroot/app/filters/*.js",
                "./wwwroot/app/services/*.js",
                "./wwwroot/app/controllers/*.js"
        ])
            .pipe(debug())
            .pipe(concat("./wwwroot/js/app.min.js"))
            //.pipe(uglify())
            .pipe(gulp.dest("."));
    });

gulp.task("copy:css", ["clean:css"], function () {
    return gulp.src([
        "./node_modules/bootstrap/dist/css/bootstrap.min.css",
        "./node_modules/angular-datatables/dist/css/angular-datatables.min.css",
        "./wwwroot/app/css/**.css"])
        .pipe(debug())
        .pipe(concat("./wwwroot/css/site.min.css"))
        //.pipe(cssmin())
        .pipe(gulp.dest("."));
});

gulp.task("build", ["concat:js", "copy:css"]);

gulp.task("watch", function () {
    return gulp.watch('wwwroot/app/**/*.js', ["concat:js"]);
})