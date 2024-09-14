layui.use(['layer', 'table', 'form', 'upload', 'element'], function () {
    var layer = layui.layer
        , table = layui.table
        , form = layui.form
        , upload = layui.upload
        , element = layui.element;


    Initialization();

    async function Initialization() {
        window.actionTable = table.render({
            elem: '#ALarmActionsTable'
            , url: '/AlarmConfig/GetAlarmActions'
            //, toolbar: '#configHeaderToolBar'
            , id: "ALarmActionsTable"
            , limit: 1000
            , limits: [1000]
            , height: 'full-235'
            , cols: [[
                { field: 'DATETIME', title: 'Time', templet: '<div>{{ FormDate(d.DATETIME, "yyyy-MM-dd HH:mm:ss") }}</div>', width: 180 }
                , { field: 'EQID', title: 'Equipment' }
                , { field: 'ALID', title: 'AlarmCode' }
                , { field: 'ISHANDLED', title: 'IsHandled' }
                , { field: 'USERNAME', title: 'HandledBy' }
                , { field: 'REMARK', title: 'Remark', width: 480 }
                , { fixed: 'right', width: 100, align: 'center', toolbar: '#actionToolBar' }
            ]]
            , where: {
            }

            , done: function (data) {
                //IsHandled为false的背景色变黄色
                var that = $("#ALarmActionsTable").siblings();
                //console.log(data);
                data.data.forEach(function (item, index) {
                    if (item.ISHANDLED === false) {
                        that.find("tr[data-index=" + index + "]").css("background", "orange");
                    }
                });
            }
        });
    }


    table.on('tool(ALarmActionsTable)', function (obj) { //注：tool是工具条事件名，test是table原始容器的属性 lay-filter="对应的值"
        var data = obj.data; //获得当前行数据
        var layEvent = obj.event; //获得 lay-event 对应的值（也可以是表头的 event 参数对应的值）
        var tr = obj.tr; //获得当前行 tr 的DOM对象
        console.log(data);
        console.log(layEvent);
        if (layEvent === 'confirm') { //查看
            //do somehing
            layer.prompt({
                formType: 2,
                value: '',
                title: 'Input Remark',
                area: ['800px', '350px'] //自定义文本域宽高
            }, function (value, index, elem) {
                $.post('/AlarmConfig/HandleAction', {actionId:data.ID, remarkText: value}, function (res) {
                    if (!res.result) {
                        layer.msg(res.message);
                    }
                });
                window.actionTable.reload();
                layer.close(index);
            });
        } 
    });





});