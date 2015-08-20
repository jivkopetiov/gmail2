var app = angular.module('gmailApp', ['ngRoute']);

app.config(function ($routeProvider, $locationProvider) {

    var realms = ["default", "inbox", "starred", "unread", "sent", "draft", "allmail", "spam", "recyclebin"];

    for (var i = 0; i < realms.length; i++) {
        var realm = realms[i];

        var realmUrl = "/" + realm;
        if (realm === "default")
            realmUrl = "/";

        $routeProvider.when(realmUrl, { templateUrl: "/views/posts.htm", controller: "PostsController", realm: realm });
    }

    var tags = [ "me", "vip", "important", "travel", "friends", "social", "family", "work", "personal" ];

    for (var i = 0; i < tags.length; i++) {
        var tag = tags[i];
        $routeProvider.when("/tags/" + tag, { templateUrl: "/views/posts.htm", controller: "PostsController", realm: "tag", tag: tag });
    }

    $routeProvider.when("/post/:postId", { templateUrl: "/views/postDetails.htm", controller: "PostDetailsController" });
    $routeProvider.when("/compose", { templateUrl: "/views/compose.htm", controller: "ComposeController" });
    $routeProvider.when("/search/:query", { templateUrl: "/views/posts.htm", controller: "PostsController" });
    $routeProvider.otherwise({ redirectTo: '/error' });

    $locationProvider.html5Mode(true);

});



if (!String.prototype.format) {
    String.prototype.format = function () {
        var args = arguments;
        return this.replace(/{(\d+)}/g, function (match, number) {
            return typeof args[number] != 'undefined'
              ? args[number]
              : match
            ;
        });
    };
}



app.directive('lvlDraggable', ['$rootScope', 'uuid', function ($rootScope, uuid) {
    return {
        restrict: 'A',
        link: function (scope, el, attrs, controller) {

            angular.element(el).attr("draggable", "true");
            var id = attrs.id;
            if (!attrs.id) {
                id = uuid.new()
                angular.element(el).attr("id", id);
            }

            el.bind("dragstart", function (e) {
                e.dataTransfer = e.originalEvent.dataTransfer;
                e.dataTransfer.setData('text', id);

                $rootScope.$emit("LVL-DRAG-START");
            });

            el.bind("dragend", function (e) {
                $rootScope.$emit("LVL-DRAG-END");
            });
        }
    }
}]);

app.directive('lvlDropTarget', ['$rootScope', 'uuid', function ($rootScope, uuid) {
    return {
        restrict: 'A',
        scope: {
            onDrop: '&'
        },
        link: function (scope, el, attrs, controller) {
            var id = attrs.id;
            if (!attrs.id) {
                id = uuid.new()
                angular.element(el).attr("id", id);
            }

            el.bind("dragover", function (e) {
                if (e.preventDefault) {
                    e.preventDefault(); // Necessary. Allows us to drop.
                }

                e.dataTransfer = e.originalEvent.dataTransfer;
                e.dataTransfer.dropEffect = 'move';  // See the section on the DataTransfer object.
                return false;
            });

            el.bind("dragenter", function (e) {
                // this / e.target is the current hover target.
                angular.element(e.target).addClass('lvl-over');
            });

            el.bind("dragleave", function (e) {
                angular.element(e.target).removeClass('lvl-over');  // this / e.target is previous target element.
            });

            el.bind("drop", function (e) {
                if (e.preventDefault) {
                    e.preventDefault(); // Necessary. Allows us to drop.
                }

                if (e.stopPropogation) {
                    e.stopPropogation(); // Necessary. Allows us to drop.
                }
                e.dataTransfer = e.originalEvent.dataTransfer;
                var data = e.dataTransfer.getData("text");
                var dest = document.getElementById(id);
                var src = document.getElementById(data);

                scope.onDrop({ dragEl: src, dropEl: dest });
            });

            $rootScope.$on("LVL-DRAG-START", function () {
                var el = document.getElementById(id);
                angular.element(el).addClass("lvl-target");
            });

            $rootScope.$on("LVL-DRAG-END", function () {
                var el = document.getElementById(id);
                angular.element(el).removeClass("lvl-target");
                angular.element(el).removeClass("lvl-over");
            });
        }
    }
}]);

app.directive('ngEnter', function () {
    return function (scope, element, attrs) {
        element.bind("keydown keypress", function (event) {
            if (event.which === 13) {
                scope.$apply(function () {
                    scope.$eval(attrs.ngEnter);
                });

                event.preventDefault();
            }
        });
    };
});