var _thisPage;

var defaultPage = Class.extend({

    init: function () {
        _thisPage = this; //allows us to ref this object inside this object.
    },
    Reset: function () {
        _thisPage.Start();
    },
    Start: function () {

    },
    QueryConsole: function (command, sessionID, terminal) {

        var response;

        if (command !== '') {
            $.ajax({
                type: "POST",
                url: "Default.aspx/QueryConsole",
                data: "{ command: '" + command + "'," +
                      "sessionID: '" + sessionID + "'}",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                async: false, //verify that this works
                success: function (retVal) {
                    response = jQuery.parseJSON(retVal.d);

                    //for a complete separation of business logic, we might want to do this in the page data.
                    $.each(response, function (index, item) {
                        terminal.echo(item,
                        {
                            raw: true
                        });
                    });

                    //set success to true
                    //can we set this to synchronous?
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

        return terminal;
    }
});
