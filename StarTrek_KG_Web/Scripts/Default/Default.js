var _thisPage;

var defaultPage = Class.extend({

    init: function () {
        _thisPage = this; //allows us to ref this object inside this object.
    },
    Reset: function () {
        _thisPage.Start();
    },
    Start: function () {

    }
});
