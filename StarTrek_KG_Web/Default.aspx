﻿<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="StarTrek_KG_Web._Default" %>

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


String.prototype.strip = function(char) {
    return this.replace(new RegExp("^" + char + "*"), '').
        replace(new RegExp(char + "*$"), '');
}


$.extend_if_has = function(desc, source, array) {
    for (var i=array.length;i--;) {
        if (typeof source[array[i]] != 'undefined') {
            desc[array[i]] = source[array[i]];
        }
    }
    return desc;
};

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
            prompt: '> ',
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

        if (command !== '') {
            terminal.echo(command);
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
