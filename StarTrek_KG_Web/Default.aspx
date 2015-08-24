<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="StarTrek_KG_Web._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <!-- -*- mode:javascript -*--->
<!DOCTYPE HTML>
<html>
<head>
  <meta charset="utf-8" />
  <title>Quake like teminal - JQuery Terminal Emulator Demo</title>
  <meta name="Description" content="This is demonstration of JQuery Terminal Emulator Plugin. To run terminal type tilda on you keyboard."/>

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
    $.fn.tilda = function (eval, options) {

        if ($('body').data('tilda')) {
            return $('body').data('tilda').terminal;
        }

        this.addClass('tilda');

        options = options || {};
        eval = eval || function(command, term) {
            term.echo("you don't set eval for tilda");
        };

        var settings = {
            prompt: '> ',
            name: 'tilda',
            height: 1000,
            enabled: true,
            greetings: 'Star Trek KG'
        };

        $.extend(settings, options);

        this.append('<div class="td"></div>');
        var self = this;

        self.terminal = this.find('.td').terminal(eval, settings);

        $('body').data('tilda', this);

        return self;
    };
})(jQuery);


jQuery(document).ready(function($) {

    $('#tilda').tilda(function(command, terminal) {
        //terminal.echo('you type command "' + command + '"');
    });
});

</script>
</head>
<body>
    <div id="tilda"></div>
</body>
</html>

</asp:Content>
