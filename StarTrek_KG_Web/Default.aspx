<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="StarTrek_KG_Web._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <!-- -*- mode:javascript -*--->
<!DOCTYPE HTML>
<html>
<head>
  <meta charset="utf-8" />
  <title>Star Trek KG</title>
  <meta name="Description" content="Star Trek KG"/>

  <script src="Scripts/jquery-1.10.2.min.js"></script>
  <script src="Scripts/jquery.mousewheel-min.js"></script>

  <script src="Scripts/jquery.terminal-0.8.8.js"></script>
  <link href="Content/jquery.terminal.css" rel="stylesheet" />
<script>

(function($) {
    $.fn.terminalWindow = function (eval, options) {

        if ($('body').data('termWindow')) {
            return $('body').data('termWindow').terminal;
        }

        this.addClass('tilda');

        options = options || {};
        eval = eval || function(command, term) {
            term.echo("you don't set eval for termWindow");
        };

        var settings = {
            prompt: 'Enter Command:> ',
            name: 'termWindow',
            height: 600,
            enabled: true,
            greetings: 'Star Trek KG'
        };

        $.extend(settings, options);

        this.append('<div class="td"></div>');
        var self = this;

        self.terminal = this.find('.td').terminal(eval, settings);

        $('body').data('termWindow', this);

        return self;
    };
})(jQuery);


jQuery(document).ready(function($) {

    $('#termWindow').terminalWindow(function (command, terminal) {

        //make an ajax call here..  when it completes, loop to echo stuff back

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

                    terminal.echo("\r");

                    //for a complete separation of business logic, we might want to do this in the page data.
                    $.each(response, function (index, item) {
                        terminal.echo(item);
                    });

                    terminal.echo("\r");
                }
            }).fail(function (failReason) {
                //turn the font color rod

                response = jQuery.parseJSON(failReason.d);

                terminal.echo("\r");

                terminal.echo("**Problem Communicating with Server** ~ ");

                //for a complete separation of business logic, we might want to do this in the page data.
                $.each(response, function (index, item) {
                    terminal.echo(item);
                });

                terminal.echo("\r");
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
