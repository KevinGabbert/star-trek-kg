﻿var _thisPage;

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

        if (command !== '') {

            if (command === 'foo') {

                terminal.push(function () {

                    terminal.echo(command);

                    if (command.match(/y|yes/i))
                    {
                        terminal.echo(command + '  type "stop" again if you really mean it');
                        terminal.pop();
                        terminal = this.QueryConsoleAJAX(command, sessionID, terminal);
                    }
                    else if (command.match(/n|no/i)) {
                        terminal.pop();
                    }
                }, {
                    prompt: 'Are you sure? '
                });
            } else {
                terminal = this.QueryConsoleAJAX(command, sessionID, terminal);
            }
        }

        return terminal;
    },
    QueryConsoleAJAX: function(command, sessionID, terminal) {

        var response;

        $.ajax({
            type: 'POST',
            url: 'Default.aspx/QueryConsole',
            data: '{ command: \'' + command + '\',' +
                  'sessionID: \'' + sessionID + '\'}',
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            async: false,
            success: function (retVal) {
                response = jQuery.parseJSON(retVal.d);

                var header = response.shift();

                //for a complete separation of business logic, we might want to do this in the page data.
                if (header === "Err:") {

                    $.each(response, function(index, item) {
                        terminal.error(item);
                    });
                }
                else if (header.indexOf("Started:") > -1) {
                    $.each(response, function (index, item) {
                        terminal.echo(item,
                        {
                            color: 'FFFF00',
                            raw: true
                        });
                    });

                    terminal.flush();
                }
                else {

                    //for a complete separation of business logic, we might want to do this in the page data.
                    $.each(response, function (index, item) {
                        terminal.echo(item,
                        {
                            raw: true
                        });
                    });  
                }

                _thisPage.GetPrompt(terminal);
            }
        }).fail(function (failReason) {
            response = jQuery.parseJSON(failReason.d);

            terminal.error('**Problem Communicating with Server** ~ ');

            //for a complete separation of business logic, we might want to do this in the page data.
            $.each(response, function (index, item) {
                terminal.echo(item);
            });
        });
    },
    GetPrompt: function(terminal) {
            $.ajax({
                type: 'POST',
                url: 'Default.aspx/Prompt',
                contentType: 'application/json; charset=utf-8',
                dataType: 'json',
                async: false, 
                success: function (retVal) {
                    var response = jQuery.parseJSON(retVal.d);
                    terminal.set_prompt(response);
                }
            }).fail(function () {
                terminal.set_prompt(':X');
            });
    }
});
