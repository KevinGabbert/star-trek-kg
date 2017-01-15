<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" EnableViewState="false" CodeBehind="Default.aspx.cs" Inherits="StarTrek_KG_Web._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

<!DOCTYPE HTML>
<html>
<head>
  <meta charset="utf-8" />
  <title>Star Trek KG</title>
  <meta name="Description" content="Star Trek KG"/>

  <script src="Scripts/jquery-2.2.4.min.js"></script>
  <script src="Scripts/jquery.mousewheel-min.js"></script>

  <script src="Scripts/jquery.terminal-0.11.23.js"></script>
  <script src="Scripts/TerminalWindow.js"></script>
  <script src="Scripts/class.js"></script>
  <script src="Scripts/Default/Default.js"></script>

  <link href="Content/jquery.terminal.css" rel="stylesheet" />
  <link href="Content/Site.css" rel="stylesheet" />

<script>

jQuery(document).ready(function ($) {

    var _default = new window.defaultPage();

    $('#termWindow').terminalWindow(function (command, terminal) {

        var sessionID = "1234";

        //todo: stop user from typing.
        //todo: start .js animation to show that we are waiting on the server to respond.
        //todo: this animation could just be text slowly typing.. (this is a jquery terminal feature)

         _default.QueryConsole(command, sessionID, terminal);
    });
});


</script>
</head>
<body>
    <div id="termWindow"></div>
    Star Trek KG
</body>
</html>

</asp:Content>
