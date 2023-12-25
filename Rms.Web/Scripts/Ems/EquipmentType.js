
window.currentProcess;

layui.use(['layer', 'table', 'form', 'upload', 'element'], function () {
    var layer = layui.layer
        , table = layui.table
        , form = layui.form
        , upload = layui.upload
        , element = layui.element;
    var processselect = document.getElementById("process");


    getProcess();

    form.on('select(process)', function (data) {
        window.currentProcess = data.value; // 获取选中的值
        ShowEquipmentTypeTable(window.currentProcess);
    });

    async function getProcess() {
        try {
            let result1 = await $.ajax({
                type: 'post',
                dataType: 'json',
                url: '/EquipmentType/GetProcesses',
                data: {
                },
                success: function (data) {

                    //var uniqueValues = new Set(data.eqpData.map(item => item.TYPEPROCESS));
                    //var processes = Array.from(uniqueValues);
                    // 清空processselect元素中的选项
                    processselect.innerHTML = "";
                    //添加默认选项
                    var defaultOption = document.createElement("option");
                    defaultOption.value = "";
                    defaultOption.text = "All";
                    processselect.add(defaultOption);
                    // 自动生成其他选项
                    for (var i = 0; i < data.length; i++) {
                        var option = document.createElement("option");
                        option.text = data[i];
                        processselect.add(option);
                    }
                    form.render('select');
                    ShowEquipmentTypeTable("");

                },
                error: function () {
                }
            });


        } catch (error) {
            //处理错误
        }
    }




    function ShowEquipmentTypeTable(processfilter) {
        window.rcptable = table.render({
            elem: '#equipmenttypetable'
            , url: '/EquipmentType/GetEquipmentTypes'
            //, toolbar: '#addnewrcp'
            , id: "equipmenttypetable"
            , limit: 1000
            , limits: [1000]
            , height: 'full-235'
            , cols: [[
                { field: 'ID', title: 'ID' }
                , { field: 'NAME', title: 'NAME', edit: true }
                , { field: 'PROCESS', title: 'PROCESS', edit: true }
                , { field: 'VENDOR', title: '供应商', edit: true }
                , { field: 'TYPE', title: '型号', edit: true }
                , { field: 'DELETEBEFOREDOWNLOAD', title: '下载前清空设备', edit: true }
                , { field: 'ORDERSORT', title: '排序', edit: true }
                , { fixed: 'right', width: 220, align: 'center', toolbar: '#toolbar' }
            ]]
            , where: {
                processfilter: processfilter
            }

            , done: function (data) {

            }
        });

    }

    table.on('edit(equipmenttypetable)', function (obj) {

        layer.confirm('is not?', { icon: 3, title: '修改确认', content: obj.value }, function (index) {
            console.log(obj);
            $.ajax({
                url: '/EquipmentType/Edit',
                data: {
                    "ID": obj.data.ID,
                    "value": obj.value,
                    "field": obj.field
                },
                type: 'POST',
                contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
                async: false,
                success: function (data) {
                    layer.msg(data.message);
                    ShowEquipmentTypeTable(window.currentProcess);
                },
                error: function (message) {
                    alert('error!');
                }
            });
            layer.close(index);
        },
            function (index) {
                layer.msg('取消修改');
                ectable.reload();
            }

        );
    });


    table.on('tool(equipmenttypetable)', function (obj) {
        console.log(obj);
        var event = obj.event, //当前单元格事件属性值
            selectdata = obj.data;
        if (event === 'permission') {
            layer.open({
                title: '更改权限'
                , type: 2
                , btn: ['确定', '取消']
                , content: 'TypeRole?TYPEID=' + selectdata.ID
                , area: ['40%', '85%']
                , success: function (layero, index) {

                }
                , yes: function (index) {
                    var res = window["layui-layer-iframe" + index].callback();
                    console.log(res);
                    var jdata = JSON.parse(res);
                    console.log(jdata);
                    var roleids = jdata.roles.map(obj => obj.ID);
                    console.log(roleids);
                    $.ajax({
                        url: '/EquipmentType/SetTypeRoles',//控制器活动,返回一个分部视图,并且给分部视图传递数据.
                        data: {
                            TypeId: selectdata.ID,
                            RoleIds: roleids
                        },//传给服务器的数据(即后台AddUsers()方法的参数,参数类型要一致才可以)
                        type: 'POST',
                        contentType: 'application/x-www-form-urlencoded; charset=UTF-8',//数据类型必须有
                        async: false,
                        success: function (data) {
                            layer.open({ content: data.message });

                        },
                        error: function (message) {
                            alert('error!');
                        }
                    });
                    layer.close(index);


                }, btn2: function (index, layero) {
                    layer.msg('取消操作');
                }

            });
        }
        else if (event === 'auditprocess') {
            layer.open({
                title: '更改签核流程'
                , type: 2
                , btn: ['确定', '取消']
                , content: 'AuditProcessPage?TYPEID=' + selectdata.ID
                , area: ['90%', '85%']
                , success: function (layero, index) {

                }
                , yes: function (index) {
                    var res = window["layui-layer-iframe" + index].callback();
                    console.log(res);
                    var jdata = JSON.parse(res);
                    console.log(jdata);
                    var roleids = jdata.roles.map(obj => obj.value);
                    console.log(roleids);
                    $.ajax({
                        url: '/EquipmentType/SetFlowRoles',//控制器活动,返回一个分部视图,并且给分部视图传递数据.
                        data: {
                            TypeId: selectdata.ID,
                            RoleIds: roleids
                        },//传给服务器的数据(即后台AddUsers()方法的参数,参数类型要一致才可以)
                        type: 'POST',
                        contentType: 'application/x-www-form-urlencoded; charset=UTF-8',//数据类型必须有
                        async: false,
                        success: function (data) {
                            layer.open({ content: data.message });
                        },
                        error: function (message) {
                            alert('error!');
                        }
                    });
                    layer.close(index);


                }, btn2: function (index, layero) {
                    layer.msg('取消操作');
                }

            });
        }

    });
});