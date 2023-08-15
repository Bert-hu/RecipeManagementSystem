$(function () {
    resizeFontSize();
    GetEqp();
})//

setInterval("GetEqp()", 2000);
setInterval("GetData()", 30000);

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

function GetEqp() {
    $.ajax({
        type: 'get',
        dataType: 'json',
        url: "/Robot/GetEqp",
        asyn: false,
        data: {

        },
        success: function (data) {
            //var data = JSON.parse(datastr);
            var currentEqp = document.getElementById("inputEquipment").value;
            if (currentEqp != data.equipment) {
                document.getElementById("inputEquipment").value = data.equipment;
                document.getElementById("inputEquipment").innerText = data.equipment;

                GetData();
            }
        },
        error: function () {
            console.log("error");
        }
    });
}

function GetData() {
    var eqp = document.getElementById("inputEquipment").innerText;
    GetIndicator(eqp);
    GetBoxMtba(eqp);
    GetBoxOutput(eqp);
    GetAlarms(eqp);
    GetTrends(eqp);
}

function GetIndicator(eqp) {
    $.ajax({
        type: 'post',
        dataType: 'json',
        data: {
            "eqp": eqp,
        },
        url: '/Box/GetIndicator',
        success: function (data) {
            document.getElementById("indOutput").innerText = data.OUTPUT;
            document.getElementById("indMtba").innerText = data.MTBA;
            document.getElementById("indOee").innerText = data.OEE+"%" ;
            document.getElementById("indYield").innerText = data.YIELD + "%" ;

        },
        error: function () {
        }
    });
}

function GetBoxMtba(eqp) {
    $.ajax({
        type: 'post',
        dataType: 'json',
        data: {
            "eqp": eqp,
        },
        url: '/Box/GetBoxMtba',
        success: function (data) {
            SetBoxMTBAChart(data);

        },
        error: function () {
        }
    });
}

function GetBoxOutput(eqp) {
    $.ajax({
        type: 'post',
        dataType: 'json',
        data: {
            "eqp": eqp,
        },
        url: '/Box/GetBoxOutput',
        success: function (data) {
            SetBoxOutputChart(data);

        },
        error: function () {
        }
    });
}

function GetAlarms(eqp) {
    $.ajax({
        type: 'post',
        dataType: 'json',
        data: {
            "eqp": eqp,
        },
        url: '/Box/GetAlarms',
        success: function (data) {
            scrollRobotAlarmDetail(data.robotalarms);
            scrollBoxAlarmDetail(data.boxalarms);
        },
        error: function () {
        }
    });
}

function GetTrends(eqp) {
    $.ajax({
        type: 'post',
        dataType: 'json',
        data: {
            "eqp": eqp,
        },
        url: '/Box/GetTrends',
        success: function (data) {
            SetOutputTrend(data);
            SetOEETrend(data);
            SetYieldTrend(data);
        },
        error: function () {
        }
    });
}

function SetOutputTrend(data) {
    var myChart = echarts.init(document.getElementById('outputTrendChart'));
    var xArr = [];
    var yArr = [];

    for (var i = 0; i < data.length; i++) {
        xArr.push(data[i].DATETIME)
        //jingArr.push(resArr[i].value)
        yArr.push(data[i].OUTPUT)
    }

    option = {
        tooltip: {

            trigger: 'axis',
            axisPointer: {
                lineStyle: {
                    color: '#57617B'
                }
            },
            formatter: '{b}:<br/> Output{c}'
        },

        grid: {
            left: '0',
            right: '5%',
            top: '20%',
            bottom: '0',
            containLabel: true
        },
        legend: {
            x: '40%',
            y: '0%',
            data: ['Output'],
            textStyle: {
                color: "#fff",
                fontSize: 15
            },
            itemWidth: 10,
            itemHeight: 10,

        },
        xAxis: [{
            type: 'category',
            boundaryGap: false,
            axisLabel: {
                show: true,
                textStyle: {
                    color: 'rgba(255,255,255,.6)'
                }
            },
            axisLine: {
                lineStyle: {
                    color: 'rgba(255,255,255,.1)'
                }
            },
            data: xArr
        }],
        yAxis: [{
            axisLabel: {
                show: true,
                textStyle: {
                    color: 'rgba(255,255,255,.6)'
                }
            },
            axisLine: {
                lineStyle: {
                    color: 'rgba(255,255,255,.1)'
                }
            },
            splitLine: {
                lineStyle: {
                    color: 'rgba(255,255,255,.1)'
                }
            }
        }],
        series: [{
            name: 'Output',
            type: 'line',
            smooth: true,
            symbol: 'circle',
            symbolSize: 5,
            showSymbol: true,
            lineStyle: {
                normal: {
                    width: 3
                }
            },
            areaStyle: {
                normal: {
                    color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [{
                        offset: 0,
                        color: 'rgba(98, 201, 141, 0.5)'
                    }, {
                        offset: 1,
                        color: 'rgba(98, 201, 141, 0.1)'
                    }], false),
                    shadowColor: 'rgba(0, 0, 0, 0.1)',
                    shadowBlur: 10
                }
            },
            itemStyle: {
                normal: {
                    color: '#4cb9cf',
                    borderColor: 'rgba(98, 201, 141,0.27)',
                    borderWidth: 12,
                    label: { show: true },
                    symbolSize: 10,
                }
            },
            data: yArr
        }]
    };
    /*   */
    // 使用刚指定的配置项和数据显示图表。
    myChart.setOption(option);
    window.addEventListener("resize", function () {
        myChart.resize();
    });
}

function SetOEETrend(data) {
    var myChart = echarts.init(document.getElementById('oeeTrendChart'));
    var xArr = [];
    var yArr = [];

    for (var i = 0; i < data.length; i++) {
        xArr.push(data[i].DATETIME)
        //jingArr.push(resArr[i].value)
        yArr.push(data[i].OEE)
    }

    option = {
        tooltip: {

            trigger: 'axis',
            axisPointer: {
                lineStyle: {
                    color: '#57617B'
                }
            },
            formatter: '{b}:<br/> OEE: {c}%'
        },

        grid: {
            left: '0',
            right: '5%',
            top: '20%',
            bottom: '0',
            containLabel: true
        },
        legend: {
            x: '40%',
            y: '0%',
            data: ['OEE'],
            textStyle: {
                color: "#fff",
                fontSize: 15
            },
            itemWidth: 10,
            itemHeight: 10,

        },
        xAxis: [{
            type: 'category',
            boundaryGap: false,
            axisLabel: {
                show: true,
                textStyle: {
                    color: 'rgba(255,255,255,.6)'
                }
            },
            axisLine: {
                lineStyle: {
                    color: 'rgba(255,255,255,.1)'
                }
            },
            data: xArr
        }],
        yAxis: [{
            axisLabel: {
                show: true,
                textStyle: {
                    color: 'rgba(255,255,255,.6)'
                },
                formatter: '{value} %'
            },
            axisLine: {
                lineStyle: {
                    color: 'rgba(255,255,255,.1)'
                }
            },
            splitLine: {
                lineStyle: {
                    color: 'rgba(255,255,255,.1)'
                }
            }
        }],
        series: [{
            name: 'OEE',
            type: 'line',
            smooth: true,
            symbol: 'circle',
            symbolSize: 5,
            showSymbol: true,
            lineStyle: {
                normal: {
                    width: 3
                }
            },
            areaStyle: {
                normal: {
                    color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [{
                        offset: 0,
                        color: 'rgba(198, 201, 141, 0.5)'
                    }, {
                        offset: 1,
                        color: 'rgba(198, 201, 141, 0.1)'
                    }], false),
                    shadowColor: 'rgba(0, 0, 0, 0.1)',
                    shadowBlur: 10
                }
            },
            itemStyle: {
                normal: {
                    color: 'rgba(198, 201, 141, 1)',
                    borderColor: 'rgba(198, 201, 141,0.27)',
                    borderWidth: 12,
                    label: { show: true, formatter: '{c} %' },
                    symbolSize: 10,
                }
            },
            data: yArr
        }]
    };
    /*   */
    // 使用刚指定的配置项和数据显示图表。
    myChart.setOption(option);
    window.addEventListener("resize", function () {
        myChart.resize();
    });
}

function SetYieldTrend(data) {
    var myChart = echarts.init(document.getElementById('yieldTrendChart'));
    var xArr = [];
    var yArr = [];

    for (var i = 0; i < data.length; i++) {
        xArr.push(data[i].DATETIME)
        //jingArr.push(resArr[i].value)
        yArr.push(data[i].YIELD)
    }

    option = {
        tooltip: {

            trigger: 'axis',
            axisPointer: {
                lineStyle: {
                    color: '#57617B'
                }
            },
            formatter: '{b}:<br/> Yield: {c}%'
        },

        grid: {
            left: '0',
            right: '5%',
            top: '20%',
            bottom: '0',
            containLabel: true
        },
        legend: {
            x: '40%',
            y: '0%',
            data: ['Yield'],
            textStyle: {
                color: "#fff",
                fontSize: 15
            },
            itemWidth: 10,
            itemHeight: 10,

        },
        xAxis: [{
            type: 'category',
            boundaryGap: false,
            axisLabel: {
                show: true,
                textStyle: {
                    color: 'rgba(255,255,255,.6)'
                }
            },
            axisLine: {
                lineStyle: {
                    color: 'rgba(255,255,255,.1)'
                }
            },
            data: xArr
        }],
        yAxis: [{
            axisLabel: {
                show: true,
                textStyle: {
                    color: 'rgba(255,255,255,.6)'
                },
                formatter: '{value} %'
            },
            axisLine: {
                lineStyle: {
                    color: 'rgba(255,255,255,.1)'
                }
            },
            splitLine: {
                lineStyle: {
                    color: 'rgba(255,255,255,.1)'
                }
            }
        }],
        series: [{
            name: 'Yield',
            type: 'line',
            smooth: true,
            symbol: 'circle',
            symbolSize: 5,
            showSymbol: true,
            lineStyle: {
                normal: {
                    width: 3
                }
            },
            areaStyle: {
                normal: {
                    color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [{
                        offset: 0,
                        color: 'rgba(98, 201, 41, 0.5)'
                    }, {
                        offset: 1,
                        color: 'rgba(98, 201, 41, 0.1)'
                    }], false),
                    shadowColor: 'rgba(0, 0, 0, 0.1)',
                    shadowBlur: 10
                }
            },
            itemStyle: {
                normal: {
                    color: 'rgba(98, 201, 41, 1)',
                    borderColor: 'rgba(98, 201, 41,0.27)',
                    borderWidth: 12,
                    label: { show: true, formatter: '{c} %' },
                    symbolSize: 10,
                }
            },
            data: yArr
        }]
    };
    /*   */
    // 使用刚指定的配置项和数据显示图表。
    myChart.setOption(option);
    window.addEventListener("resize", function () {
        myChart.resize();
    });
}


function SetBoxMTBAChart(data) {
    //var resArr = [
    //    { name: '测试11', value: 300 },
    //    { name: '测试22', value: 500 },
    //    { name: '测试33', value: 400 },
    //    { name: '测试44', value: 350 },
    //    { name: '测试55', value: 363 },
    //    { name: '测试66', value: 800 },
    //    { name: '测试66', value: 800 },
    //    { name: '测试66', value: 800 },
    //    { name: '测试66', value: 800 },
    //    { name: '测试66', value: 800 },
    //];
    var resArr = data;

    var xunArr = []
    var jingArr = []
    var dateArr = []
    for (var i = 0; i < resArr.length; i++) {
        xunArr.push(resArr[i].value)
        //jingArr.push(resArr[i].value)
        dateArr.push(resArr[i].name)
    }
    var publicNumChart = echarts.init(document.getElementById('mtbaChart'));
    var option = {
        tooltip: {
            trigger: 'axis'
        },
        legend: {
            x: '50%',
            y: '0%',
            data: ['MTBA'],
            textStyle: {
                color: "#fff",
                fontSize: 15
            },
            itemWidth: 10,
            itemHeight: 10,
        },
        calculable: true,
        xAxis: [
            {
                type: 'category',
                data: dateArr,
                axisLabel: {
                    interval: 0,
                    textStyle: {
                        fontSize: 15,
                        color: 'rgba(255,255,255,.7)',
                    }
                },
                "axisTick": {       //y轴刻度线
                    "show": false
                },
                "axisLine": {       //y轴
                    "show": false,
                },
            }
        ],
        yAxis: [
            {
                type: 'value',
                scale: true,
                //name: '单位：%',
                nameTextStyle: {
                    color: 'rgba(255,255,255,.7)',
                    fontSize: 15
                },
                max: 1440,
                min: 0,
                boundaryGap: [0.2, 0.2],
                "axisTick": {       //y轴刻度线
                    "show": false
                },
                "axisLine": {       //y轴
                    "show": false,
                },
                axisLabel: {
                    textStyle: {
                        color: 'rgba(255,255,255,.8)',
                        fontSize: 15
                        // opacity: 0.1,
                    }
                },
                splitLine: {  //决定是否显示坐标中网格
                    show: true,
                    lineStyle: {
                        color: ['#fff'],
                        opacity: 0.2
                    }
                },
            }
        ],
        color: ['#38EB70', '#2E8CFF' ],
        grid: {
            left: '5%',
            right: '1%',
            top: '20%',
            bottom: '15%'
            // containLabel: true
        },
        series: [
            {
                animationDuration: 2500,
                barWidth: '50%',
                name: 'MTBA',
                type: 'bar',
                data: xunArr,
            }
             //   ,
            //{
            //    barWidth: '20%',
            //    name: '警示',
            //    type: 'bar',
            //    data: jingArr,
            //}
        ],
        animationEasing: 'cubicOut'
    };
    publicNumChart.setOption(option);
    window.addEventListener("resize", function () {
        publicNumChart.resize();
    });

}


function SetBoxOutputChart(data) {
    //var resArr = [
    //    { name: '测试11', value: 300 },
    //    { name: '测试22', value: 500 },
    //    { name: '测试33', value: 400 },
    //    { name: '测试44', value: 350 },
    //    { name: '测试55', value: 363 },
    //    { name: '测试66', value: 800 },
    //    { name: '测试66', value: 800 },
    //    { name: '测试66', value: 800 },
    //    { name: '测试66', value: 800 },
    //    { name: '测试66', value: 800 },
    //];
    var resArr = data;

    var xunArr = []
    var jingArr = []
    var dateArr = []
    for (var i = 0; i < resArr.length; i++) {
        xunArr.push(resArr[i].value)
        //jingArr.push(resArr[i].value)
        dateArr.push(resArr[i].name)
    }
    var publicNumChart = echarts.init(document.getElementById('outputChart'));
    var option = {
        tooltip: {
            trigger: 'axis'
        },
        legend: {
            x: '50%',
            y: '0%',
            data: ['Output'],
            textStyle: {
                color: "#fff",
                fontSize: 15
            },
            itemWidth: 10,
            itemHeight: 10,
        },
        calculable: true,
        xAxis: [
            {
                type: 'category',
                data: dateArr,
                axisLabel: {
                    interval: 0,
                    textStyle: {
                        fontSize: 15,
                        color: 'rgba(255,255,255,.7)',
                    }
                },
                "axisTick": {       //y轴刻度线
                    "show": false
                },
                "axisLine": {       //y轴
                    "show": false,
                },
            }
        ],
        yAxis: [
            {
                type: 'value',
                scale: true,
                //name: '单位：%',
                nameTextStyle: {
                    color: 'rgba(255,255,255,.7)',
                    fontSize: 15
                },
                
                min: 0,
                boundaryGap: [0.2, 0.2],
                "axisTick": {       //y轴刻度线
                    "show": false
                },
                "axisLine": {       //y轴
                    "show": false,
                },
                axisLabel: {
                    textStyle: {
                        color: 'rgba(255,255,255,.8)',
                        fontSize: 15
                        // opacity: 0.1,
                    }
                },
                splitLine: {  //决定是否显示坐标中网格
                    show: true,
                    lineStyle: {
                        color: ['#fff'],
                        opacity: 0.2
                    }
                },
            }
        ],
        color: [ '#2E8CFF'],
        grid: {
            left: '5%',
            right: '1%',
            top: '20%',
            bottom: '15%'
            // containLabel: true
        },
        series: [
            {
                animationDuration: 2500,
                barWidth: '50%',
                name: 'Output',
                type: 'bar',
                data: xunArr,
            }
            //   ,
            //{
            //    barWidth: '20%',
            //    name: '警示',
            //    type: 'bar',
            //    data: jingArr,
            //}
        ],
        animationEasing: 'cubicOut'
    };
    publicNumChart.setOption(option)
    window.addEventListener("resize", function () {
        publicNumChart.resize();
    });
}

var MyMarhq = '';
function scrollRobotAlarmDetail(Items) {

    clearInterval(MyMarhq);

    $('.tbl-body tbody').empty();
    $('.tbl-header tbody').empty();
    var str = '';
    $.each(Items, function (i, item) {
        str = '<tr>' +
            '<td>' + item.ALARM_CODE + '</td>' +
            '<td>' + item.START_TIME + '</td>' +
            '<td>' + item.END_TIME + '</td>' +
            '</tr>'

        $('.tbl-body tbody').append(str);
        $('.tbl-header tbody').append(str);
    });

    if (Items.length >= 5) {
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

var MyMarhq2 = '';
function scrollBoxAlarmDetail(Items) {

    clearInterval(MyMarhq2);

    $('.tbl-body2 tbody').empty();
    $('.tbl-header2 tbody').empty();
    var str = '';
    $.each(Items, function (i, item) {
        str = '<tr>' +
            '<td>' + item.STATION + '</td>' +
            '<td>' + item.ALARM_CODE + '</td>' +
            '<td>' + item.START_TIME + '</td>' +
            '<td>' + item.END_TIME + '</td>' +
            '</tr>'

        $('.tbl-body2 tbody').append(str);
        $('.tbl-header2 tbody').append(str);
    });

    if (Items.length >= 5) {
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

        MyMarhq2 = setInterval(Marqueehq, speedhq);

        // 鼠标移上去取消事件
        $(".tbl-header2 tbody").hover(function () {
            clearInterval(MyMarhq2);
        }, function () {
            clearInterval(MyMarhq2);
            MyMarhq2 = setInterval(Marqueehq, speedhq);
        })

    }
}
