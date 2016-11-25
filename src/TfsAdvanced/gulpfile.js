"use strict";

var gulp = require("gulp");
var clean = require("gulp-clean");
var concat = require("gulp-concat");
var debug = require("gulp-debug");
var watch = require("gulp-watch");
var uglify = require("gulp-uglify");

gulp.task("clean:js",
    function () {
        return gulp.src("./wwwroot/js/**/*.js")
            .pipe(clean());
    });

gulp.task("clean:css",
    function () {
        return gulp.src("./wwwroot/css/**/*.css")
            .pipe(clean());
    });

gulp.task("clean:fonts",
    function () {
        return gulp.src("./wwwroot/fonts/*")
            .pipe(clean());
    });

gulp.task("clean:html",
    function () {
        return gulp.src("./wwwroot/views/*")
            .pipe(clean());
    });

gulp.task("clean", ["clean:js", "clean:css", "clean:html"]);

gulp.task("concat:js-app",
    ["clean:js"],
    function() {
        return gulp.src([
                "./wwwroot/app/site.js",
                "./wwwroot/app/filters/*.js",
                "./wwwroot/app/services/*.js",
                "./wwwroot/app/controllers/*.js",
                "./wwwroot/app/buildDefinitions/*.js",
                "./wwwroot/app/builds/*.js",
                "./wwwroot/app/buildStatistics/*.js",
                "./wwwroot/app/jobRequests/*.js",
                "./wwwroot/app/pullRequests/*.js",
                "./wwwroot/app/updater/*.js",
                "./wwwroot/app/updateStatus/*.js",
                "./wwwroot/app/lib/*.js"
            ])
            .pipe(debug())
            .pipe(concat("./wwwroot/js/app-only.js"))
            .pipe(gulp.dest("."));
    });

gulp.task("minify:js-app",
    ["concat:js-app"],
    function () {
        return gulp.src("./wwwroot/js/app-only.js")
            .pipe(concat("./wwwroot/js/app-only.min.js"))
            .pipe(uglify())
            .pipe(gulp.dest("."));
    });

gulp.task("concat:js",
    ["minify:js-app"],
    function () {
        return gulp.src([
                "./bower_components/jquery/dist/jquery.min.js",
                "./bower_components/bootstrap/dist/js/bootstrap.min.js",
                "./bower_components/angular/angular.js",
                "./bower_components/angular-bootstrap/ui-bootstrap.min.js",
                "./bower_components/angular-cookies/angular-cookies.min.js",
                "./bower_components/angular-sanitize/angular-sanitize.min.js",
                "./bower_components/angular-resource/angular-resource.min.js",
                "./bower_components/angular-route/angular-route.min.js",
                "./bower_components/angular-animate/angular-animate.min.js",
                "./bower_components/angular-ui-router/release/angular-ui-router.min.js",
                "./bower_components/angular-datatables/dist/angular-datatables.min.js",
                "./bower_components/angular-notification/angular-notification.min.js",
                "./bower_components/angular-route/angular-route.min.js",
                "./bower_components/d3/d3.min.js",
                "./bower_components/nvd3/build/nv.d3.min.js",
                "./bower_components/angular-nvd3/dist/angular-nvd3.min.js",
                "./wwwroot/app/lib/angular-appinsights.min.js",
                "./node_modules/ng-table/bundles/ng-table.min.js",
                "./wwwroot/js/app-only.js"
        ])
            .pipe(debug())
            .pipe(concat("./wwwroot/js/app.min.js"))
            .pipe(gulp.dest("."));
    });

gulp.task("copy:css",
    ["clean:css"],
    function () {
        return gulp.src([
                "./bower_components/bootstrap/dist/css/bootstrap.min.css",
                "./bower_components/angular-datatables/dist/css/angular-datatables.min.css",
                "./node_modules/ng-table/bundles/ng-table.min.css",
                "./bower_components/nvd3/build/nv3.d3.min.css",
                "./wwwroot/app/css/**.css"
        ])
            .pipe(debug())
            .pipe(concat("./wwwroot/css/site.min.css"))
            //.pipe(cssmin())
            .pipe(gulp.dest("."));
    });

gulp.task("copy:fonts", ["clean:fonts"],
    function() {
        return gulp.src([
               "./bower_components/bootstrap/dist/fonts/*",
        ])
           .pipe(debug())
           .pipe(gulp.dest("./wwwroot/fonts/"));
    });

gulp.task("copy:html", ["clean:html"],
    function() {
        return gulp.src([
                "./wwwroot/app/buildDefinitions/*.html",
                "./wwwroot/app/builds/*.html",
                "./wwwroot/app/buildStatistics/*.html",
                "./wwwroot/app/jobRequests/*.html",
                "./wwwroot/app/pullRequests/*.html",
                "./wwwroot/app/updater/*.html",
                "./wwwroot/app/updateStatus/*.html"
            ])
            .pipe(debug())
            .pipe(gulp.dest("./wwwroot/views/"));
    });

gulp.task("build", ["concat:js", "copy:css", "copy:fonts", "copy:html"]);

gulp.task("watch",
    function() {
        gulp.watch("wwwroot/app/**/*.css", ["copy:css"]);
        gulp.watch("wwwroot/app/**/*.js", ["concat:js"]);
        gulp.watch("wwwroot/app/**/*.html", ["copy:html"]);
    });


gulp.task("watch:js",
    function () {
        return gulp.watch("wwwroot/app/**/*.js", ["concat:js"]);
    });


gulp.task("watch:css",
    function () {
        return gulp.watch("wwwroot/app/**/*.css", ["copy:css"]);
    });

gulp.task("watch:html",
    function() {
        return gulp.watch("wwwroot/app/**/*.html", ["copy:html"]);
    });