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

gulp.task("clean", ["clean:js", "clean:css"]);

gulp.task("concat:js",
    ["clean:js"],
    function () {
        return gulp.src([
                "./node_modules/jquery/dist/jquery.min.js",
                "./node_modules/bootstrap/dist/js/bootstrap.min.js",
                "./node_modules/datatables.net/js/jquery.dataTables.min.js",
                "./node_modules/angular/angular.min.js",
                "./node_modules/angular-bootstrap/ui-bootstrap.min.js",
                "./node_modules/angular-cookies/angular-cookies.min.js",
                "./node_modules/angular-sanitize/angular-sanitize.min.js",
                "./node_modules/angular-resource/angular-resource.min.js",
                "./node_modules/angular-route/angular-route.min.js",
                "./node_modules/angular-animate/angular-animate.min.js",
                "./node_modules/angular-ui-router/release/angular-ui-router.min.js",
                "./node_modules/angular-sanitize/angular-sanitize.min.js",
                "./node_modules/angular-datatables/dist/angular-datatables.min.js",
                "./node_modules/angular-notification/angular-notification.min.js",
                "./node_modules/angular-route/angular-route.min.js",
                "./bower_components/d3/d3.min.js",
                "./bower_components/nvd3/build/nv.d3.min.js",
                "./bower_components/angular-nvd3/dist/angular-nvd3.min.js",
                "./node_modules/ng-table/dist/ng-table.min.js/",
                "./wwwroot/app/lib/angular-appinsights.js",
                "./wwwroot/app/site.js",
                "./wwwroot/app/filters/*.js",
                "./wwwroot/app/services/*.js",
                "./wwwroot/app/controllers/*.js"
        ])
            .pipe(debug())
            .pipe(concat("./wwwroot/js/app.js"))
            .pipe(gulp.dest("."));
    });

gulp.task("minify:js",
    ["concat:js"],
    function() {
        return gulp.src("./wwwroot/js/app.js")
            .pipe(concat("./wwwroot/js/app.min.js"))
            .pipe(uglify())
            .pipe(gulp.dest("."));
    });

gulp.task("copy:css",
    ["clean:css"],
    function () {
        return gulp.src([
                "./node_modules/bootstrap/dist/css/bootstrap.min.css",
                "./node_modules/angular-datatables/dist/css/angular-datatables.min.css",
                "./node_modules/ng-table/dist/ng-table.css",
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
               "./node_modules/bootstrap/dist/fonts/*",
        ])
           .pipe(debug())
           .pipe(gulp.dest("./wwwroot/fonts/"));
    });

gulp.task("build", ["concat:js", "minify:js", "copy:css", "copy:fonts"]);

gulp.task("watch",
    function () {
        return gulp.watch("wwwroot/app/**/*.js", ["concat:js"]);
    });

gulp.task("watch:css",
    function () {
        return gulp.watch("wwwroot/app/**/*.css", ["copy:css"]);
    });