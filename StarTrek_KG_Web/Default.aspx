<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="StarTrek_KG_Web._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="jumbotron">
        <h1>Star Trek KG</h1>
        <p class="lead">Home Page</p>
<%--        <p><a href="http://www.asp.net" class="btn btn-primary btn-lg">Learn more &raquo;</a></p>--%>
    </div>

    <div id="testtt"></div>

    <script type="text/javascript">

    //.js is structured so that there are separate, testable objects in other files.
        $(document).ready(function () {

            $("#testtt").text("Hello world");

            var _default = new window.defaultPage();
        });

    </script>
       <script src="Scripts/jquery-1.10.2.js"></script>   <script src="Scripts/Class.js" type="text/javascript"></script> <!-- a nice little pattern by John Resig to have "private" properties in .js classes -->
   <script src="Scripts/default/default.js" type="text/javascript"></script>

</asp:Content>
