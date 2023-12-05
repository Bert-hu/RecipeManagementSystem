
window.versiontable;
layui.use(['layer', 'table', 'form', 'upload', 'element'], function () {
    var layer = layui.layer
        , table = layui.table
        , form = layui.form
        , upload = layui.upload
        , element = layui.element;

    var EQPsel = xmSelect.render({
        el: '#eqps',
        initValue: [0],
        radio: true,
        tips: '请选择设备',
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

            console.log(change);
            //var projectid = change[0].value;

            var eqpid = change[0].value;
            document.getElementById("info").innerHTML = eqpid;
            ShowTable(eqpid);
        },

    })

    var rcptable;
    var newversionlock = true;

    function ShowVersionTable(id) {
        versiontable = table.render({
            elem: '#versiontable'
            , url: '/Recipe/GetVersions'
            , toolbar: '#addnewversion'
            , id: "versiontable"
            , limit: 1000
            , limits: [1000]
            , height: 'full-235'
            , cols: [[
                { field: 'VERSION', title: '版本号', width: '10%' , sort: true }
                //, { field: 'NAME', title: '版本', width: 200 }
                , {
                    field: 'CURRENT_FLOW_INDEX', title: '签核状态',  width: '45%',
                    templet: function (d, s) {
                        var strstate;
                        if (d.CURRENT_FLOW_INDEX == 100) {
                            strstate = '已完成'
                        } else if (d.CURRENT_FLOW_INDEX == -1) {
                            strstate = '未提交'
                        }
                        else {
                            strstate = d._FLOW_ROLES[d.CURRENT_FLOW_INDEX];
                        }
                        return strstate;
                    }
                }
                , { field: 'CREATOR', title: '创建者', width: '15%' }
                , { field: 'CREATE_TIME', title: '创建时间', templet: '<div>{{ FormDate(d.CREATE_TIME, "yyyy-MM-dd HH:mm:ss") }}</div>', width: '20%' }
                , { fixed: 'right', width: '10%', align: 'center', toolbar: '#versiontoolbar' }

            ]]
            , where: {
                rcpID: id
            }
            , currentrcpid: id
            , done: function (data) {
                newversionlock = data.newversionlock;
                var that = $("#versiontable").siblings();
                console.log(data);
                data.data.forEach(function (item, index) {
                    var tr = that.find(".layui-table-box tbody tr[data-index='" + index + "']");


                    if (item.ID === data.effective_version_id) {//绿色
                        tr.css("background-color", "#5FB878");
                    }
                    console.log(item.CURRENT_FLOW_INDEX);
                    if (item.CURRENT_FLOW_INDEX < 100) {//黄色
                        tr.css("background-color", "#FFB800");
                    }
                    if (item.CURRENT_FLOW_INDEX == -1) {

                        tr.css("background-color", "#C0C0C0");
                    }
                });
            }
        });
        
    }

    function ShowTable(id) {
        rcptable = table.render({
            elem: '#changetable'
            , url: '/RecipeChange/GetRecipeChangeRecords'
            , toolbar: '#addnewrcp'
            , id: "changetable"
            , limit: 1000
            , limits: [1000]
            , height: 'full-235'
            , cols: [[
                { field: 'EQID', title: '设备ID', width: '15%' }
                //, { field: 'FROM_RECIPE_NAME', title: 'FROM', width: '20%' }
                //, { field: 'FROM_RECIPE_VERSION', title: 'Server版本', width: '10%' }
                , { field: 'TO_RECIPE_NAME', title: 'Recipe Name', width: '20%' }
                , { field: 'TO_RECIPE_VERSION', title: '版本', width: '10%' }
                , { field: 'CREATOR', title: '操作人', width: '10%' }
                //, { field: 'RECIPE_EFFECTIVE_VERSION', title: '生效版本', width: '25%' }
                , { field: 'CREATETIME', title: '操作时间', templet: '<div>{{ FormDate(d.CREATETIME, "yyyy-MM-dd HH:mm:ss") }}</div>', width: 180 }
                //, { fixed: 'right', title: '操作', width: '20%', align: 'center', toolbar: '#rcptoolbar' }

            ]]
            , where: {
                EQID: id
            }
            , currenteqpid: id
            , done: function (data) {

            }
        });

    }




    getEQP();

    async function getEQP() {
        try {
            let result1 = await $.ajax({
                type: 'post',
                dataType: 'json',
                url: '/Equipment/GetEQPs',
                data: {
                    page: 1,
                    limit: 9999
                },
                success: function (data) {
                    var seldata = data.data.map(it => {
                        return {
                            name: it.TYPEPROCESS + "--" + it.TYPENAME + "--" + it.ID,
                            value: it.ID
                        };
                    });
                    
                    var eqpid = data.data[0].ID;
                    document.getElementById("info").innerHTML = eqpid;
                    ShowTable(eqpid);

                    EQPsel.update({
                        data: seldata,
                        
                    });
                    
                    //console.log(EQPsel)

                },
                error: function () {
                }
            });


        } catch (error) {
            //处理错误
        }
    }

});