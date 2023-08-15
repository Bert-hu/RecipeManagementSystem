$(function () {
    resizeFontSize();
})

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
}

layui.use(['form','util','laydate'], function () {
    var form = layui.form
        , util = layui.util
        , laydate = layui.laydate;

    util.event('lay-active', {
        set: function(othis) {
            Reload($("#reput").val(), $("#startdate").val(), $("#greater").val(), $("#less").val());
        }
    });

    laydate.render({
        elem: '#startdate'
        , type: 'date'
        , format: 'yyyy-MM-dd'
        , done: function (value, date, endDate) {
            Reload($("#reput").val(), $("#startdate").val(), $("#greater").val(), $("#less").val());
        }
    });
})

function Reload(value,date,g,l) {
    $.ajax({
        url: '/DPPM/GetBoxData',
        data: {
            "value": value,
            "startdate": date,
            "greater": g,
            "less": l
        },
        type: 'POST',
        contentType: 'application/x-www-form-urlencoded; charset=UTF-8',//数据类型必须有
        async: false,
        success: function (data) {
            CreateThead(data.dates);
            CreateTbody(data.boxdata);
            CreateRange(data.greater, data.less);
            getratechart("dppm_fan", data.ratedata);
        },
        error: function (message) {
            alert('error!');
        }
    });
}

function CreateThead(dates) {
    var headText = '<tr><td>Robot No.</td><td>Robot Type</td><td>' + dates[0] + '<br />DPPM</td><td>' + dates[0] + '<br />(NG-ALL)</td><td>S1</td><td>S2</td><td>S3</td><td>S4</td><td>S5</td><td>S6</td><td>S7</td><td>S8</td><td>S9</td><td>S10</td><td>N1</td><td>N2</td><td>N3</td><td>N4</td><td>N5</td>';
    for (var i = 1; i < dates.length; i++) {
        headText += '<td>D' + (i + 1) + '</td><td>' + dates[i] + '<br />(NG-ALL)</td>'
    }
    headText += '<td>详情</td></tr>';
    document.getElementById("tablehead").innerHTML = headText;
}

function CreateTbody(data) {
    var bodyText = '';
    for (var i = 0; i < data.length; i++) {
        bodyText += '<tr><td>' + data[i].Equipment + '</td><td>' + data[i].Type + '</td>';
        var rates = data[i].Rates;
        var paras = data[i].Parameters;

        bodyText += '<td><input class="div-radius" style="background-color:' + paras[0].Color + '" readonly>' + paras[0].DPPM + '</td><td>' + paras[0].NG_All + '</td>';

        for (var n = 0; n < rates.length; n++) {
            bodyText += '<td>' + rates[n] + '</td>'
        }

        for (var j = 1; j < paras.length; j++) {
            if (paras[j].NG_All == "0-0") {
                bodyText += '<td></td><td></td>';
            }
            else {
                bodyText += '<td><input class="div-radius" style="background-color:' + paras[j].Color + '" readonly></td><td>' + paras[j].NG_All + '</td>';
            }
        }
        bodyText += '<td><button class="table-btn" onclick="ShowDetails(\'' + data[i].Equipment + '\')">详情</button></td></tr>';
    }
    document.getElementById("tablebody").innerHTML = bodyText;
}

function CreateRange(g,l) {
    document.getElementById("ag").innerHTML = "&gt;" + g;
    document.getElementById("ae").innerHTML = g + "~" + l;
    document.getElementById("al").innerHTML = "&lt;" + l;
    document.getElementById("greater").value = g;
    document.getElementById("less").value = l;
}

function ShowDetails(eqid) {
    layui.use('layer', function () {
        var layer = layui.layer;

        var reput = document.getElementById("reput").value;
        var date = document.getElementById("startdate").value;
        
        layer.open({
            title: 'Box Details'
            , type: 2
            , btn: ['确定']
            , content: '/DPPM/BoxDetails?eqid=' + eqid + '&reput=' + reput + '&date=' + date
            , area: ['70%', ' 80%']
            , yes: function (index, layero) {
                layer.close(index);
            }
        });
    });
}

function getratechart(id,ratedata) {
    var chartDom = document.getElementById(id);
    var myChart = echarts.init(chartDom);

    $(window).on('resize', function () {
        //屏幕大小自适应，重置容器高宽
        chartDom.style.width = window.innerWidth * 0.15 + 'px';
        chartDom.style.height = window.innerHeight * 0.08 + 'px';
        myChart.resize();
    });
    chartDom.style.width = window.innerWidth * 0.15 + 'px';
    chartDom.style.height = window.innerHeight * 0.08 + 'px';

    var option;

    option = {
        tooltip: {
            trigger: 'item',
            formatter: '{b} : {c} ({d}%)'
        },
        series: [
            {
                name: 'DPPM Rate',
                type: 'pie',
                radius: '70%',
                center: ['60%', '50%'],
                data: ratedata,
                labelLine: {
                    show: true,
                    length: 1
                },
                label: {
                    show: true,
                    position: "outside",
                    formatter: '{d}%',
                    color: "#ffffff",
                    fontSize:10
                },
                emphasis: {
                    itemStyle: {
                        shadowBlur: 10,
                        shadowOffsetX: 0,
                        shadowColor: 'rgba(0, 0, 0, 0.5)'
                    }
                }
            }
        ]
    };
    option && myChart.setOption(option);
    myChart.resize();
}