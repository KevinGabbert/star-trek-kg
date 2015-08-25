<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="StarTrek_KG_Web._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <!-- -*- mode:javascript -*--->
<!DOCTYPE HTML>
<html>
<head>
  <meta charset="utf-8" />
  <title>Star Trek KG</title>
  <meta name="Description" content="Star Trek KG"/>

  <script src="Scripts/jquery-2.1.4.min.js"></script>
  <script src="Scripts/jquery.mousewheel-min.js"></script>

  <script src="Scripts/jquery.terminal-0.8.8.js"></script>
  <script src="Scripts/TerminalWindow.js"></script>

  <link href="Content/jquery.terminal.css" rel="stylesheet" />
  <link href="Content/Site.css" rel="stylesheet" />
<script>


jQuery(document).ready(function ($) {

    $('#termWindow').terminalWindow(function (command, terminal) {

        var sessionID = "1234";
        var response;

        if (command !== '') {
            $.ajax({
                type: "POST",
                url: "Default.aspx/QueryConsole",
                data: "{ command: '" + command + "'," +
                      "sessionID: '" + sessionID + "'}",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function(retVal) {
                    response = jQuery.parseJSON(retVal.d);

                    //for a complete separation of business logic, we might want to do this in the page data.
                    $.each(response, function (index, item) {
                        terminal.echo(item,
                        {
                            raw: true
                        });
                    });
                }
            }).fail(function (failReason) {
                response = jQuery.parseJSON(failReason.d);

                terminal.echo("**Problem Communicating with Server** ~ ");

                //for a complete separation of business logic, we might want to do this in the page data.
                $.each(response, function (index, item) {
                    terminal.echo(item);
                });
            });
        }
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
