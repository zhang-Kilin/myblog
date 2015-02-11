(function ($) {
    var bgResize = function (options) {
        this.options = $.extend({
            //图片切换的时长
            //fast = 3000ms, normal = 6000ms, slow = 12000ms
            //number = 自定义时间
            speed: "normal",
            //是否启动切换动画
            //true : 启动
            //false : 不启动
            animate: true,
            //切换过程中负责动画处理的回调函数
            //ele1 : 需要被隐藏的jQuery对象
            //ele2 : 需要显示出来的jQuery对象
            //fn : 动画播放完毕的回调函数
            //onanimating: null//function (ele1,ele2,fn) { }
        }, options);

        //动画播放完毕后需要执行的事件队列
        this.animateEventQueue = [];
        //
        switch (this.options.speed) {
            case "fast":
                this.options.speed = 6000;
                break;
            case "slow":
                this.options.speed = 18000;
                break;
            default:
                if (!this.options.speed || isNaN(this.options.speed)) {
                    this.options.speed = 12000;
                }
                break;
        }
    }
    bgResize.queue = [];

    bgResize.prototype = {
        //ele 要进行渲染的节点
        init: function (ele) {
            this.clear();
            var container = this.container = $("body");
            var children = this.children = [];
            var self = this;
            container.css("overflow", "hidden");
            ele.find(">img").each(function (i) {
                var _this = $(this);
                var url = _this.attr("url");
                var opt = {};
                $("<img/>").data("_index", i).css({
                    "z-index": "-100"
                }).attr("src", url).load(function () {
                    var width = opt.width = this.width,
                        height = opt.height = this.height;
                    var _index = $(this).data("_index");
                    var div = $("<div></div>")
                                .css({
                                    "background": "transparent",
                                    "opacity": "0",
                                    "position": "fixed",
                                    "top": "0px",
                                    "left": "0px",
                                    "z-index": "-1"
                                })
                                .appendTo(container)
                                .append(this);
                    resizeElement.call(self, div);
                    if (_this.attr("title")) {
                        var div_title = $("<div></div>")
                                        .css({
                                            "font-size": "80px",
                                            "font-weight": "bold",
                                            "font-family": "'BebasNeueRegular', 'Arial Narrow', Arial, sans-serif",
                                            "color": "rgba(169,3,41, 0.8)",
                                            "text-align": "center",
                                            "height": "auto",
                                            "width": "100%",
                                            "opacity": "0",
                                            "position": "fixed",
                                            "top": "0",
                                            "left": "0"
                                        })
                                        .text(_this.attr("title"))
                                        .appendTo(div)
                                        .fontFlex();
                    }
                    children.push(div);
                    if (_index == 0) {
                        //计算尺寸
                        self._current = children[0].css("opacity", "1");
                        //self._current.children("div").css("opacity", "1");
                        self._slideTitle(self._current.children("div"));
                        self._zoom(self._current.children("img"), 1.15);
                    }
                });
            });
            ele.remove();
            $(window).bind("resize", function () {
                resize.call(self);
            });

            var index = 0,
                times = self.options.speed / 100;
            setInterval(function () {
                if (times > 0) {
                    times--;
                }
                if (--times > 0) {
                    return;
                }
                //窗口大小变化完成2秒后进行动画
                if (new Date().getTime() - (self._resizeTime || 0) <= 2000) {
                    return;
                }
                if (self._isAnimating) {
                    return;
                }
                times = self.options.speed / 100;
                self._isAnimating = true;
                index++;
                if (index >= children.length) {
                    index = 0;
                }
                var old = self._current,
                    current = self._current = children[index];
                self._animate(old, current, function () {
                    self._isAnimating = false;
                    var flag = false;
                    while (self.animateEventQueue.length) {
                        flag = true;
                        var fn = self.animateEventQueue.pop();
                        fn();
                    }
                    if (flag) {
                        self._isResizing = false;
                    }
                });
            }, 100);
        },
        //清空所有的背景图
        clear: function () {
            if (this.children) {
                $(this.children).each(function () {
                    this.remove();
                });
            }
            this.container = null;
        },
        //默认的动画播放函数
        _animate: function (ele1, ele2, fn) {
            var speed = this.options.speed / 5,
                img = ele2.children("img"),
                ops = ele1.children("img").data("options");
            var self = this;
            speed = speed > 2000 ? 2000 : speed;
            ele1.animate({
                opacity: 0
            }, speed, function () {
                $(this).children("img").css({
                    top: ops.top + "px",
                    left: ops.left + "px",
                    width: ops.width + "px",
                    height: ops.height + "px"
                });
            })
            .children("div").animate({
                opacity: 0
            }, speed, function () {

            });

            ele2.animate({
                opacity: 1
            }, speed, function () {
                //计算title的位置，并播放动画
                var title_panel = $(this).children("div");
                self._slideTitle(title_panel);
                //播放背景图的动画
                self._zoom(ele2.children("img"), 1.15, fn);
            });
        },
        //放大图片
        //zoom : 放大的倍数
        //fn : 放大完成后的回调函数
        _zoom: function (img, zoom, fn) {
            var speed = this.options.speed * 0.3;
            var this_width = img.width(),
                this_height = img.height(),
                this_left = img.offset().left,
                this_top = img.offset().top;
            var width = this_width * zoom,
                height = this_height * zoom,
                left = this_left - (width - this_width) / 2,
                top = this_top - (height - this_height) / 2;

            speed = speed > 2000 ? 2000 : speed;
            //记录原始位置和大小
            var ops = {
                left: this_left,
                top: this_top,
                width: this_width,
                height: this_height
            };
            img.data("options", ops);
            img.animate({
                top: top + "px",
                left: left + "px",
                width: width + "px",
                height: height + "px"
            }, speed, fn);
        },
        //坠落title的动画效果
        _slideTitle: function (panel, fn) {
            if (panel.length > 0) {
                var speed = this.options.speed * 0.3,
                    height_title = panel.height(),
                    height_window = $(window).height();
                var top = height_window - (speed * 0.3 * 0.143 + height_title);
                panel.css("top", top + "px");
                speed = speed > 2000 ? 2000 : speed;
                panel.animate({
                    top: (height_window - height_title) + "px",
                    opacity: 1
                }, speed, fn);
            } else {
                if ($.isFunction(fn)) {
                    fn();
                }
            }
        },
        _fontFlex: function (panel) {
            //计算title的位置
            //var title_panel = child.children("div");
            panel.css("top", ($(window).height() - panel.height()) + "px");
        }
    };

    //自适应大小
    function resize() {
        var self = this;
        self._resizeTime = new Date().getTime();
        //注册动画播放完毕的回调事件
        if (self._isAnimating) {
            self.animateEventQueue.push(function () {
                resize.call(self);
            });
            return;
        }

        if (self.children) {
            $(self.children).each(function () {
                resizeElement.call(self, $(this));
            });
        }
        self._resizeTime = 0;
    }

    function resizeElement(child) {
        var w = $(window);
        if (!w) {
            return;
        }
        child.css({
            "overflow": "hidden",
            "width": w.width() + "px",
            "height": w.height() + "px"
            //"opacity": 1
        });
        var img = child.children("img").css("position", "relative");
        var iw = img.width();
        var ih = img.height();
        if (w.width() > w.height()) {
            if (iw > ih) {
                var fRatio = iw / ih;
                img.css("width", w.width() + "px");
                img.css("height", Math.round(w.width() * (1 / fRatio)));

                var newIh = Math.round(w.width() * (1 / fRatio));

                if (newIh < w.height()) {
                    var fRatio = ih / iw;
                    img.css("height", w.height());
                    img.css("width", Math.round(w.height() * (1 / fRatio)));
                }
            } else {
                var fRatio = ih / iw;
                img.css("height", w.height());
                img.css("width", Math.round(w.height() * (1 / fRatio)));
            }
        } else {
            var fRatio = ih / iw;
            img.css("height", w.height());
            img.css("width", Math.round(w.height() * (1 / fRatio)));
        }

        if (img.width() > w.width()) {
            var this_left = (img.width() - w.width()) / 2;
            img.css({
                "top": 0,
                "left": -this_left
            });
        }
        if (img.height() > w.height()) {
            var this_height = (img.height() - w.height()) / 2;
            img.css({
                "left": 0,
                "top": -this_height
            });
        }
        //记录原始位置和大小
        img.data("options", {
            left: img.offset().left,
            top: img.offset().top,
            width: img.width(),
            height: img.height()
        });

        this._fontFlex(child.children("div"));
    }

    //导出到jQuery插件
    $.fn.bgResize = function (options) {
        var t = new bgResize(options);
        bgResize.queue.push(t);
        t.init(this);
        return this;
    }
})(jQuery);