layui.use('laydate', function () {
    var laydate = layui.laydate;

    laydate.render({
        elem: '#datepicker'
        , type: 'date'
        , format: 'yyyy-MM-dd'
        , done: function (value, date, endDate) {
            getData($("#equipment").val(), $("#datepicker").val());
        }
    });
})

function getData(eqid, date) {
    $.ajax({
        url: '/DPPM/GetTrayDetails',
        data: {
            "eqid": eqid,
            "date": date
        },
        type: 'POST',
        contentType: 'application/x-www-form-urlencoded; charset=UTF-8',//数据类型必须有
        async: false,
        success: function (data) {
            var ratedata = data.ratedata;
            var trenddata = data.trenddata;
            var trenddates = data.trenddates;

            getratechart('ratechart', ratedata);
            gettrendchart('trendchart', trenddates, trenddata);

        },
        error: function (message) {
            alert('error!');
        }
    });
}

function getratechart(id, ratedata) {
    var chartDom = document.getElementById(id);
    var myChart = echarts.init(chartDom);

    $(window).on('resize', function () {//
        //屏幕大小自适应，重置容器高宽
        chartDom.style.width = window.innerWidth * 0.8 + 'px';
        chartDom.style.height = window.innerHeight * 0.4 + 'px';
        myChart.resize();
    });
    chartDom.style.width = window.innerWidth * 0.8 + 'px';
    chartDom.style.height = window.innerHeight * 0.4 + 'px';

    var option;

    option = {
        title: {
            text: 'NG Rate by Nozzle',
            left: 'center'
        },
        tooltip: {
            trigger: 'item'
        },
        legend: {
            orient: 'horizontal',
            top: '8%',
            left: 'center'
        },
        series: [
            {
                name: 'NG Rate',
                type: 'pie',
                radius: '80%',
                center: ['50%', '60%'],
                data: ratedata,
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

function gettrendchart(id, trenddates, trenddata) {
    var chartDom = document.getElementById(id);
    var myChart = echarts.init(chartDom);

    $(window).on('resize', function () {//
        //屏幕大小自适应，重置容器高宽
        chartDom.style.width = window.innerWidth * 0.8 + 'px';
        chartDom.style.height = window.innerHeight * 0.4 + 'px';
        myChart.resize();
    });
    chartDom.style.width = window.innerWidth * 0.8 + 'px';
    chartDom.style.height = window.innerHeight * 0.4 + 'px';

    var option;

    option = {
        title: {
            text: 'DPPM Trends'
        },
        tooltip: {
            trigger: 'axis',
            axisPointer: {
                type: 'cross'
            }
        },
        legend: {
            data: ['DPPM']
        },
        grid: {

        },
        xAxis: {
            type: 'category',
            axisTick: {
                alignWithLabel: true
            },
            data: trenddates
        },
        yAxis: {
            type: 'value',
            name: 'DPPM',
            position: 'left',
            splitLine: {     //网格线
                show: false
            }
        },
        series: {
            name: 'DPPM',
            type: 'line',
            data: trenddata,
            label: {
                show: true,
                position: 'top'
            },
        }
    };
    option && myChart.setOption(option);
    myChart.resize();
}