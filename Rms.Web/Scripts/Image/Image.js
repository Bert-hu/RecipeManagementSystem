
var searchxm;

$(function () {
    resizeFontSize();
    GetEqplist();

})//



function resizeFontSize() {
    let fullheight = $(window).height();
    $("html").css({ fontSize: fullheight / 10 });
    $(window).resize(function () {
        let fullheight = $(window).height();
        $("html").css({ fontSize: fullheight / 10 });
    });

}

function showFullScreen() {

    /*判断是否全屏*/
    let isFullscreen = document.fullScreenElement//W3C
        || document.msFullscreenElement //IE11
        || document.mozFullScreenElement //火狐
        || document.webkitFullscreenElement //谷歌
        || false;

    if (!isFullscreen) {
        let el = document.documentElement;
        if (el.requestFullscreen) {
            el.requestFullscreen();
        } else if (el.mozRequestFullScreen) {
            el.mozRequestFullScreen();
        } else if (el.webkitRequestFullscreen) {
            el.webkitRequestFullscreen();
        } else if (el.msRequestFullscreen) {
            el.msRequestFullscreen();
        }
        $("#fullScreen").removeClass("fa-expand").addClass("fa-compress");
    }
    else {
        if (document.exitFullscreen) {
            document.exitFullscreen();
        } else if (document.msExitFullscreen) {
            document.msExitFullscreen();
        } else if (document.mozCancelFullScreen) {
            document.mozCancelFullScreen();
        } else if (document.webkitCancelFullScreen) {
            document.webkitCancelFullScreen();
        }
        $("#fullScreen").removeClass("fa-compress").addClass("fa-expand");
    }
};
layui.use(['layer'], function () {
    var layer = layui.layer;//预加载一下，不然第一次打开会有问题


})


function GetEqplist() {
    $.ajax({
        type: 'post',
        dataType: 'json',
        url: '/Image/GetEqplist',
        success: function (data) {
            scrollEqpDetail(data);
            SelectEqp(data[0].EQUIPMENT);
            var searchdta = [];
            searchdta.push({ name: "all", value: "" })
            data.forEach(function (item) {
                console.log(item);
                searchdta.push({ name: item.EQUIPMENT, value: item.EQUIPMENT })
            })


            var searchxm = xmSelect.render({
                el: '#search',
                tips: 'Equipment',
                filterable: true,
                theme: {
                    color: '#1cbbb4',
                },
                radio: true,
                data: searchdta,
                on: function (data) {

                    var change = data.change;
                    var seleqp = change[0].value;
                    if (seleqp == '') {
                        GetEqplist();
                    }
                    else {
                        SelectEqp(seleqp);
                    }
                  

                   

                },
            })
        },
        error: function () {
        }
    });
}


var MyMarhq = '';
function scrollEqpDetail(Items) {

    clearInterval(MyMarhq);

    $('.tbl-body tbody').empty();
    $('.tbl-header tbody').empty();
    var str = '';
    $.each(Items, function (i, item) {
        str = '<tr onclick="SelectEqp(\'' + item.EQUIPMENT + '\');">' +
            '<td>' + item.EQUIPMENT + '</td>' +
            '<td>' + item.TIME + '</td>' +
            '</tr>'

        $('.tbl-body tbody').append(str);
        $('.tbl-header tbody').append(str);
    });

    if (Items.length >= 999) {
        $('.tbl-body tbody').html($('.tbl-body tbody').html() + $('.tbl-body tbody').html());
        $('.tbl-body').css('top', '0');
        var tblTop = 0;
        var speedhq = 30; // 数值越大越慢
        var outerHeight = $('.tbl-body tbody').find("tr").outerHeight();
        function Marqueehq() {
            if (tblTop <= -outerHeight * Items.length) {
                tblTop = 0;
            } else {
                tblTop -= 1;
            }
            $('.tbl-body').css('top', tblTop + 'px');
        }

        MyMarhq = setInterval(Marqueehq, speedhq);

        // 鼠标移上去取消事件
        $(".tbl-header tbody").hover(function () {
            clearInterval(MyMarhq);
        }, function () {
            clearInterval(MyMarhq);
            MyMarhq = setInterval(Marqueehq, speedhq);
        })

    }
}

function SelectEqp(eqp) {
    GetStationlist(eqp);
    GetImagelist(eqp, "");
}

function GetStationlist(eqp) {
    currenteqp = eqp;
    $.ajax({
        type: 'post',
        dataType: 'json',
        data: {
            eqp: eqp
        },
        url: '/Image/GetStationlist',
        success: function (data) {
            scrollStationDetail(data);
        },
        error: function () {
        }
    });
}

var MyMarhq1 = '';
var currenteqp = '';
function scrollStationDetail(Items) {

    clearInterval(MyMarhq1);

    $('.tbl-body2 tbody').empty();
    $('.tbl-header2 tbody').empty();
    var str = '';
    $.each(Items, function (i, item) {
        str = '<tr onclick="SelectStation(this)">' +
            '<td>' + item.STATION + '</td>' +

            '</tr>'

        $('.tbl-body2 tbody').append(str);
        $('.tbl-header2 tbody').append(str);
    });

    if (Items.length >= 999) {
        $('.tbl-body2 tbody').html($('.tbl-body2 tbody').html() + $('.tbl-body2 tbody').html());
        $('.tbl-body2').css('top', '0');
        var tblTop = 0;
        var speedhq = 30; // 数值越大越慢
        var outerHeight = $('.tbl-body2 tbody').find("tr").outerHeight();
        function Marqueehq() {
            if (tblTop <= -outerHeight * Items.length) {
                tblTop = 0;
            } else {
                tblTop -= 1;
            }
            $('.tbl-body2').css('top', tblTop + 'px');
        }

        MyMarhq1 = setInterval(Marqueehq, speedhq);

        // 鼠标移上去取消事件
        $(".tbl-header2 tbody").hover(function () {
            clearInterval(MyMarhq1);
        }, function () {
            clearInterval(MyMarhq1);
            MyMarhq1 = setInterval(Marqueehq, speedhq);
        })

    }
}

function SelectStation(station) {
    console.log(station.childNodes[0].innerText);
    GetImagelist(currenteqp, station.childNodes[0].innerText);
}

function GetImagelist(eqp, station) {
    $.ajax({
        type: 'post',
        dataType: 'json',
        data: {
            eqp: eqp,
            station: station
        },
        url: '/Image/GetImagelist',
        success: function (data) {
            SetImageDetail(data);
            ShowImage(data[0].ID);
        },
        error: function () {
        }
    });
}

var MyMarhq3 = '';
function SetImageDetail(Items) {

    clearInterval(MyMarhq3);

    $('.tbl-body3 tbody').empty();
    $('.tbl-header3 tbody').empty();
    var str = '';
    $.each(Items, function (i, item) {
        str = '<tr onclick="ShowImage(\'' + item.ID + '\');">' +

            '<td>' + item.EQUIPMENT + '</td>' +
            '<td>' + item.STATION + '</td>' +
            '<td>' + item.RESULT + '</td>' +
            '<td>' + item.TIME + '</td>' +
            '<td>' + item.REJUDGE_RESULT + '</td>' +
            '</tr>'

        $('.tbl-body3 tbody').append(str);
        //$('.tbl-header3 tbody').append(str);
    });

    if (Items.length >= 999) {
        $('.tbl-body3 tbody').html($('.tbl-body3 tbody').html() + $('.tbl-body3 tbody').html());
        $('.tbl-body3').css('top', '0');
        var tblTop = 0;
        var speedhq = 100; // 数值越大越慢
        var outerHeight = $('.tbl-body3 tbody').find("tr").outerHeight();
        function Marqueehq() {
            if (tblTop <= -outerHeight * Items.length) {
                tblTop = 0;
            } else {
                tblTop -= 1;
            }
            $('.tbl-body3').css('top', tblTop + 'px');
        }

        MyMarhq3 = setInterval(Marqueehq, speedhq);

        // 鼠标移上去取消事件
        $(".tbl-header3 tbody").hover(function () {
            clearInterval(MyMarhq3);
        }, function () {
            clearInterval(MyMarhq3);
            MyMarhq3 = setInterval(Marqueehq, speedhq);
        })

    }
}


function ShowImage(imgID) {
    $.ajax({
        type: 'post',
        dataType: 'json',
        data: {
            imgid: imgID,
        },
        url: '/Image/ShowImage',
        success: function (retdata) {

            console.log(retdata);
            document.getElementById("eqpimage").innerHTML =

                "<img src='" + retdata + "' style='max-width: 100%; max-height: 100%;' onclick='ShowImageLayer(\"" + imgID + "\");'/>";


            document.getElementById("btn_details").innerHTML = '<button type="button" class="layui-btn layui-btn-normal" onclick="ShowImageLayer(\'' + imgID + '\');">Details</button>';

            ShowImageDetail(imgID);
            

            //var image = retdata.fileurl;
            //console.log(image);

            CountDown(59);

        },
        error: function () {
            layer.close(loading);
        }
    });
}

function ShowImageDetail(imgID) {
    $.ajax({
        type: 'post',
        dataType: 'json',
        data: {
            imgid: imgID,
        },
        url: '/Image/ShowImageDetail',
        success: function (retdata) {

            console.log(retdata.STATION);

            document.getElementById("eqp").innerHTML = retdata.EQUIPMENT;
            document.getElementById("station").innerHTML = retdata.STATION;

            document.getElementById("dateTime").innerHTML = FormDate(retdata.EVENT_TIME, "yyyy-MM-dd HH:mm");

        },
        error: function () {

        }
    });
}


function ShowImageLayer(imgID) {

    layui.use(['layer'], function () {
        var layer = layui.layer;
        layer.open({
            title: 'Box Details'
            , type: 2
            , btn: ['AI判断有误', 'AI判断无误']
            , shade: [0.8, '#393D49']
            , shadeClose: true
            , content: '/Image/ShowImageLayer?id=' + imgID
            , area: ['90%', ' 90%']
            , yes: function (index, layero) {
                Rejudge(imgID, "Error");
                layer.close(index);
            }
            , btn2: function (index, layero) {
                //按钮【按钮二】的回调
                Rejudge(imgID, "Errorless");
                //return false 开启该代码可禁止点击该按钮关闭
            }
        });

    })
}

function Rejudge(id, result) {
    $.ajax({
        type: 'post',
        dataType: 'json',
        data: {
            id: id,
            result: result
        },
        url: '/Image/Rejudge',
        success: function (retdata) {
            //console.log(retdata);
            return retdata;
        }
    });
}


var flag = ' ';
function CountDown(time) {
    clearInterval(flag);
    var timeName = document.getElementById("tim");
    var t = time;
    flag = setInterval(function () {
        timeName.innerHTML = t + "s后刷新页面";
        t--;
        if (t < 0) {
            t = time;
            GetEqplist();
            //console.log("refresh");
            clearInterval(flag);
        }


    }, 1000);


}
function FormDate(str, fmt) { //str: 日期字符串；fmt:格式类型
    if (str == null || str == '') {
        return "";
    }
    var date = eval('new ' + str.substr(1, str.length - 2)); //截取字符串之后：Date(1572490889017)
    var o = {
        "M+": date.getMonth() + 1, //月份
        "d+": date.getDate(), //日
        "H+": date.getHours(), //小时
        "m+": date.getMinutes(), //分
        "s+": date.getSeconds(), //秒
        "q+": Math.floor((date.getMonth() + 3) / 3), //季度
        "S": date.getMilliseconds() //毫秒
    };
    if (/(y+)/.test(fmt)) fmt = fmt.replace(RegExp.$1, (date.getFullYear() + "").substr(4 - RegExp.$1.length));
    for (var k in o)
        if (new RegExp("(" + k + ")").test(fmt)) fmt = fmt.replace(RegExp.$1, (RegExp.$1.length == 1) ? (o[k]) : (("00" + o[k]).substr(("" + o[k]).length)));
    return fmt;
}