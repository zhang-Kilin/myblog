(function ($) {
    var loading = function (options) {
        var panel = $("<div>")
            .css({
                "position": "fixed",
                "z-index": "9999",                
                "margin": "0",
                "padding": "0",
                "border": "0",
                "background": "url("+options.img+") no-repeat 50% 50%",
                "overflow": "hidden",
                "display":"none"
            })
            .appendTo(document.body);
        this.__panel = panel;
        var self = this;
        $(window).bind("resize", function () {
            self._resize();
        });
    }
    loading.prototype = {
        show: function () {
            this.__status = "showing";
            this.__panel.css({
                "top": $(document).scrollTop() + "px",
                "left": $(document).scrollLeft() + "px",
                "width": $(window).width() + "px",
                "height": $(window).height() + "px"
            }).show();
        },
        close: function () {
            this.__status = "hidden";
            this.__panel.hide();
        },
        _resize: function () {
            if(this.__status === "showing"){
                this.__panel.css({
                    "top": $(document).scrollTop() + "px",
                    "left": $(document).scrollLeft() + "px",
                    "width": $(window).width() + "px",
                    "height": $(window).height() + "px"
                });
            }
        }
    };

    $.loading = function (options) {
        options = $.extend({
            img: "static/pub/images/loading.gif"
        }, options);
        var obj = $(window).data("__loading_target");
        if (!obj) {
            obj = new loading(options);
            $(window).data("__loading_target", obj);
        }
        obj.show();
        return obj;
    }
})(jQuery);