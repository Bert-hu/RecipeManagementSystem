
setInterval("GetEqp()", 2000);

setInterval("GetCondition()", 30000);

$(function () {
    resizeFontSize();
    GetEqp();
    
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

                GetCondition();
            }
        },
        error: function () {
            console.log("error");
        }
    });
}

function GetCondition() {
    var objEquipmentID = document.getElementById("inputEquipment").value;
    var objStationID;
    var objStartTime = document.getElementById("dtQueryDate").value + " 05:00:00";


    // GetTestTimeRatio
    GetAvgTestTime(objEquipmentID, objStartTime);
    var objCapacityTarget = document.getElementById("inputCapacityTarget").value;


    // GetProfile
    GetProfile(objEquipmentID, objStartTime);
    // GetBxoInfo
    GetBxoInfo(objEquipmentID, objStartTime);
    // Capacity
    // GetDailyCapacity(objEquipmentID, objStartTime, objCapacityTarget);
    // GetWeeklyCapacity(objEquipmentID, objStartTime);
    // GetMonthlyCapacity(objEquipmentID, objStartTime);

    SetEquipmentIndicator();
}

function GetBxoInfo(objEquipment, objStartTime) {
    $.ajax({
        type: 'post',
        dataType: 'json',
        url: "/Robot/BoxInfo",
        asyn: true,
        data: {
            "EQ": objEquipment,
            "StartTime": objStartTime,
        },
        success: function (datastr) {
            var data = JSON.parse(datastr);
            SetBoxInfo(data.dtData);
            SetCapacityUtilization(data.dtData, data.capacityTarget);
        },
        error: function () {

        }
    });
}

function SetBoxInfo(data) {
    var objEquipmentCapacity = document.getElementById("h6EquipmentCapacity");
    var iTotalCapacity = 0;
    //var dataBoxInfo = [];
    document.getElementById("divStationCapacity1").innerText = 0;
    document.getElementById("divStationCapacity2").innerText = 0;
    document.getElementById("divStationCapacity3").innerText = 0;
    document.getElementById("divStationCapacity4").innerText = 0;
    document.getElementById("divStationCapacity5").innerText = 0;
    document.getElementById("divStationCapacity6").innerText = 0;
    document.getElementById("divStationCapacity7").innerText = 0;
    document.getElementById("divStationCapacity8").innerText = 0;
    document.getElementById("divStationCapacity9").innerText = 0;
    document.getElementById("divStationCapacity10").innerText = 0;
    for (var i = 0; i < data.length; i++) {
        //dataBoxInfo.push({
        //    name: data[i]["Station"],
        //    value: data[i]["Total"],
        //    itemStyle: {
        //        normal: {
        //            color: data[i]["Color"]
        //        }
        //    },
        //    //itemStyle: {
        //    //    normal: {
        //    //        borderColor: '#A1DFF2',  //地图边界线颜色  
        //    //        borderWidth: 1,  //边界线宽度  
        //    //        areaStyle: {
        //    //            color: 'white'
        //    //        },
        //    //        label: {
        //    //            show: true
        //    //        }
        //    //    }
        //    //},
        //});



        if (data[i]["Station"] == "station_1") {
            var objaStationCapacity1 = document.getElementById("aStationCapacity1");
            var objiStationCapacity1 = document.getElementById("iStationCapacity1");
            var objdivStationCapacity1 = document.getElementById("divStationCapacity1");

            if (data[i]["Status"] == "PROCESS") {
                objaStationCapacity1.className = "btn border-indigo-400 text-indigo-400 btn-flat btn-rounded btn-icon btn-xs valign-text-bottom";
                objiStationCapacity1.className = "icon-play3";
            } else {
                objaStationCapacity1.className = "btn border-warning-400 text-warning-400 btn-flat btn-rounded btn-icon btn-xs valign-text-bottom";
                objiStationCapacity1.className = "icon-warning22";
            }

            objdivStationCapacity1.innerText = data[i]["Total"];
        } else if (data[i]["Station"] == "station_2") {
            var objaStationCapacity2 = document.getElementById("aStationCapacity2");
            var objiStationCapacity2 = document.getElementById("iStationCapacity2");
            var objdivStationCapacity2 = document.getElementById("divStationCapacity2");

            if (data[i]["Status"] == "PROCESS") {
                objaStationCapacity2.attributes.class = "btn border-indigo-400 text-indigo-400 btn-flat btn-rounded btn-icon btn-xs valign-text-bottom";
                objiStationCapacity2.attributes.class = "icon-play3";
            } else {
                objaStationCapacity2.attributes.class = "btn border-warning-400 text-warning-400 btn-flat btn-rounded btn-icon btn-xs valign-text-bottom";
                objiStationCapacity2.attributes.class = "icon-warning22";
            }

            objdivStationCapacity2.innerText = data[i]["Total"];
        } else if (data[i]["Station"] == "station_3") {
            var objaStationCapacity3 = document.getElementById("aStationCapacity3");
            var objiStationCapacity3 = document.getElementById("iStationCapacity3");
            var objdivStationCapacity3 = document.getElementById("divStationCapacity3");

            if (data[i]["Status"] == "PROCESS") {
                objaStationCapacity3.attributes.class = "btn border-indigo-400 text-indigo-400 btn-flat btn-rounded btn-icon btn-xs valign-text-bottom";
                objiStationCapacity3.attributes.class = "icon-play3";
            } else {
                objaStationCapacity3.attributes.class = "btn border-warning-400 text-warning-400 btn-flat btn-rounded btn-icon btn-xs valign-text-bottom";
                objiStationCapacity3.attributes.class = "icon-warning22";
            }

            objdivStationCapacity3.innerText = data[i]["Total"];
        } else if (data[i]["Station"] == "station_4") {
            var objaStationCapacity4 = document.getElementById("aStationCapacity4");
            var objiStationCapacity4 = document.getElementById("iStationCapacity4");
            var objdivStationCapacity4 = document.getElementById("divStationCapacity4");

            if (data[i]["Status"] == "PROCESS") {
                objaStationCapacity4.attributes.class = "btn border-indigo-400 text-indigo-400 btn-flat btn-rounded btn-icon btn-xs valign-text-bottom";
                objiStationCapacity4.attributes.class = "icon-play3";
            } else {
                objaStationCapacity4.attributes.class = "btn border-warning-400 text-warning-400 btn-flat btn-rounded btn-icon btn-xs valign-text-bottom";
                objiStationCapacity4.attributes.class = "icon-warning22";
            }

            objdivStationCapacity4.innerText = data[i]["Total"];
        } else if (data[i]["Station"] == "station_5") {
            var objaStationCapacity5 = document.getElementById("aStationCapacity5");
            var objiStationCapacity5 = document.getElementById("iStationCapacity5");
            var objdivStationCapacity5 = document.getElementById("divStationCapacity5");

            if (data[i]["Status"] == "PROCESS") {
                objaStationCapacity5.attributes.class = "btn border-indigo-400 text-indigo-400 btn-flat btn-rounded btn-icon btn-xs valign-text-bottom";
                objiStationCapacity5.attributes.class = "icon-play3";
            } else {
                objaStationCapacity5.attributes.class = "btn border-warning-400 text-warning-400 btn-flat btn-rounded btn-icon btn-xs valign-text-bottom";
                objiStationCapacity5.attributes.class = "icon-warning22";
            }

            objdivStationCapacity5.innerText = data[i]["Total"];
        } else if (data[i]["Station"] == "station_6") {
            var objaStationCapacity6 = document.getElementById("aStationCapacity6");
            var objiStationCapacity6 = document.getElementById("iStationCapacity6");
            var objdivStationCapacity6 = document.getElementById("divStationCapacity6");

            if (data[i]["Status"] == "PROCESS") {
                objaStationCapacity6.attributes.class = "btn border-indigo-400 text-indigo-400 btn-flat btn-rounded btn-icon btn-xs valign-text-bottom";
                objiStationCapacity6.attributes.class = "icon-play3";
            } else {
                objaStationCapacity6.attributes.class = "btn border-warning-400 text-warning-400 btn-flat btn-rounded btn-icon btn-xs valign-text-bottom";
                objiStationCapacity6.attributes.class = "icon-warning22";
            }

            objdivStationCapacity6.innerText = data[i]["Total"];
        } else if (data[i]["Station"] == "station_7") {
            var objaStationCapacity7 = document.getElementById("aStationCapacity7");
            var objiStationCapacity7 = document.getElementById("iStationCapacity7");
            var objdivStationCapacity7 = document.getElementById("divStationCapacity7");

            if (data[i]["Status"] == "PROCESS") {
                objaStationCapacity7.attributes.class = "btn border-indigo-400 text-indigo-400 btn-flat btn-rounded btn-icon btn-xs valign-text-bottom";
                objiStationCapacity7.attributes.class = "icon-play3";
            } else {
                objaStationCapacity7.attributes.class = "btn border-warning-400 text-warning-400 btn-flat btn-rounded btn-icon btn-xs valign-text-bottom";
                objiStationCapacity7.attributes.class = "icon-warning22";
            }

            objdivStationCapacity7.innerText = data[i]["Total"];
        } else if (data[i]["Station"] == "station_8") {
            var objaStationCapacity8 = document.getElementById("aStationCapacity8");
            var objiStationCapacity8 = document.getElementById("iStationCapacity8");
            var objdivStationCapacity8 = document.getElementById("divStationCapacity8");

            if (data[i]["Status"] == "PROCESS") {
                objaStationCapacity8.attributes.class = "btn border-indigo-400 text-indigo-400 btn-flat btn-rounded btn-icon btn-xs valign-text-bottom";
                objiStationCapacity8.attributes.class = "icon-play3";
            } else {
                objaStationCapacity8.attributes.class = "btn border-warning-400 text-warning-400 btn-flat btn-rounded btn-icon btn-xs valign-text-bottom";
                objiStationCapacity8.attributes.class = "icon-warning22";
            }

            objdivStationCapacity8.innerText = data[i]["Total"];
        } else if (data[i]["Station"] == "station_9") {
            var objaStationCapacity9 = document.getElementById("aStationCapacity9");
            var objiStationCapacity9 = document.getElementById("iStationCapacity9");
            var objdivStationCapacity9 = document.getElementById("divStationCapacity9");

            if (data[i]["Status"] == "PROCESS") {
                objaStationCapacity9.attributes.class = "btn border-indigo-400 text-indigo-400 btn-flat btn-rounded btn-icon btn-xs valign-text-bottom";
                objiStationCapacity9.attributes.class = "icon-play3";
            } else {
                objaStationCapacity9.attributes.class = "btn border-warning-400 text-warning-400 btn-flat btn-rounded btn-icon btn-xs valign-text-bottom";
                objiStationCapacity9.attributes.class = "icon-warning22";
            }

            objdivStationCapacity9.innerText = data[i]["Total"];
        } else if (data[i]["Station"] == "station_10") {
            var objaStationCapacity10 = document.getElementById("aStationCapacity10");
            var objiStationCapacity10 = document.getElementById("iStationCapacity10");
            var objdivStationCapacity10 = document.getElementById("divStationCapacity10");

            if (data[i]["Status"] == "PROCESS") {
                objaStationCapacity10.attributes.class = "btn border-indigo-400 text-indigo-400 btn-flat btn-rounded btn-icon btn-xs valign-text-bottom";
                objiStationCapacity10.attributes.class = "icon-play3";
            } else {
                objaStationCapacity10.attributes.class = "btn border-warning-400 text-warning-400 btn-flat btn-rounded btn-icon btn-xs valign-text-bottom";
                objiStationCapacity10.attributes.class = "icon-warning22";
            }

            objdivStationCapacity10.innerText = data[i]["Total"];
        }

        iTotalCapacity += data[i]["Total"];
    }



    objEquipmentCapacity.innerText = "Equipment Capacity (" + iTotalCapacity + " pcs)";


    //var maxData = 0;
    //var minData = 0;
    //for (var i = 0; i < dataBoxInfo.length; i++) {
    //    var value = parseInt(dataBoxInfo[i].value);
    //    if (value > maxData) {
    //        maxData = value;
    //    }
    //    if (value < minData) {
    //        minData = value;
    //    }
    //}

    //var option = {
    //    tooltip: {
    //        trigger: 'item',
    //        formatter: '{b}<br/>{c} (次)'
    //    },
    //    toolbox: {
    //        show: true,
    //        orient: 'vertical',
    //        x: 'right',
    //        y: 'center',
    //        feature: {
    //            mark: {
    //                show: true
    //            },
    //            saveAsImage: {
    //                show: false
    //            }
    //        }
    //    },
    //    //dataRange: {
    //    //    min: minData * 0.8,
    //    //    max: maxData * 1.2,
    //    //    text: ['Low', 'High'],
    //    //    realtime: false,
    //    //    calculable: true,
    //    //    x: 'left',
    //    //    y: 'center',
    //    //    orient: 'vertical'
    //    //},

    //    //visualMap: {
    //    //    left: 'left',
    //    //    min: minData * 0.8,
    //    //    max: maxData * 1.2,
    //    //    inRange: {
    //    //        color: ['#313695', '#4575b4', '#74add1', '#abd9e9', '#e0f3f8', '#ffffbf', '#fee090', '#fdae61', '#f46d43', '#d73027']
    //    //    },
    //    //    text: ['High', 'Low'],           // 文本，默认为数值文本
    //    //    calculable: true
    //    //},

    //    series: [{
    //        name: '测试次数',
    //        type: 'map',
    //        mapType: 'mymap', // 自定义扩展图表类型
    //        itemStyle: {
    //            normal: {
    //                label: {
    //                    show: true
    //                }
    //            },
    //            emphasis: {
    //                label: {
    //                    show: true
    //                }
    //            }
    //        },
    //        data: dataBoxInfo
    //    }]
    //};

    //echarts.registerMap('mymap', boxJson);
    //var chart = echarts.init(document.getElementById('divBox'));
    //chart.setOption(option);
}

function GetAvgTestTime(objEquipment, objStartTime) {
    $.ajax({
        type: 'post',
        dataType: 'json',
        url: "/Robot/TestTimeRatio",
        asyn: false,
        data: {
            "EQ": objEquipment,
            "StartTime": objStartTime,
        },
        success: function (datastr) {
            var data = JSON.parse(datastr);
            var _data = data.dt;
            var count = data.stationcount;
            SetAvgTestTime(_data, count);

            document.getElementById("inputCapacityTarget").value = data.capacityTarget;
            var iTarget = document.getElementById("inputCapacityTarget").value;
            document.getElementById("dayOffTime").value = data.dayofftime;
            document.getElementById("nightOffTime").value = data.nightofftime;
            SetQualityRatio(_data);
        },
        error: function () {
            console.log("error");
        }
    });
}


function SetAvgTestTime(data, count) {
    var objAvgTestTime = document.getElementById("h6AvgTestTime");
    //var objAvgTestTimeExport = document.getElementById("btnAvgTestTimeExport");
    var idx = objAvgTestTime.innerHTML.indexOf('Avg TestTime');

    var iTarget = document.getElementById("inputCapacityTarget").value;
    var objTestTimeTarget = (86400 * count / iTarget).toFixed(1);
    document.getElementById("inputTestTimeTarget").value = objTestTimeTarget + " sec";

    //objAvgTestTime.innerHTML = "Avg TestTime (" + data[0]["AvgTestTime"] + " sec)";
    objAvgTestTime.innerHTML = objAvgTestTime.innerHTML.substr(0, idx) + "Avg TestTime (" + data[0]["AvgTestTime"] + " sec)";
    iIndicatorTestTimeRatio = ((objTestTimeTarget / data[0]["AvgTestTime"]) * 100).toFixed(1);
    //console.log(data);
    if (iIndicatorTestTimeRatio > 100)
        iIndicatorTestTimeRatio = 100;


    var dataCapacityUtilization = [];

    for (var i = 0; i < data.length; i++) {
        dataCapacityUtilization.push({
            value: [data[i]["EventTime"], data[i]["TestTime"], data[i]["Station"], data[i]["Result"], data[i]["ErrorCode"], data[i]["Barcode"]],
            itemStyle: {
                normal: {
                    color: data[i]["Color"]
                }
            }
        });
    }

    var option = {
        tooltip: {
            formatter: function (params) {
                if (params.value[4] == "NA") {
                    return params.marker + params.value[3] + "<br />" + "Station: " + params.value[2] + "<br />" + "EventTime: " + params.value[0] + "<br />" + "Barcode: " + params.value[5];
                } else {
                    return params.marker + params.value[3] + "<br />" + "Station: " + params.value[2] + "<br />" + "EventTime: " + params.value[0] + "<br />" + "ErrorCode: " + params.value[4] + "<br />" + "Barcode: " + params.value[5];
                }
            }
        },
        dataZoom: [
            {
                type: 'inside'
            }
            , {
                type: 'slider',
                showDataShadow: false,
                top: '93%',
                height: 10,
                handleIcon: 'M10.7,11.9v-1.3H9.3v1.3c-4.9,0.3-8.8,4.4-8.8,9.4c0,5,3.9,9.1,8.8,9.4v1.3h1.3v-1.3c4.9-0.3,8.8-4.4,8.8-9.4C19.5,16.3,15.6,12.2,10.7,11.9z M13.3,24.4H6.7V23h6.6V24.4z M13.3,19.6H6.7v-1.4h6.6V19.6z',
                handleSize: 10,
                handleStyle: {
                    color: 'white',
                    shadowBlur: 3,
                    shadowColor: 'rgba(0, 0, 0, 0.6)',
                    shadowOffsetX: 2,
                    shadowOffsetY: 2
                },
            }
        ],
        grid: [
            {
                x: '5%',
                y: '3%',
                width: '90%',
                height: '88%'
            },
        ],
        xAxis: {
            type: 'time',
            splitNumber: 1,
            axisLabel: {
                show: false,
                textStyle: {
                    color: '#fff'
                }
            },
            axisTick: {
                show: false
            },
        },
        yAxis: {
            //splitNumber: 10,
            axisLabel: {
                show: true,
                textStyle: {
                    color: '#fff'
                }
            },
            axisTick: {
                show: false
            },
        },
        series: [{
            type: 'scatter',
            symbolSize: 6,
            data: dataCapacityUtilization,
            legendHoverLink: true,
            hoverAnimation: true,
            emphasis: {
                label: {
                    show: true,
                    formatter: function (param) {
                        return param.data[0];
                    },
                    position: 'top'
                }
            },
        }]
    };
    var eChart = echarts.init(document.getElementById('divAvgTestTime'));
    eChart.setOption(option);
    window.addEventListener("resize", function () {
        eChart.resize();
    });
}

function SetOEE(iFailCount, iPassCount, iActual) {
    var dateNow = new Date();
    var dateNow2 = dateNow.getFullYear() + "-" + AddZero(dateNow.getMonth() + 1) + "-" + AddZero(dateNow.getDate());
    var minutesNow = (dateNow.getHours() * 60) + dateNow.getMinutes();
    var iOEE = 0;
    var dataOEE = [];
    var iTarget = document.getElementById("inputCapacityTarget").value;

    if (iTarget < 0)
        itarget = 0;

    if (dateNow.getHours() >= 5) {
        minutesNow = ((dateNow.getHours() - 5) * 60) + dateNow.getMinutes();
    } else {
        minutesNow = (19 * 60) + (dateNow.getHours() * 60) + dateNow.getMinutes();
    }
    iOEE = 1 * (iActual / (minutesNow * (iTarget / 1440))) * (iPassCount / iActual);

    //if (document.getElementById("dtQueryDate").value != dateNow2) {
    //    iOEE = (1440 / 1440) * (iActual / iTarget) * (iPassCount / iActual);
    //} else {
    //    //需要有 PM System 提供时间
    //    //iOEE = (PM time / minutesNow) * (iActual / iTarget) * (iPassCount / iActual);

    //    // target = 9600 (86400 / 90 sec)
    //    // 9600 / 24 / 60 = 6.7
    //    // 1 hr = 400 pcs
    //    // 1 min = 6.67 pcs
    //    iOEE = 1 * (iActual / (minutesNow * (iTarget / 1440))) * (iPassCount / iActual);
    //}
    iIndicatorOEE = iOEE * 100;

    dataOEE = [iOEE, iOEE, iOEE, iOEE];

    var option = {
        series: [{
            type: 'liquidFill',
            center: ["50%", "50%"],
            radius: "95%",
            color: ['#45BDFF', '#45BDFF', '#45BDFF', '#45BDFF'],
            waveLength: '100%',
            data: dataOEE,
            //backgroundStyle: {
            //    borderWidth: 5,
            //    borderColor: 'red',
            //    //color: 'yellow'
            //},
            itemStyle: {
                shadowBlur: 0
            },
            outline: {
                borderDistance: 0,
                itemStyle: {
                    borderWidth: 3,
                    borderColor: '#156ACF',
                    shadowBlur: 20,
                }
            },
            label: {
                normal: {
                    textStyle: {
                        color: 'black',
                        insideColor: 'yellow',
                        fontSize: 20
                    }
                }
            },
        }]
    };

    var eChart = echarts.init(document.getElementById('divOEE'));
    eChart.setOption(option);
    window.addEventListener("resize", function () {
        eChart.resize();
    });
}

function GetDowntimeRatio(objEquipment) {
    $.ajax({
        type: 'post',
        dataType: 'json',
        url: "/ashx/DowntimeRatio.ashx?",
        asyn: true,
        data: {
            "EQ": objEquipment,
            "StartTime": objStartTime,
            "EndTime": objEndTime,
        },
        success: function (data) {
            SetDowntimeRatio(data);
        },
        error: function () {

        }
    });
}


function SetDowntimeRatio(data) {
    var dataDowntimeRatio = [];
    dataDowntimeRatio = [data[0]["DowntimeRatio1"], data[0]["DowntimeRatio2"], data[0]["DowntimeRatio3"], data[0]["DowntimeRatio4"]];


    var option = {
        series: [{
            type: 'liquidFill',
            data: dataDowntimeRatio,
            itemStyle: {
                shadowBlur: 0
            },
            outline: {
                borderDistance: 0,
                itemStyle: {
                    borderWidth: 3,
                    borderColor: '#156ACF',
                    shadowBlur: 20,
                }
            },
            label: {
                normal: {
                    textStyle: {
                        color: 'black',
                        insideColor: 'yellow',
                        fontSize: 20
                    }
                }
            },
        }]
    };

    var eChart = echarts.init(document.getElementById('divDowntimeRatio'));
    eChart.setOption(option);
    window.addEventListener("resize", function () {
        eChart.resize();
    });
}

function SetQualityRatio(data) {
    var iFailCount = 0;
    var iPassCount = 0;
    var iQualityRatio = 0;

    for (var i = 0; i < data.length; i++) {
        if (data[i]["Result"] == "FAIL")
            iFailCount++;
    }

    if (data.length > 0) {
        iPassCount = data.length - iFailCount;
        iQualityRatio = ((iPassCount / data.length) * 100).toFixed(1);
    } else {
        iQualityRatio = 100;
    }
    iIndicatorQualityRatio = iQualityRatio;

    var option = {
        series: [
            {
                type: "gauge",
                center: ["50%", "65%"], // 默认全局居中
                radius: "85%",
                startAngle: 200,
                endAngle: -20,
                axisLine: {
                    show: true,
                    lineStyle: { // 属性lineStyle控制线条样式
                        color: [ //表盘颜色
                            [0.9, "red"],//0-80%处的颜色
                            //[0.9, "orange"],//81%-90%处的颜色
                            [1, "lightgreen"]//80%-100%处的颜色
                        ],
                        width: 15//表盘宽度
                    }
                },
                splitLine: { //分割线样式（及10、20等长线样式）
                    length: 15,
                    lineStyle: { // 属性lineStyle控制线条样式
                        width: 2
                    }
                },
                axisTick: { //刻度线样式（及短线样式）
                    length: 20,
                    show: false
                },
                axisLabel: { //文字样式（及“10”、“20”等文字样式）
                    color: "white",
                    distance: -23, //文字离表盘的距离
                    textStyle: {
                        fontSize: 6
                    }
                },
                pointer: { //指针样式
                    length: '70%',
                },
                detail: {
                    formatter: "{score|{value}%}",
                    offsetCenter: [0, "40%"],
                    height: 10,
                    rich: {
                        score: {
                            //color: "white",
                            //fontFamily: "Calibri",
                            fontSize: 18
                        }
                    }
                },
                data: [{
                    value: iQualityRatio,
                    label: {
                        textStyle: {
                            fontSize: 12
                        }
                    }
                }]
            }
        ]
    };

    var eChart = echarts.init(document.getElementById('divQualityRatio'));
    eChart.setOption(option);
    window.addEventListener("resize", function () {
        eChart.resize();
    });

    SetOEE(iFailCount, iPassCount, data.length);
}

function SetCapacityUtilization(data, target) {
    var dateNow = new Date();
    var dateNow2 = dateNow.getFullYear() + "-" + AddZero(dateNow.getMonth() + 1) + "-" + AddZero(dateNow.getDate());
    var minutesNow = (dateNow.getHours() * 60) + dateNow.getMinutes();
    var iActual = 0;
    //var iTarget = document.getElementById("inputCapacityTarget").value;
    var iTarget = target;
    var iExpected = 0;
    var iCapUtilization = 0;


    if (iTarget < 0)
        itarget = 0;

    if (dateNow.getHours() >= 5) {
        minutesNow = ((dateNow.getHours() - 5) * 60) + dateNow.getMinutes();
    } else {
        minutesNow = (19 * 60) + (dateNow.getHours() * 60) + dateNow.getMinutes();
    }

    for (var i = 0; i < data.length; i++) {
        iActual += data[i]["Total"];
    }

    // 产能利用率的公式：实际产能/设计产能*100%
    //if (document.getElementById("dtQueryDate").value != dateNow2) {
    //    iExpected = iTarget;
    //    iCapUtilization = ((iActual / iTarget) * 100).toFixed(1);
    //} else {
    //    // target = 9600 (86400 / 90 sec)
    //    // 9600 / 24 / 60 = 6.7
    //    // 1 hr = 400 pcs
    //    // 1 min = 6.67 pcs
    //    iExpected = (minutesNow * (iTarget / 1440)).toFixed(0);
    //    iCapUtilization = ((iActual / (minutesNow * (iTarget / 1440))) * 100).toFixed(1);
    //}

    iExpected = (minutesNow * (iTarget / 1440)).toFixed(0);
    iCapUtilization = ((iActual / (minutesNow * (iTarget / 1440))) * 100).toFixed(1);

    iIndicatorCapacityUtilization = iCapUtilization;

    var option = {
        series: [
            //{
            //    type: "gauge",
            //    center: ["50%", "50%"], // 仪表位置
            //    radius: "100%", //仪表大小
            //    startAngle: 200, //开始角度
            //    endAngle: -20, //结束角度
            //    axisLine: {
            //        show: false,
            //        lineStyle: { // 属性lineStyle控制线条样式
            //            color: [
            //                [0.5, new echarts.graphic.LinearGradient(0, 0, 1, 0, [{
            //                    offset: 1,
            //                    color: "#E75F25" // 50% 处的颜色
            //                }, {
            //                    offset: 0.8,
            //                    color: "#D9452C" // 40% 处的颜色
            //                }], false)], // 100% 处的颜色
            //                [0.7, new echarts.graphic.LinearGradient(0, 0, 1, 0, [{
            //                    offset: 1,
            //                    color: "#FFC539" // 70% 处的颜色
            //                }, {
            //                    offset: 0.8,
            //                    color: "#FE951E" // 66% 处的颜色
            //                }, {
            //                    offset: 0,
            //                    color: "#E75F25" // 50% 处的颜色
            //                }], false)],
            //                [0.9, new echarts.graphic.LinearGradient(0, 0, 0, 1, [{
            //                    offset: 1,
            //                    color: "#C7DD6B" // 90% 处的颜色
            //                }, {
            //                    offset: 0.8,
            //                    color: "#FEEC49" // 86% 处的颜色
            //                }, {
            //                    offset: 0,
            //                    color: "#FFC539" // 70% 处的颜色
            //                }], false)],
            //                [1, new echarts.graphic.LinearGradient(0, 0, 0, 1, [{
            //                    offset: 0.2,
            //                    color: "#1CAD52" // 92% 处的颜色
            //                }, {
            //                    offset: 0,
            //                    color: "#C7DD6B" // 90% 处的颜色
            //                }], false)]
            //            ],
            //            width: 10
            //        }
            //    },
            //    splitLine: {
            //        show: false
            //    },
            //    axisTick: {
            //        show: false
            //    },
            //    axisLabel: {
            //        show: false
            //    },
            //    pointer: { //指针样式
            //        length: '20%'
            //    },
            //    detail: {
            //        show: false
            //    }
            //},
            {
                type: "gauge",
                center: ["50%", "65%"], // 默认全局居中
                radius: "85%",
                startAngle: 200,
                endAngle: -20,
                axisLine: {
                    show: true,
                    lineStyle: { // 属性lineStyle控制线条样式
                        color: [ //表盘颜色
                            [0.93, "red"],//0-60%处的颜色
                            //[0.8, "orange"],//61%-80%处的颜色
                            [1, "lightgreen"]//80%-100%处的颜色
                        ],
                        width: 15//表盘宽度
                    }
                },
                splitLine: { //分割线样式（及10、20等长线样式）
                    length: 15,
                    lineStyle: { // 属性lineStyle控制线条样式
                        width: 2
                    }
                },
                axisTick: { //刻度线样式（及短线样式）
                    length: 20,
                    show: false
                },
                axisLabel: { //文字样式（及“10”、“20”等文字样式）
                    color: "white",
                    distance: -23, //文字离表盘的距离
                    textStyle: {
                        fontSize: 6
                    }
                },
                pointer: { //指针样式
                    length: '70%',
                },
                detail: {
                    formatter: "{score|{value}%}",
                    offsetCenter: [0, "40%"],
                    //backgroundColor: 'yellow',
                    height: 10,
                    rich: {
                        score: {
                            //color: "white",
                            //fontFamily: "Calibri",
                            fontSize: 18
                        }
                    }
                },
                data: [{
                    value: iCapUtilization,
                    label: {
                        textStyle: {
                            fontSize: 12
                        }
                    }
                }]
            }
        ]
    };

    var eChart = echarts.init(document.getElementById('divCapacityUtilization'));
    eChart.setOption(option);
    window.addEventListener("resize", function () {
        eChart.resize();
    });



    // Capacity Status
    var option2 = {
        tooltip: {
            trigger: 'axis',
            axisPointer: {
                type: 'shadow'
            }
        },
        grid: {
            x: '2%',
            y: '1%',
            width: '95%',
            height: '99%',
            containLabel: true
        },
        xAxis: {
            type: 'value',
            splitNumber: 1,
            boundaryGap: [0, 0.01],
            axisLabel: {
                show: true,
                textStyle: {
                    color: '#fff'
                },
                fontSize: 7,
                rotate: 10
            },
            axisTick: {
                show: false
            },
            axisLine: {
                show: true,
                lineStyle: {
                    color: 'white'
                }
            },
        },
        yAxis: {
            type: 'category',
            data: ['Daily', 'Expected', 'Now'],
            axisLabel: {
                show: true,
                textStyle: {
                    color: '#fff'
                },
                fontSize: 7
            },
            axisTick: {
                show: false
            },
            axisLine: {
                show: false,
                lineStyle: {
                    color: 'white'
                }
            },
        },
        series: [
            {
                type: 'bar',
                data: [iTarget, iExpected, iActual],
                itemStyle: {
                    normal: {
                        color: '#45BDFF',
                    }
                },
                label: {
                    show: true,
                    position: 'inside'
                },
            },
        ]
    };

    var eChart2 = echarts.init(document.getElementById('divCapacityStatus'));
    eChart2.setOption(option2);
    window.addEventListener("resize", function () {
        eChart2.resize();
    });
}

function GetProfile(objEquipment, objStartTime) {
    $.ajax({
        type: 'post',
        dataType: 'json',
        url: "/Robot/RobotProfile",
        asyn: true,
        data: {
            "EQ": objEquipment,
            "StartTime": objStartTime,
        },
        success: function (datastr) {
            var data = JSON.parse(datastr);
            SetProfile(data);
            SetShutdownRatio(data);
        },
        error: function () {

        }
    });
}

function SetProfile(data) {
    var profileChart;
    var categories = ['Station_1', 'Station_2', 'Station_3', 'Station_4', 'Station_5', 'Station_6', 'Station_7', 'Station_8', 'Station_9', 'Station_10', 'Robot'];
    var dataProfile = [];
    for (var i = 0; i < data.length; i++) {
        dataProfile.push({
            name: [data[i]["Station"]],
            value: [data[i]["Station"], data[i]["LongStartTime"], data[i]["LongEndTime"], data[i]["TestTime"], data[i]["StrStartTime"], data[i]["StrEndTime"], data[i]["Content"]],
            itemStyle: {
                normal: {
                    color: data[i]["Color"]
                }
            }
        });
    }

    var option = {
        tooltip: {
            formatter: function (params) {
                return params.marker + params.value[3] + "<br />" + params.value[4] + "~" + params.value[5] + "<br />" + params.value[6]
            }
        },
        title: {
            show: false
        },
        //dataZoom: [{
        //    type: 'slider',
        //    filterMode: 'weakFilter',
        //    showDataShadow: false,
        //    top: '95%',
        //    height: 5,
        //    borderColor: 'transparent',
        //    backgroundColor: '#e2e2e2',
        //    handleIcon: 'M10.7,11.9H9.3c-4.9,0.3-8.8,4.4-8.8,9.4c0,5,3.9,9.1,8.8,9.4h1.3c4.9-0.3,8.8-4.4,8.8-9.4C19.5,16.3,15.6,12.2,10.7,11.9z M13.3,24.4H6.7v-1.2h6.6z M13.3,22H6.7v-1.2h6.6z M13.3,19.6H6.7v-1.2h6.6z', // jshint ignore:line
        //    handleSize: 10,
        //    handleStyle: {
        //        shadowBlur: 6,
        //        shadowOffsetX: 1,
        //        shadowOffsetY: 2,
        //        shadowColor: '#aaa'
        //    },
        //    labelFormatter: ''
        //}, {
        //    type: 'inside',
        //    filterMode: 'weakFilter'
        //}],
        dataZoom: [{
            type: 'inside'
        }, {
            type: 'slider',
            showDataShadow: false,
            top: '93%',
            height: 10,
            handleIcon: 'M10.7,11.9v-1.3H9.3v1.3c-4.9,0.3-8.8,4.4-8.8,9.4c0,5,3.9,9.1,8.8,9.4v1.3h1.3v-1.3c4.9-0.3,8.8-4.4,8.8-9.4C19.5,16.3,15.6,12.2,10.7,11.9z M13.3,24.4H6.7V23h6.6V24.4z M13.3,19.6H6.7v-1.4h6.6V19.6z',
            handleSize: 10,
            handleStyle: {
                color: 'white',
                shadowBlur: 3,
                shadowColor: 'rgba(0, 0, 0, 0.6)',
                shadowOffsetX: 2,
                shadowOffsetY: 2
            }
        }],
        grid: {
            borderWidth: 0,
            x: '2%',
            y: '1%',
            width: '95%',
            height: '88%',
            containLabel: true,
        },
        xAxis: {
            type: 'time',
            scale: true,
            splitNumber: 1,
            axisTick: {
                show: false
            },
            axisLabel: {
                show: false,
                textStyle: {
                    color: '#fff'
                }
            }
        },
        yAxis: {
            data: categories,
            nameLocation: 'left',
            axisTick: {
                show: false
            },
            axisLine: {
                show: false,
                textStyle: {
                    color: '#fff'
                }
            },
            axisLabel: {
                show: true,
                textStyle: {
                    color: '#fff',
                },
            }
        },
        series: [{
            type: 'custom',
            renderItem: renderItem,
            itemStyle: {
                normal: {
                    opacity: 0.8
                }
            },
            encode: {
                x: [1, 2],
                y: 0
            },
            data: dataProfile
        }]
    };

    profileChart = echarts.init(document.getElementById("divProfile"));
    profileChart.setOption(option);
    window.addEventListener("resize", function () {
        profileChart.resize();
    });

    //profileChart.showLoading({
    //    text: "图表数据正在努力加载..."
    //});
}

function renderItem(params, api) {
    var categoryIndex = api.value(0);
    var start = api.coord([api.value(1), categoryIndex]);
    var end = api.coord([api.value(2), categoryIndex]);
    var height = api.size([0, 1])[1] * 0.6;

    var rectShape = echarts.graphic.clipRectByRect({
        x: start[0],
        y: start[1] - height / 2,
        width: end[0] - start[0],
        height: height
    }, {
        x: params.coordSys.x,
        y: params.coordSys.y,
        width: params.coordSys.width,
        height: params.coordSys.height
    });

    return rectShape && {
        type: 'rect',
        shape: rectShape,
        style: api.style()
    };
}

function SetShutdownRatio(data) {
    var iShutdownRatio = 0;
    var iShutdownTime = 0;
    var dateNow = new Date();
    var dateNow2 = dateNow.getFullYear() + "-" + AddZero(dateNow.getMonth() + 1) + "-" + AddZero(dateNow.getDate());
    var minutesNow = (dateNow.getHours() * 60) + dateNow.getMinutes();

    //if (document.getElementById("dtQueryDate").value != dateNow2) {
    //    minutesNow = 1440;
    //}
    //else {
    //    if (dateNow.getHours() >= 5) {
    //        minutesNow = ((dateNow.getHours() - 5) * 60) + dateNow.getMinutes();
    //    } else {
    //        minutesNow = (19 * 60) + (dateNow.getHours() * 60) + dateNow.getMinutes();
    //    }
    //}
    if (dateNow.getHours() >= 5) {
        minutesNow = ((dateNow.getHours() - 5) * 60) + dateNow.getMinutes();
    } else {
        minutesNow = (19 * 60) + (dateNow.getHours() * 60) + dateNow.getMinutes();
    }

    for (var i = 0; i < data.length; i++) {
        if (data[i]["Color"] == "red") {
            iShutdownTime += parseInt(data[i]["TestTime"]);
        }
    }

    iShutdownRatio = ((iShutdownTime / (minutesNow * 10)) * 100).toFixed(1);
    iIndicatorShutdownRatio = iShutdownRatio;

    var option = {
        series: [
            {
                type: "gauge",
                center: ["50%", "65%"], // 默认全局居中
                radius: "85%",
                startAngle: 200,
                endAngle: -20,
                axisLine: {
                    show: true,
                    lineStyle: { // 属性lineStyle控制线条样式
                        color: [ //表盘颜色
                            [0.1, "lightgreen"],
                            [1, "red"]

                            //[(iShutdownRatio / 100).toFixed(2), "red"],
                            //[1, "gray"]
                        ],
                        width: 15//表盘宽度
                    }
                },
                splitLine: { //分割线样式（及10、20等长线样式）
                    length: 15,
                    lineStyle: { // 属性lineStyle控制线条样式
                        width: 2
                    }
                },
                axisTick: { //刻度线样式（及短线样式）
                    length: 20,
                    show: false
                },
                axisLabel: { //文字样式（及“10”、“20”等文字样式）
                    color: "white",
                    distance: -23, //文字离表盘的距离
                    textStyle: {
                        fontSize: 6
                    }
                },
                pointer: { //指针样式
                    length: '70%',
                },
                detail: {
                    formatter: "{score|{value}%}",
                    offsetCenter: [0, "40%"],
                    height: 10,
                    rich: {
                        score: {
                            //color: "white",
                            //fontFamily: "Calibri",
                            fontSize: 18
                        }
                    }
                },
                data: [{
                    value: iShutdownRatio,
                    label: {
                        textStyle: {
                            fontSize: 12
                        }
                    }
                }]
            }
        ]
    };

    var eChart = echarts.init(document.getElementById('divShutdownRatio'));
    eChart.setOption(option);
    window.addEventListener("resize", function () {
        eChart.resize();
    });
}

var iIndicatorCapacityUtilization;
var iIndicatorQualityRatio;
var iIndicatorShutdownRatio;
var iIndicatorTestTimeRatio;
var iIndicatorOEE;


function SetEquipmentIndicator() {
    var iIndicatorCapacityUtilization2 = iIndicatorCapacityUtilization;
    var iIndicatorQualityRatio2 = iIndicatorQualityRatio;
    var iIndicatorRunRatio = 100 - iIndicatorShutdownRatio;
    var iIndicatorTestTimeRatio2 = iIndicatorTestTimeRatio;
    var iIndicatorOEE2 = iIndicatorOEE;


    if (iIndicatorCapacityUtilization2 > 100)
        iIndicatorCapacityUtilization2 = 100;
    if (iIndicatorQualityRatio2 > 100)
        iIndicatorQualityRatio2 = 100;
    if (iIndicatorRunRatio > 100)
        iIndicatorRunRatio = 100;
    if (iIndicatorTestTimeRatio2 > 100)
        iIndicatorTestTimeRatio2 = 100;
    if (iIndicatorOEE2 > 100)
        iIndicatorOEE2 = 100;


    var option = {
        tooltip: {
            formatter: function (params) {
                return "Cap. Utilization: " + params.value[0] + "%" + "<br/>" +
                    "Quality Ratio: " + params.value[1] + "%" + "<br/>" +
                    "Run Ratio: " + params.value[2] + "%" + "<br/>" +
                    "TestTime Utilization: " + params.value[3] + "%" + "<br/>" +
                    "OEE: " + params.value[4] + "%" + "<br/>";
            }
        },
        radar: {
            //shape: 'circle',
            center: ["50%", "58%"],
            radius: "88%",
            nameGap: 2,
            name: {
                textStyle: {
                    color: '#fff',
                    fontSize: 10,
                    //backgroundColor: '#999',
                    borderRadius: 3,
                    //padding: [-15, 0],
                },

            },
            indicator: [
                { name: 'Cap. Utilization', max: 100 },
                { name: 'Quality Ratio', max: 100 },
                { name: 'Run Ratio', max: 100 },
                { name: 'TestTime Ratio', max: 100 },
                { name: 'OEE', max: 100 }
            ],
            splitArea: {
                show: false,
                areaStyle: {
                    color: 'rgba(255,0,0,0)', // 图表背景的颜色
                },
            },
            //splitLine: {
            //    show: true,
            //    lineStyle: {
            //        width: 1,
            //        color: 'rgba(131,141,158,.1)', // 设置网格的颜色
            //    },
            //},
            axisLine: {
                show: true,
                lineStyle: {
                    color: 'gray',
                },
            },
        },
        series: [{
            type: 'radar',
            areaStyle: {
                opacity: 0.9,
                color: new echarts.graphic.RadialGradient(0.5, 0.5, 1, [
                    {
                        color: 'skyblue',
                        offset: 0
                    },
                    {
                        color: 'yellow',
                        offset: 1
                    }
                ])
            },
            data: [
                {
                    value: [iIndicatorCapacityUtilization2, iIndicatorQualityRatio2, iIndicatorRunRatio, iIndicatorTestTimeRatio2, iIndicatorOEE2],
                }
            ],
        }]
    };

    var eChart = echarts.init(document.getElementById('divEquipmentIndicator'));
    eChart.setOption(option);
    window.addEventListener("resize", function () {
        eChart.resize();
    });
}


function AddZero(num) {
    if (num < 10)
        return "0" + num;
    else
        return num;
}