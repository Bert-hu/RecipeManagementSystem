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

layui.use(['layer', 'table', 'form', 'upload'], function () {
    var layer = layui.layer
        , table = layui.table
        , form = layui.form;

    var alarmtable = table.render({
        elem: '#alarmconfig'
        , url: '../Alarm/GetAlarmConfig'
        , height: 'full'
        , toolbar: '#toolbar'
        , defaultToolbar: []
        , editTrigger: 'dblclick'
        , cols: [[
            { type: 'checkbox', fixed: 'left'}
            , { field: 'ALARM_CODE', title: 'ALARM_CODE',width:'10%'}
            , { field: 'LEVELS', title: 'LEVELS', width: '8%', edit:'text' }
            , { field: 'TYPE', title: 'TYPE', width: '8%', edit: 'text' }
            , { field: 'MESSAGE', title: 'MESSAGE', edit: 'text' }
            , { field: 'TROUBLESHOOTING', title: 'TROUBLESHOOTING', edit: 'text' }
            , {field: 'CLASS', title: 'CLASS', width: '10%'}
        ]]
    });

    table.on('toolbar(alarmconfig)', function (obj) {
        var id = obj.config.id;
        var checkStatus = table.checkStatus(id);
        var othis = lay(this);

        switch (obj.event) {
            case 'add':
                layer.open({
                    type: 1,
                    title: '添加',
                    closeBtn: false,
                    area: ['50%', '50%'],
                    shadeClose: true,
                    content: $("#add-main"),
                    success: function (layero, index) {
                        $("input[name= 'alarmcode']").val("");
                        $("input[name= 'levels']").val("");
                        $("input[name= 'type']").val("");
                        $("input[name= 'message']").val("");
                        $("input[name= 'troubleshooting']").val("");
                        $("input[name= 'class']").val("");
                    },
                    yes: function () {
                    },
                    end: function (index, layero) {
                        $("#add-main").css("display", "none");
                    }
                });
                break;
            case 'import':
                layer.open({
                    type: 1,
                    title: '导入',
                    closeBtn: false,
                    area: ['400px', '200px'],
                    shadeClose: true,
                    content: $("#import-main"),
                    success: function (layero, index) {
                        $("#excel-file").val("");
                    },
                    yes: function () {

                    },
                    end: function (index, layero) {
                        $("#import-main").css("display", "none");
                    }
                });
                break;
            case 'export':
                var data = checkStatus.data;
                table.exportFile(alarmtable.config.id, data, 'csv');
                break;
            case 'search':
                var filter = $("#searchfield").val();

                table.reload('alarmconfig', {
                    url: './Alarm/GetAlarmConfig',
                    where: {
                        filter: filter
                    }
                })

                $("#searchfield").val(filter);
                break;
        }
    })

    table.on('edit(alarmconfig)', function (obj) {
        var value = obj.value//得到修改后的值
            , data = obj.data//得到所在行所有键值
            , field = obj.field;//得到字段名

        layer.confirm('确认修改字段' + field + '为' + value + '?', { icon: 3, title: '提示' }, function (index) {

            $.ajax({
                url: './Alarm/Edit',
                data: {
                    "id": data.ALARM_CODE,
                    "value": value,
                    "field": field
                },
                type: 'POST',
                contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
                async: false,
                success: function (data) {
                    layer.msg('<em style="color:white;font-style:normal;font-weight:normal">修改成功！</em>', { icon: 1 });
                    alarmtable.reload();
                },
                error: function (message) {
                    alert('error!');
                }
            });

            layer.close(index);
        },
            function (index) {
                layer.msg('取消修改');
                alarmtable.reload();
            }
        )

    })

    form.on('submit(save)', function (data) {
        var params = data.field;

        $.ajax({
            url: './Alarm/Add',
            data: {
                "data":params
            },
            type: 'POST',
            contentType: 'application/x-www-form-urlencoded; chartset=UTF-8',
            async: false,
            success: function (data) {
                if (data == 'True') {
                    layer.msg('<em style="color:white;font-style:normal;font-weight:normal">添加成功！</em>', { icon: 1 });
                    parent.location.reload();
                }
                else
                    layer.msg('<em style="color:white;font-style:normal;font-weight:normal">添加失败！</em>', { icon: 5 });
            },
            error: function (message) {
                alert('Error!');
            }
        })
        return false;
    })

    form.verify({
        type:[/^\d{1}$/,'TYPE必须为一位的数字！']
    })
})

function ImportExcle(persons) {
    if (persons.length > 0) {
        $.ajax({
            url: './Alarm/Importdata',
            data: {
                "data": persons
            },
            type: 'POST',
            contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
            async: false,
            success: function (res) {
                if (res == "True") {
                    layui.layer.msg('<em style="color:white;font-style:normal;font-weight:normal">导入成功！</em>', { icon: 1 });
                    parent.location.reload();
                }
                else
                    layui.layer.msg('<em style="color:white;font-style:normal;font-weight:normal">导入失败！</em>', { icon: 5 });

            },
            error: function (message) {
                alert('error!');
            }
        });
    }
}