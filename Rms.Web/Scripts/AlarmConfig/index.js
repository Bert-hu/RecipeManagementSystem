layui.use(['layer', 'table', 'form', 'upload', 'element'], function () {
    var layer = layui.layer
        , table = layui.table
        , form = layui.form
        , upload = layui.upload
        , element = layui.element;

    var processSel = xmSelect.render({
        el: '#processSelect',
        //initValue: [0],
        radio: true,
        clickClose: true,
        model: {
            icon: 'hidden',
            label: {
                type: 'text'
            }
        },
        tips: '��ѡ��Process',
        block: {
            //�����ʾ����, 0:������
            showCount: 0,
            //�Ƿ���ʾɾ��ͼ��
            showIcon: false,
        },
        on: function (data) {
            //arr:  ��ǰ��ѡ��ѡ�е�����
            var arr = data.arr;
            //change, �˴�ѡ��仯������,����
            var change = data.change;
            //isAdd, �˴β�������������ɾ��
            var isAdd = data.isAdd;

            currentLine = arr[0].value;

            //var projectid = change[0].value;

            //var line = change[0].value;

            ShowEquipmentTypeTable(arr[0].value);
        },

    })


    function ShowEquipmentTypeTable(process) {
       table.render({
           elem: '#equipmentTypeTable'
            , url: '/AlarmConfig/GetEquipmentType'
           , id: "equipmentTypeTable"
            , limit: 1000
            , limits: [1000]
            , height: 'full-235'
            , cols: [[
                { field: 'NAME', title: 'Name', sort: true },
                { field: 'VENDOR', title: 'Vendor', sort: true },
                { field: 'TYPE', title: 'Type', sort: true }
            ]]
            , where: {
                process: process
            }

        });

    }

    function ShowAlarmConfigTable(equipmentTypeId) {
        window.configTable = table.render({
            elem: '#configTable'
            , url: '/AlarmConfig/GetAlarmConfig'
            , toolbar: '#configHeaderToolBar'
            , id: "configTable"
            , limit: 1000
            , limits: [1000]
            , height: 'full-235'
            , cols: [[
                { field: 'NAME', title: 'Name' }
                , { field: 'ALID', title: 'AlarmCode' }
                , { field: 'ALTX', title: 'AlarmText' }
                , { field: 'TRIGGER_INTERVAL', title: 'Trigger Interval(min)' }
                , { field: 'TRIGGER_COUNT', title: 'Trigger Count' }
                , { field: 'ISVALID', title: 'Enabled' }

                // , { field: 'RECIPE_EFFECTIVE_VERSION', title: '��Ч�汾', width: '15%' }
                , { fixed: 'right', width: '15%', align: 'center', toolbar: '#configToolBar' }
            ]],
            done: function (res, curr, count, origin) {
                console.log(res);
                var that = $("#configTable").siblings();
                res.data.forEach(function (item, index) {
                    console.log(index);
                    var tr = that.find(".layui-table-box tbody tr[data-index='" + index + "']");
                    if (item.ISVALID == false) {//��ɫ
                        tr.css("background-color", "#FFB800");
                    }
                });
            }
            , where: {
                equipmentTypeId: equipmentTypeId
            }

       
        });

    }




    Initialization();

    async function Initialization() {
        try {
            let result1 = await $.ajax({
                type: 'post',
                dataType: 'json',
                url: '/AlarmConfig/GetProcesses',//'/RecipeMapping/GetLines',
                data: {

                },
                success: function (data) {

                    var seldata = data.map((item, index) => ({
                        name: item,
                        value: item,
                        selected: index === 0
                    }));


                    processSel.update({
                        data: seldata,
                    });

                    ShowEquipmentTypeTable(data[0]);
                },
                error: function () {
                }
            });


        } catch (error) {
            //�������
        }
    }


    table.on('tool(configTable)', function (obj) {
        var event = obj.event, //��ǰ��Ԫ���¼�����ֵ
            selectdata = obj.data;
        console.log(selectdata);
        if (event === 'delete') {
            layer.confirm('Delete Config?', { icon: 3 }, function () {
                $.post('/AlarmConfig/DeleteConfig', { configId: selectdata.ID }, function (res) {
                    // ������Ӧ����
                    console.log(res);
                    if (res.result) {
                        window.configTable.reload();
                        layer.msg("OK", { icon: 1 });
                    } else {
                        layer.msg(res.message);
                    }
                });

            }, function () {
            });

        }
        else if (event === 'edit') {
            layer.open({
                type: 2,
                content: 'AddConfig?equipmentTypeId=' + selectdata.EQUIPMENT_TYPE_ID + '&configId=' + selectdata.ID,
                title: 'Edit Config',
                area: ['60%', '45%'],
                success: function (layero, index) {
                    ////��layerҳ�洫ֵ����ֵ��Ҫ����
                    //var body = layer.getChildFrame('body', index);
                    ////������ҳ����������id="xxxx"�ı�ǩ��ֵ
                    //body.find("[id='equipmentTypeId']").val(equipmentTypeId);
                    //��equipmentTypeId���ݸ���ҳ��
                    var iframeWindow = layero.find('iframe')[0].contentWindow;

                    // ֱ�Ӹ�ֵ����ҳ��ı���
                    //iframeWindow.equipmentTypeId = equipmentTypeId;
                    //iframeWindow.configId = selectdata.ID;
                }
            }, function (value, index, elem) {




            });
        }
    })


    table.on('row(equipmentTypeTable)', function (obj) {

        var data = obj.data;//dataΪ��ǰ����е�����
        //console.log(data)
        var selectEquipmentTypeId = data.ID;
        ShowAlarmConfigTable(selectEquipmentTypeId);
        //��עѡ����ʽ
        obj.tr.addClass('selected').siblings().removeClass('selected');
    });




    table.on('toolbar(configTable)', function (obj) {
        //console.log(obj);
        //console.log(obj.config.where.equipmentTypeId);
        var equipmentTypeId = obj.config.where.equipmentTypeId;
  
        window.selectedEQP = { "eqid": obj.config.currenteqpid };
        if (obj.event == 'add') {
            layer.open({
                type: 2,
                content: 'AddConfig',
                title: 'AddConfig',
                area: ['60%', '45%'],
                success: function (layero, index) {
                    ////��layerҳ�洫ֵ����ֵ��Ҫ����
                    //var body = layer.getChildFrame('body', index);
                    ////������ҳ����������id="xxxx"�ı�ǩ��ֵ
                    //body.find("[id='equipmentTypeId']").val(equipmentTypeId);
                    //��equipmentTypeId���ݸ���ҳ��
                    var iframeWindow = layero.find('iframe')[0].contentWindow;

                    // ֱ�Ӹ�ֵ����ҳ��ı���
                    iframeWindow.equipmentTypeId = equipmentTypeId;
                }
            }, function (value, index, elem) {


              

            });
        }



    })




});