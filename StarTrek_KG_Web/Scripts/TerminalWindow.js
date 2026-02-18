(function ($) {
    $.fn.terminalWindow = function (eval, options) {

        if ($('body').data('termWindow')) {
            return $('body').data('termWindow').terminal;
        }

        this.addClass('tilda');

        options = options || {};
        eval = eval || function (command, term) {
            term.echo('you don\'t set eval for term Window');
        };

        var settings = {
            prompt: 'Terminal: ',
            name: 'termWindow',
            height: $(window).height(),
            greetings: 'Star Trek KG \n' +
                       'A modern, C# Code rewrite of the original 1971 Star Trek game by Mike Mayfield, with additional features... :) \n\n' +
                       'Type "start" to begin, or "term menu" for terminal commands\n' +
                       'This application is currently under construction.\n',
            completion: function (terminal, command) {

                $.ajax({
                    type: 'POST',
                    url: 'Default.aspx/AutoComplete',
                    data: '{ input: \'' + command + '\'}',
                    contentType: 'application/json; charset=utf-8',
                    dataType: 'json',
                    async: false,
                    success: function (retVal) {
                        var response = jQuery.parseJSON(retVal.d);

                        var previousText = $('.cmd span:nth-child(2)').text();
                        terminal.set_command(previousText + response);
                    }
                }).fail(function () {      
                });
            }
        };

        $.extend(settings, options);

        this.append('<div class="td"></div>');
        var self = this;

        self.terminal = this.find('.td').terminal(eval, settings);

        $(window).on('resize', function () {
            if (self.terminal) {
                self.terminal.resize($(window).width(), $(window).height());
            }
        });

        $('body').data('termWindow', this);

        return self;
    };
})(jQuery);
