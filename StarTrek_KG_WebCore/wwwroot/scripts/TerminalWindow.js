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
                       'Type "start" or "war games" to begin, or "term menu" for terminal commands\n' +
                       'This application is currently under construction.\n',
            completion: function (terminal, command) {
                if (settings._autocompleteTimer) {
                    clearTimeout(settings._autocompleteTimer);
                }

                settings._autocompleteTimer = setTimeout(function () {
                    if (settings._autocompleteRequest && settings._autocompleteRequest.readyState !== 4) {
                        settings._autocompleteRequest.abort();
                    }

                    settings._autocompleteRequest = $.ajax({
                    type: 'POST',
                    url: 'Default.aspx/AutoComplete',
                    data: '{ input: \'' + command + '\'}',
                    contentType: 'application/json; charset=utf-8',
                    dataType: 'json'
                    });

                    settings._autocompleteRequest.done(function (retVal) {
                        var response = JSON.parse(retVal.d);

                        var previousText = $('.cmd span:nth-child(2)').text();
                        terminal.set_command(previousText + response);
                    });
                }, 120);
            }
        };

        $.extend(settings, options);

        this.append('<div class="td"></div>');
        var self = this;

        self.terminal = this.find('.td').terminal(eval, settings);

        var resizePending = false;
        var lastWidth = -1;
        var lastHeight = -1;

        function applyResize() {
            resizePending = false;
            if (!self.terminal) {
                return;
            }
            var width = $(window).width();
            var height = $(window).height();
            if (width === lastWidth && height === lastHeight) {
                return;
            }
            lastWidth = width;
            lastHeight = height;
            self.terminal.resize(width, height);
        }

        $(window).on('resize', function () {
            if (resizePending) {
                return;
            }
            resizePending = true;
            if (window.requestAnimationFrame) {
                window.requestAnimationFrame(applyResize);
            } else {
                setTimeout(applyResize, 16);
            }
        });

        $('body').data('termWindow', this);

        return self;
    };
})(jQuery);
