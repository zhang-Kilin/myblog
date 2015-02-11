(function ($) {
    $.fn.fontFlex = function (options) {
        this.data("fontFlex:options", $.extend({
            //最小字号20px
            min: 20,
            //最大字号80px
            max: 100,
            //缩放比率
            mid: {
                width: 13,
                height: 10
            },
            //字体大小更改后的回调函数
            //options 当前配置
            onchange: function (options) { }
        }, options));

        var self = this;
        resize.call(self);
        $(window).resize(function () {
            resize.call(self);
        });

        return this;
    }

    function resize() {
        var ops = this.data("fontFlex:options"),
            w = $(window).width(),
            h = $(window).height();
        var fontSize_w = w / ops.mid.width,
            fontSize_h = h / ops.mid.height;
        var fontSize = Math.min(fontSize_h, fontSize_w);
        if (fontSize < ops.min) {
            fontSize = ops.min;
        } else if (fontSize > ops.max) {
            fontSize = ops.max;
        }
        this.css({
            "font-size": fontSize + "px"
        });

        if ($.isFunction(ops.onchange)) {
            ops.onchange($.extend({}, ops));
        }
    }
})(jQuery);