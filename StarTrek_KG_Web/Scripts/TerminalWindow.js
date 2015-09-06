(function ($) {
    $.fn.terminalWindow = function (eval, options) {

        if ($('body').data('termWindow')) {
            return $('body').data('termWindow').terminal;
        }

        this.addClass('tilda');

        options = options || {};
        eval = eval || function (command, term) {
            term.echo("you don't set eval for term Window");
        };

        var settings = {
            prompt: 'Terminal: ',
            name: 'termWindow',
            height: 600,
            enabled: true,
            greetings: 'Star Trek KG \n' +
                       'A modern, C# Code rewrite of the original 1971 Star Trek game by Mike Mayfield, with additional features... :) \n\n' +
                       'Type "start" to begin, or "term menu" for terminal commands\n'
        };

        $.extend(settings, options);

        this.append('<div class="td"></div>');
        var self = this;

        self.terminal = this.find('.td').terminal(eval, settings);

        $('body').data('termWindow', this);

        return self;
    };
})(jQuery);