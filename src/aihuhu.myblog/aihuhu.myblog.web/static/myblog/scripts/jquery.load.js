//延迟加载
(function ($) {
    //url:页面地址
    //options:加载参数
    //fn:加载完成后的回调函数
    $.load = function (url,params,fn) {
        if ($.isFunction(params)) {
            fn = params;
            params = {};
        }
        var loading = $.loading();
        $.get(url, params, function (responseText) {
            $(document.body).html(responseText);
            loading.close();
        });
    }
})(jQuery);