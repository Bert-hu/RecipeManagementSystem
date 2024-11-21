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
        tips: '请选择Process',
        block: {
            //最大显示数量, 0:不限制
            showCount: 0,
            //是否显示删除图标
            showIcon: false,
        },
        on: function (data) {
            //arr:  当前多选已选中的数据
            var arr = data.arr;
            //change, 此次选择变化的数据,数组
            var change = data.change;
            //isAdd, 此次操作是新增还是删除
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

                // , { field: 'RECIPE_EFFECTIVE_VERSION', title: '生效版本', width: '15%' }
                , { fixed: 'right', width: '15%', align: 'center', toolbar: '#configToolBar' }
            ]],
            done: function (res, curr, count, origin) {
                console.log(res);
                var that = $("#configTable").siblings();
                res.data.forEach(function (item, index) {
                    console.log(index);
                    var tr = that.find(".layui-table-box tbody tr[data-index='" + index + "']");
                    if (item.ISVALID == false) {//黄色
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
            //处理错误
        }
    }


    table.on('tool(configTable)', function (obj) {
        var event = obj.event, //当前单元格事件属性值
            selectdata = obj.data;
        console.log(selectdata);
        if (event === 'delete') {
            layer.confirm('Delete Config?', { icon: 3 }, function () {
                $.post('/AlarmConfig/DeleteConfig', { configId: selectdata.ID }, function (res) {
                    // 处理响应数据
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
                    ////向layer页面传值，传值主要代码
                    //var body = layer.getChildFrame('body', index);
                    ////将弹窗页面中属性名id="xxxx"的标签赋值
                    //body.find("[id='equipmentTypeId']").val(equipmentTypeId);
                    //把equipmentTypeId传递给子页面
                    var iframeWindow = layero.find('iframe')[0].contentWindow;

                    // 直接赋值给子页面的变量
                    //iframeWindow.equipmentTypeId = equipmentTypeId;
                    //iframeWindow.configId = selectdata.ID;
                }
            }, function (value, index, elem) {




            });
        }
    })


    table.on('row(equipmentTypeTable)', function (obj) {

        var data = obj.data;//data为当前点击行的数据
        //console.log(data)
        var selectEquipmentTypeId = data.ID;
        ShowAlarmConfigTable(selectEquipmentTypeId);
        //标注选中样式
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
                    ////向layer页面传值，传值主要代码
                    //var body = layer.getChildFrame('body', index);
                    ////将弹窗页面中属性名id="xxxx"的标签赋值
                    //body.find("[id='equipmentTypeId']").val(equipmentTypeId);
                    //把equipmentTypeId传递给子页面
                    var iframeWindow = layero.find('iframe')[0].contentWindow;

                    // 直接赋值给子页面的变量
                    iframeWindow.equipmentTypeId = equipmentTypeId;
                }
            }, function (value, index, elem) {


              

            });
        }



    })




});