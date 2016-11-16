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
                "./bower_components/jquery/dist/jquery.js",
                "./bower_components/bootstrap/dist/js/bootstrap.js",
                "./bower_components/angular/angular.js",
                "./bower_components/angular-bootstrap/ui-bootstrap.js",
                "./bower_components/angular-cookies/angular-cookies.js",
                "./bower_components/angular-sanitize/angular-sanitize.js",
                "./bower_components/angular-resource/angular-resource.js",
                "./bower_components/angular-route/angular-route.js",
                "./bower_components/angular-animate/angular-animate.js",
                "./bower_components/angular-ui-router/release/angular-ui-router.js",
                "./bower_components/angular-datatables/dist/angular-datatables.js",
                "./bower_components/angular-notification/angular-notification.js",
                "./bower_components/angular-route/angular-route.js",
                "./bower_components/d3/d3.js",
                "./bower_components/nvd3/build/nv.d3.js",
                "./bower_components/angular-nvd3/dist/angular-nvd3.js",
                "./wwwroot/app/lib/angular-appinsights.js",
                "./node_modules/ng-table/bundles/ng-table.js",
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
          //  .pipe(uglify())
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

gulp.task("build", ["concat:js", "minify:js", "copy:css", "copy:fonts"]);

gulp.task("watch",
    function () {
        return gulp.watch("wwwroot/app/**/*.js", ["concat:js"]);
    });

gulp.task("watch:css",
    function () {
        return gulp.watch("wwwroot/app/**/*.css", ["copy:css"]);
    });