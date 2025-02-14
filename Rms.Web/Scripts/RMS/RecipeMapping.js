

window.recipegrouptable;
window.equipmenttable;
layui.use(['layer', 'table', 'form', 'upload', 'element'], function () {
    var layer = layui.layer
        , table = layui.table
        , form = layui.form
        , upload = layui.upload
        , element = layui.element;

    var LineSel = xmSelect.render({
        el: '#lines',
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

            ShowEquipmentTable();
        },

    })
    var currentRecipeGroup;
    var currentLine;
    var newversionlock = true;

    function ShowRecipeGroupTable() {
        window.recipegrouptable = table.render({
            elem: '#rcpgrouptable'
            , url: '/RecipeMapping/GetRecipeGroups'
            , toolbar: '#addnewgroup'
            , id: "rcpgrouptable"            
            , limit: 1000
            , limits: [1000]
            , height: 'full-235'
            , cols: [[
                { field: 'NAME', title: 'Name', sort: true }
            ]]
           
        });
        
    }

    function ShowEquipmentTable() {
        //var selectedline = LineSel.getValue()[0].value;
        var selectedline = currentLine;
        console.log(selectedline);
        window.rcptable = table.render({
            elem: '#equipmenttable'
            , url: '/RecipeMapping/GetEquipments'
            //, toolbar: '#addnewrcp'
            , id: "equipmenttable"
            , limit: 1000
            , limits: [1000]
            , height: 'full-235'
            , cols: [[
                { field: 'LINE', title: 'LINE' }
                ,{ field: 'EQUIPMENT_ID', title: 'EQID' }
                , { field: 'EQUIPMENT_TYPE_NAME', title: 'EQ Type' }
                , { field: 'EQUIPMENT_NAME', title: 'Name' }
                , { field: 'RECIPE_NAME', title: 'Recipe Name' }

               // , { field: 'RECIPE_EFFECTIVE_VERSION', title: '生效版本', width: '15%' }
                , { fixed: 'right', width: '15%', align: 'center', toolbar: '#equipmenttoolbar' }
            ]]
            , where: {
                recipegroup_id: currentRecipeGroup,
                line: selectedline,
                processfilter: selectedline
            }
          
            , done: function (data) {
    
            }
        });

    }




    Initialization();

    async function Initialization() {
        try {
            let result1 = await $.ajax({
                type: 'post',
                dataType: 'json',
                url:'/Equipment/GetProcesses',//'/RecipeMapping/GetLines',
                data: {
       
                },
                success: function (data) {
                    
                    var seldata = data.map((item,index) => ({
                        name: item,
                        value: item,
                        selected: index===0
                    }));

             
                    LineSel.update({
                        data: seldata,
                    });
                    currentLine = data[0];
                    ShowRecipeGroupTable();
                },
                error: function () {
                }
            });


        } catch (error) {
            //处理错误
        }
    }


    table.on('tool(equipmenttable)', function (obj) {
        var event = obj.event, //当前单元格事件属性值
            selectdata = obj.data;
        console.log(selectdata);
        if (event === 'modify') {
            layer.open({
                title: '更改Recipe绑定/Change recipe binding'
                , type: 2
                , btn: ['确定Confirm', '取消Cancel']
                , content: 'BindingRecipePage?EQUIPMENT_ID=' + selectdata.EQUIPMENT_ID + '&RECIPE_GROUP_ID=' + selectdata.RECIPE_GROUP_ID
                , area: ['40%', '85%']
                , success: function (layero, index) {


                }
                , yes: function (index) {
                    var res = window["layui-layer-iframe" + index].callback();
                    console.log(res);
                    var jdata = JSON.parse(res);
                    console.log(jdata);
                    var recipeid = jdata.selectData[0].ID;

                    $.ajax({
                        url: '/RecipeMapping/SetRecipeBinding',//控制器活动,返回一个分部视图,并且给分部视图传递数据.
                        data: {
                            RECIPE_ID: recipeid,
                            EQUIPMENT_ID: selectdata.EQUIPMENT_ID,
                            RECIPE_GROUP_ID: selectdata.RECIPE_GROUP_ID
                        },//传给服务器的数据(即后台AddUsers()方法的参数,参数类型要一致才可以)
                        type: 'POST',
                        contentType: 'application/x-www-form-urlencoded; charset=UTF-8',//数据类型必须有
                        async: false,
                        success: function (data) {
                            layer.open({ content: data.message });
                            ShowEquipmentTable();
                        },
                        error: function (message) {
                            alert('error!');
                        }
                    });
                    layer.close(index);


                }, btn2: function (index, layero) {
                    layer.msg('取消操作.Cancel.');
                }

            });
        }
    })


    table.on('row(rcpgrouptable)', function (obj) {

        var data = obj.data;//data为当前点击行的数据
        //console.log(data)
        var selectGroup = data.ID;
        currentRecipeGroup = selectGroup;
        ShowEquipmentTable();
        //selid = data.ID;
        //LoadRecipeVersion(selid);//级联，调用右表数据加载函数

        //标注选中样式
        obj.tr.addClass('selected').siblings().removeClass('selected');
    });

  

   
    table.on('toolbar(rcpgrouptable)', function (obj) {
        console.log(obj)
        console.log(obj.config.currenteqpid)
        window.selectedEQP = { "eqid": obj.config.currenteqpid };
        if (obj.event == 'add') {
            layer.prompt({
                formType: 2,

                title: 'Enter new Model Name',
                area: ['300px', '35px'] //自定义文本域宽高
            }, function (value, index, elem) {


                $.ajax({
                    type: 'post',
                    dataType: 'json',
                    url: '/RecipeMapping/AddRecipeGroup',
                    data: {
                        recipegroupname: value
                    },
                    success: function (data) {
                        if (data.Result) {
                            layer.close(index)
                            window.recipegrouptable.reload();
                            layer.msg('<em style="color:white;font-style:normal;font-weight:normal">' + '成功.Success.' + '</em>');
                            //return false;
                        } else {
                            layer.open({
                                title: '失败.Fail.'
                                , content: data.Message
                            });
                        }

                    },
                    error: function (err) {
                        console.log(err)
                    }
                });

            });
        }
      
       

    })

    //表单提交
   

  
});