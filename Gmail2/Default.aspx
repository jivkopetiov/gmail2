<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Gmail2.Default" %>


<!doctype html>
<html ng-app="gmailApp">
<head>
    <title>Gmail 2</title>
    <link href="/Site.css" rel="stylesheet" type="text/css" />
    <link href="/scripts/select2/select2.css" rel="stylesheet" />
</head>
<body>
    
    <div class="top" ng-controller="HeadingController">
        <span id="heading">Gmail 2</span>

        <span id="actions">
            Welcome {{currentUser.Name}},  
            <a href="#" ng-click="logout()">Logout</a>
        </span>
    </div>

    <div class="left" ng-controller="MenuController">
        <div class="gmail-button"><a href="/compose">Compose</a></div>
        <div id="folders">
            <div ng-repeat="item in fixedMenuItems" x-lvl-drop-target="true" on-drop="droppedItem(dragEl, dropEl)">
                <a ng-class="{'selected': locationPath === item.url}" href="{{item.url}}">{{item.text}}</a>
            </div>
            <br />
            <div ng-repeat="item in dynamicMenuItems" x-lvl-drop-target="true" on-drop="droppedItem(dragEl, dropEl, item)">
                <a ng-class="{'selected': locationPath === item.url}" href="{{item.url}}">{{item.text}}</a>
            </div>
        </div>
    </div>

    <div class="right">
        <div ng-view></div>
    </div>

    <script src="/scripts/underscore-min.js"></script>
    <script src="//ajax.googleapis.com/ajax/libs/jquery/1.11.1/jquery.min.js"></script>
    <script src="/scripts/angular.min.js" type="text/javascript"></script>
    <script src="/scripts/angular-route.min.js" type="text/javascript"></script>
    <script src="/scripts/select2/select2.min.js"></script>
    <script src="/scripts/app.js" type="text/javascript"></script>
    <script src="/scripts/controllers.js" type="text/javascript"></script>
    <script src="/scripts/services.js" type="text/javascript"></script>

    <script type="text/javascript">

        app.factory("currentUser", function () {

            var user = <%= currentUserString %>;

            return user;
        });

    </script>

</body>
</html>
