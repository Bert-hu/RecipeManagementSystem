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
                //IsHandledΪfalse�ı���ɫ���ɫ
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


    table.on('tool(ALarmActionsTable)', function (obj) { //ע��tool�ǹ������¼�����test��tableԭʼ���������� lay-filter="��Ӧ��ֵ"
        var data = obj.data; //��õ�ǰ������
        var layEvent = obj.event; //��� lay-event ��Ӧ��ֵ��Ҳ�����Ǳ�ͷ�� event ������Ӧ��ֵ��
        var tr = obj.tr; //��õ�ǰ�� tr ��DOM����
        console.log(data);
        console.log(layEvent);
        if (layEvent === 'confirm') { //�鿴
            //do somehing
            layer.prompt({
                formType: 2,
                value: '',
                title: 'Input Remark',
                area: ['800px', '350px'] //�Զ����ı�����
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