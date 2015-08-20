var nowTime = new Date().getTime();

app.factory("server", function ($http, $q) {

    var result = {};

    var cachedUsers = [];

    result.regenerateData = function () {
        return $http.get('/Server/Service.ashx?op=RegenerateData');
    };

    result.createPost = function (title, body, to) {
        return $http.get('/Server/Service.ashx?op=CreatePost&title={0}&body={1}&to={2}'.format(title, body, to));
    };

    result.getAllUsers = function () {
        return cachedUsers;
    };

    result.getPostById = function (id) {

        var def = $q.defer();

        $http.get('/Server/Service.ashx?op=GetPostById&id=' + id).then(function (response) {

            FixDate(response.data.Data);
            def.resolve(response.data);

        });

        return def.promise;
    };

    result.getAttachmentUrl = function (post, attachment) {
        return "/Server/Service.ashx?op=GetAttachment&postId={0}&attachmentId={1}".format(post.Id, attachment.Id);
    };

    result.moveItem = function (itemId, tag) {
        return $http.get("/Server/Service.ashx?op=MoveItem&id={0}&tag={1}".format(itemId, tag.text));
    };

    result.getPosts = function (realm, page, tag) {

        var url = '/Server/Service.ashx?op=GetPosts&page=' + page + "&realm=" + realm;
        if (tag)
            url += "&tag=" + tag;

        return makePostsRequest(url);
    };

    result.searchPosts = function (query, page) {

        var url = '/Server/Service.ashx?op=SearchPosts&page=' + page + "&query=" + query;
        return makePostsRequest(url);
        
    };
    
    var makePostsRequest = function(url) {

        var def = $q.defer();

        $http.get(url).then(function (response) {
            var posts = response.data.Data;

            if (posts) {
                for (var i = 0; i < posts.length; i++)
                    FixDate(posts[i]);

                cachedUsers = _.uniq(_.map(posts, function (post) { return post.Sender; }));
            }

            def.resolve(response.data);
        });

        return def.promise;

    };

    result.updatePost = function (post) {
        return $http.post("/Server/Service.ashx?op=UpdatePost", post);
    };

    result.deletePosts = function (posts) {
        return $http.post("/Server/Service.ashx?op=DeletePosts", _.map(posts, function (post) { return post.Id; }) );
    };

    function FixDate(post) {
        if (post.DateSent)
            post.DateSent = new Date(parseInt(post.DateSent.substr(6)));
    }


    return result;

});

app.filter("dateOffset", function () {

    return function (date) {

        var offsetMilli = nowTime - (date);

        if (offsetMilli < 0)
            return "moments ago";

        var seconds = Math.floor(offsetMilli / 1000);
        if (seconds < 60)
            return "moments ago";

        var minutes = Math.floor(seconds / 60);

        if (minutes === 1)
            return "1 minute ago";

        if (minutes < 60)
            return minutes.toString() + " minutes ago";

        var hours = Math.floor(minutes / 60);
        if (hours === 1)
            return "1 hour ago";

        if (hours < 24)
            return hours.toString() + " hours ago";

        var days = Math.floor(hours / 24);

        if (days === 1)
            return "1 day ago";

        if (days < 60)
            return days.toString() + " days ago";

        var months = Math.floor(days / 30);

        if (months < 24)
            return months.toString() + " months ago";

        return "more than 2 years ago";

    };

});

app.filter("trimBody", function () {

    return function(body) {
        if (body.length > 40)
            body = body.substring(0, 39);

        return body;
    };

});

app.filter("trimSubject", function () {

    return function (subject) {
        if (subject.length > 50)
            subject = subject.substring(0, 49);

        return subject;
    };

});

app.filter("friendlySizeFromBytes", function () {

    return function(bytes) {
        var sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB'];
        if (bytes == 0) return '0 Bytes';
        var i = parseInt(Math.floor(Math.log(bytes) / Math.log(1024)));
        return Math.round(bytes / Math.pow(1024, i), 2) + ' ' + sizes[i];
    };

});


app.factory('uuid', function () {
    var svc = {
        new: function () {
            function _p8(s) {
                var p = (Math.random().toString(16) + "000000000").substr(2, 8);
                return s ? "-" + p.substr(0, 4) + "-" + p.substr(4, 4) : p;
            }
            return _p8() + _p8(true) + _p8(true) + _p8();
        },

        empty: function () {
            return '00000000-0000-0000-0000-000000000000';
        }
    };

    return svc;
});