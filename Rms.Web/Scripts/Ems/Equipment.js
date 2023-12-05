
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
        var selectedValue = data.value; // 获取选中的值
        ShowEquipmentTable(selectedValue);
    });

    async function getProcess() {
        try {
            let result1 = await $.ajax({
                type: 'post',
                dataType: 'json',
                url: '/Equipment/GetProcesses',
                data: {
                },
                success: function (data) {

                    //var uniqueValues = new Set(data.eqpData.map(item => item.TYPEPROCESS));
                    //var processes = Array.from(uniqueValues);
                    // 清空processselect元素中的选项
                    processselect.innerHTML = "";
                    // 添加默认选项
                    //var defaultOption = document.createElement("option");
                    //defaultOption.value = "";
                    //defaultOption.text = "请选择Process";
                    //processselect.add(defaultOption);
                    // 自动生成其他选项
                    for (var i = 0; i < data.length; i++) {
                        var option = document.createElement("option");
                        option.text = data[i];
                        processselect.add(option);
                    }
                    form.render('select');
                    ShowEquipmentTable(data[0]);

                },
                error: function () {
                }
            });


        } catch (error) {
            //处理错误
        }
    }

    async function getEQP(process) {
        try {
            let result1 = await $.ajax({
                type: 'post',
                dataType: 'json',
                url: '/Equipment/GetEQPs',
                data: {
                    page: 1,
                    limit: 9999,
                    processfilter: process
                },
                success: function (data) {




                },
                error: function () {
                }
            });


        } catch (error) {
            //处理错误
        }
    }

    function ShowEquipmentTable(processfilter) {
        window.rcptable = table.render({
            elem: '#equipmenttable'
            , url: '/Equipment/GetEQPs'
            //, toolbar: '#addnewrcp'
            , id: "equipmenttable"
            , limit: 1000
            , limits: [1000]
            , height: 'full-235'
            , cols: [[
                { field: 'TYPEPROCESS', title: 'Process' }
                , { field: 'TYPENAME', title: '设备类型' }
                , { field: 'TYPETYPE', title: '设备型号' }
                , { field: 'TYPEVENDOR', title: '厂商' }
                , { field: 'ID', title: 'EQID' }
                , { field: 'NAME', title: '名称', edit: true }
                , { field: 'RECIPE_TYPE', title: 'Recipe类型' }

                // , { fixed: 'right', width: '15%', align: 'center', toolbar: '#equipmenttoolbar' }
            ]]
            , where: {
                processfilter: processfilter
            }

            , done: function (data) {

            }
        });

    }

    table.on('edit(equipmenttable)', function (obj) { 

        layer.confirm('is not?', { icon: 3, title: '修改确认', content: obj.value }, function (index) {
            console.log(obj);
            $.ajax({
                url: '/Equipment/Edit',
                data: {
                    "EQID": obj.data.ID,
                    "TYPEID": obj.data.TYPEID,
                    "value": obj.value,
                    "field": obj.field
                },
                type: 'POST',
                contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
                async: false,
                success: function (data) {
                    layer.msg(data.message);
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
});