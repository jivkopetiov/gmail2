app.controller("HeadingController", function ($scope, server, currentUser) {
    $scope.logout = function () {
        server.regenerateData().then(function (response) {
            window.location.reload();
        });
    };

    $scope.currentUser = currentUser;
});

app.controller("ComposeController", function ($scope, server, $location) {
    
    $scope.users = server.getAllUsers();
    
    jQuery("#to").select2();

    $scope.createPost = function () {

        server.createPost($scope.title, $scope.body, $scope.to).then(function (response) {
            $location.path('/inbox');
        });
    };

    $scope.discardPost = function () {
        window.history.back();
    };

});

app.controller("MenuController", function ($scope, $location, server, $rootScope) {

    $scope.fixedMenuItems = [
        { url: "/inbox", text: "Inbox" },
        { url: "/starred", text: "Starred" },
        { url: "/unread", text: "Unread" },
        { url: "/sent", text: "Sent" },
        { url: "/draft", text: "Drafts" },
        { url: "/allmail", text: "All Mail" },
        { url: "/spam", text: "Spam" },
        { url: "/recyclebin", text: "Recycle Bin" }
    ];

    $scope.dynamicMenuItems = [
        { url: "/tags/vip", text: "VIP" },
        { url: "/tags/important", text: "Important" },
        { url: "/tags/travel", text: "Travel" },
        { url: "/tags/friends", text: "Friends" },
        { url: "/tags/social", text: "Social" },
        { url: "/tags/family", text: "Family" },
        { url: "/tags/work", text: "Work" },
        { url: "/tags/personal", text: "Personal" }
    ];

    $scope.$watch(function () {
        return $location.path();
    }, function (path) {
        $scope.locationPath = path;
    });

    $scope.droppedItem = function (drag, drop, tag) {
        server.moveItem(drag.id, tag).then(function () {
            $rootScope.$broadcast("postsUpdate");
        });
    };

});

app.controller("PostDetailsController", function ($scope, $routeParams, server, $location) {

    server.getPostById($routeParams.postId).then(function (data) {
        $scope.post = data.Data;
    });

    $scope.favorite = function (post) {
        post.IsFavorite = !post.IsFavorite;
        server.updatePost(post);
    };

    $scope.attachmentClick = function (post, attachment) {

        var url = server.getAttachmentUrl(post, attachment);
        window.open(url, '_blank', '');

    };

});

app.controller("PostsController", function ($scope, $location, server, $route, $routeParams) {

    reloadData(0);

    $scope.getPage = function (newPage) {
        reloadData(newPage);
    };

    $scope.$on("postsUpdate", function () {
        reloadData($scope.page);
    });

    function reloadData(page) {

        if ($routeParams.query) {
            server.searchPosts($routeParams.query, page).then(function (data) {
                $scope.Posts = data.Data;
                $scope.page = data.Page;
                $scope.totalPages = data.Total;
            });
        }
        else {
            server.getPosts($route.current.realm, page, $route.current.tag).then(function (data) {
                $scope.Posts = data.Data;
                $scope.page = data.Page;
                $scope.totalPages = data.Total;
            });
        }

        
    }

    $scope.getVisibleTags = function (tags) {

        var realm = $route.current.realm;

        if (!tags)
            return null;

        var result = [];

        for (var i = 0; i < tags.length; i++) {
            var tag = tags[i];
            if (tag === realm)
                continue;

            if (tag === "draft" && realm === "draft")
                continue;

            result.push(tag);
        }

        return result;
    };

    $scope.favorite = function (post) {
        post.IsFavorite = !post.IsFavorite;
        server.updatePost(post);
    };

    $scope.postClicked = function(post) {
        $location.path("/post/" + post.Id);
        if (!post.IsRead) {
            post.IsRead = true;
            server.updatePost(post);
        }
    };

    var specialTags = [ "inbox", "spam", "starred", "unread", "sent", "draft", "allmail", "recyclebin" ];

    $scope.checkedPosts = function () {
        return _.filter($scope.Posts, function (post) { return post.IsChecked; });
    };

    $scope.deleteSelected = function () {

        var postsToDelete = $scope.checkedPosts();
        if (postsToDelete)
            server.deletePosts(postsToDelete).then(function () {
                reloadData($scope.page);
            });
        
    };

    $scope.markAsRead = function () {

        var checkedPosts = $scope.checkedPosts();

        angular.forEach(checkedPosts, function(post) {
            if (!post.IsRead) {
                post.IsRead = true;
                server.updatePost(post);
            }
        });

    };

    $scope.refresh = function () {
        reloadData($scope.page);
    };

    $scope.tagClicked = function (post, tag) {

        if (_.contains(specialTags, tag))
            $location.path("/" + tag);
        else
            $location.path("/tags/" + tag);
    };

    $scope.searchClicked = function () {

        var text = $scope.searchText;
        if (text)
            $location.path("/search/" + text);

    };

});
