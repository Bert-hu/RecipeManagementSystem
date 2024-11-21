
window.versiontable;
window.rcptable;
window.currenteqid;
layui.use(['layer', 'table', 'form', 'upload', 'element'], function () {
    var layer = layui.layer
        , table = layui.table
        , form = layui.form
        , upload = layui.upload
        , element = layui.element;
    var processselect = document.getElementById("process");
    var EQPsel = xmSelect.render({
        el: '#eqps',
        initValue: [0],
        filterable: true,
        radio: true,
        tips: '请选择设备',
        clickClose: true,
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
            window.currenteqid = change[0].value;
            document.getElementById("info").innerHTML = eqpid;
            ShowRCPTable(eqpid);

        },

    })


    var newversionlock = true;

    function ShowVersionTable(id) {
        window.versiontable = table.render({
            elem: '#versiontable'
            , url: '/Recipe/GetVersions'
            , toolbar: '#addnewversion'
            , id: "versiontable"

            , limit: 1000
            , limits: [1000]
            , height: 'full-235'
            , cols: [[
                { field: 'VERSION', title: 'Version No.', width: '15%', sort: true }
                //, { field: 'NAME', title: '版本', width: 200 }
                , {
                    field: 'CURRENT_FLOW_INDEX', title: 'Status', width: '30%',
                    templet: function (d, s) {
                        var strstate;
                        if (d.CURRENT_FLOW_INDEX == 100) {
                            strstate = '已完成/Finished'
                        } else if (d.CURRENT_FLOW_INDEX == -1) {
                            strstate = '未提交/'
                        }
                        else {
                            strstate = d._FLOW_ROLES[d.CURRENT_FLOW_INDEX];
                        }
                        return strstate;
                    }
                }
                , { field: 'CREATOR', title: 'User', width: '15%' }
                , { field: 'CREATE_TIME', title: 'Date Time', templet: '<div>{{ FormDate(d.CREATE_TIME, "yyyy-MM-dd HH:mm:ss") }}</div>', width: '30%' }
                , { fixed: 'right', width: '10%', align: 'center', toolbar: '#versiontoolbar' }

            ]]
            , where: {
                rcpID: id
            }
            , currentrcpid: id
            , done: function (data) {
                newversionlock = data.newversionlock;
                var that = $("#versiontable").siblings();
                //console.log(data);
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

    function ShowRCPTable(id) {
        window.rcptable = table.render({
            elem: '#rcptable'
            , url: '/Recipe/GetRecipe'
            , toolbar: '#addnewrcp'
            , id: "rcptable"
            , limit: 1000
            , limits: [1000]
            , height: 'full-235'
            , cols: [[
                { field: 'RECIPE_NAME', title: 'Recipe Name', width: '400' }
                , { field: 'RECIPE_LATEST_VERSION', title: 'Latest Version', width: '100' }
                , { field: 'RECIPE_EFFECTIVE_VERSION', title: 'Effective Version', width: '100' }
                , { fixed: 'right', width: '200', align: 'center', toolbar: '#recipetoolbar' }
            ]]
            , where: {
                EQID: id
            }
            , currenteqpid: id
            , done: function (data) {
                newversionlock = data.newversionlock;
                var that = $("#rcptable").siblings();
                console.log(data);
                data.data.forEach(function (item, index) {
                    var tr = that.find(".layui-table-box tbody tr[data-index='" + index + "']");


                    if (item.RECIPE_LATEST_VERSION > item.RECIPE_EFFECTIVE_VERSION) {//黄色
                        //console.log('here')
                        tr.css("background-color", "#fdfd96");
                    }
                    console.log(data.canEdit);
                    if (data.canEdit) {
                        //20231225暂时不开放修改功能
                        //tr.find('a[lay-event="editbody"]').show();

                    }
                });
                //ShowVersionTable(data.data[0].RECIPE_ID)
            }
        });

    }


    getProcess();

    form.on('select(process)', function (data) {
        var selectedValue = data.value; // 获取选中的值
        getEQP(selectedValue);
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
                    console.log(data);

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
                    getEQP(data[0]);
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
                    var seldata = data.data.map(it => {
                        //console.log(it.NAME)
                        return {
                            name: it.TYPEPROCESS + "--" + it.TYPENAME + "--" + it.LINE + "--" + it.ID + "--" + it.NAME,
                            value: it.ID
                        };
                    });

                    var eqpid = data.data[0].ID;
                    document.getElementById("info").innerHTML = eqpid;
                    window.currenteqid = seldata[0].value;
                    console.log(window.currenteqid);
                    ShowRCPTable(eqpid);

                    EQPsel.update({
                        data: seldata,

                    });
                    EQPsel.setValue([
                        seldata[0]
                    ])


                },
                error: function () {
                }
            });


        } catch (error) {
            //处理错误
        }
    }

    //表头工具条
    table.on('toolbar(versiontable)', function (obj) {
        console.log(obj)
        var id = obj.config.id;
        var checkStatus = table.checkStatus(id);

        console.log(versiontable);
        if (newversionlock) {
            console.log(newversionlock)
            layer.msg('<em style="color:white;font-style:normal;font-weight:normal">有未完成的签核版本，禁止新增！There is a unfinished version, no new additions allowed!</em>', { icon: 5 });
            versiontable.reload();
        }
        else {
            switch (obj.event) {
                case 'add':
                    if (confirm('确定新增版本？Are you sure to add a new version?')) {
                        $.ajax({
                            url: '/Recipe/AddNewVersion',
                            data: {
                                recipeid: versiontable.config.currentrcpid,
                            },
                            async: false,
                            type: 'POST',
                            contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
                            success: function (response) {
                                if (response.result) {
                                    ShowRCPTable(window.currenteqid)
                                    ShowVersionDetail(response.data.versionid);
                                } else {
                                    layer.msg('<em style="color:black;font-style:normal;font-weight:normal">' + response.message + '</em>', { icon: response.result ? 1 : 4 });
                                }

                                result = response.result;
                                //rcptable.reload();
                                //window.versiontable.reload();

                            },
                            error: function () {
                                layer.msg('提交失败.Submit fail', { icon: 4 });
                                result = false;
                            }
                        });
                    }
                    break;

            }
        }

    })
    //表中工具条
    table.on('tool(versiontable)', function (obj) { //注：tool 是工具条事件名，recipe 是 table 原始容器的属性 lay-filter="对应的值"
        var selectdata = obj.data //获得当前行数据
            , layEvent = obj.event; //获得 lay-event 对应的值
        if (layEvent === 'detail') {
            ShowVersionDetail(selectdata.ID);



        }

    });

    table.on('row(rcptable)', function (obj) {

        var data = obj.data;//data为当前点击行的数据
        //console.log(data)
        var selectedRcp = data.RECIPE_ID
        //console.log(selectedRcp)
        ShowVersionTable(selectedRcp);
        //selid = data.ID;
        //LoadRecipeVersion(selid);//级联，调用右表数据加载函数

        //标注选中样式
        obj.tr.addClass('selected').siblings().removeClass('selected');
    });

    table.on('tool(rcptable)', function (obj) { //注：tool 是工具条事件名，recipe 是 table 原始容器的属性 lay-filter="对应的值"
        var selectdata = obj.data //获得当前行数据
            , layEvent = obj.event; //获得 lay-event 对应的值
        console.log(layEvent);
        if (layEvent === 'download') {
            layer.prompt({
                formType: 2,

                title: '请确认下载"' + obj.data.RECIPE_NAME + '"到设备吗？请输入Y确认.Enter "Y" to confirm download',
                area: ['80px', '35px'] //自定义文本域宽高
            }, function (value, index, elem) {
                if (value.toUpperCase() === 'Y') {
                    console.log(selectdata.RECIPE_ID);
                    $.ajax({
                        type: 'post',
                        dataType: 'json',
                        url: '/Recipe/DownloadRecipeToApi',
                        data: {
                            rcpID: selectdata.RECIPE_ID
                        },
                        success: function (data) {
                            var resultData = data.replyItem

                            if (resultData.Result) {
                                layer.close(index)
                                window.rcptable.reload();
                                layer.msg('<em style="color:white;font-style:normal;font-weight:normal">' + 'Recipe download succeed.' + '</em>');
                                //return false;
                            } else {

                                layer.msg('<em style="color:white;font-style:normal;font-weight:normal">' + 'Error:' + resultData.Message + '</em>');
                                //return true;
                            }

                        },
                        error: function (err) {
                            console.log(err)
                        }
                    });
                }
            });
        }
        else if (layEvent === 'ppselect') {
            layer.prompt({
                formType: 2,

                title: '请确认切换"' + obj.data.RECIPE_NAME + '"到设备吗？请输入Y确认.Enter "Y" to confirm ppselect',
                area: ['80px', '35px'] //自定义文本域宽高
            }, function (value, index, elem) {
                if (value.toUpperCase() === 'Y') {
                    console.log(selectdata.RECIPE_ID);
                    $.ajax({
                        type: 'post',
                        dataType: 'json',
                        url: '/Recipe/SwitchRecipe',
                        data: {
                            rcpID: selectdata.RECIPE_ID
                        },
                        success: function (data) {
                            var resultData = data

                            if (resultData.Result) {
                                layer.close(index)
                                window.rcptable.reload();
                                layer.msg('<em style="color:white;font-style:normal;font-weight:normal">' + 'PP-SELECT succeed.' + '</em>');
                                //return false;
                            } else {

                                layer.msg('<em style="color:white;font-style:normal;font-weight:normal">' + 'Error:' + resultData.Message + '</em>');
                                //return true;
                            }

                        },
                        error: function (err) {
                            console.log(err)
                        }
                    });
                }
            });
        }
        else if (layEvent === 'compare') {
            $.post('/Recipe/CompareRecipe', { reciepId: selectdata.RECIPE_ID }, function (res) {

                var icon = res.Result ? 1 : 2;
                layer.msg('<em style="color:black;font-style:normal;font-weight:normal">' + res.Message + '</em>', { icon: icon });

                //if (res.result) {
                //    layer.alert('<em style="color:white;font-style:normal;font-weight:normal">' + res.message + '</em>');
                //} else {
                //    layer.alert('<em style="color:white;font-style:normal;font-weight:normal">' + res.message + '</em>');

                //}
            });
        }
        //else if (layEvent === 'editbody'){

        //    ShowBodyEditPage(selectdata.RECIPE_LATEST_VERSION_ID);
        //}

    });

    window.selectedEQP;
    table.on('toolbar(rcptable)', function (obj) {
        console.log(obj)
        console.log(obj.config.currenteqpid)
        window.selectedEQP = { "eqid": obj.config.currenteqpid };
        if (obj.event == 'add') {
            layer.open({
                title: 'Add New Recipe'
                , type: 2
                , btn: ['确定Confirm', '取消Cancel']
                , content: 'AddRecipe'
                , area: ['60%', '90%']
                //, params: { id: currentversionid }

                , success: function (layero, index) {
                    //向layer页面传值，传值主要代码
                    var body = layer.getChildFrame('body', index);

                }
                , yes: function (index) {
                    var res = window["layui-layer-iframe" + index].callback();
                    //console.log(res);
                    var data = JSON.parse(res);
                    var loadingIndex = layer.load();
                    $.ajax({
                        type: 'post',
                        dataType: 'json',
                        url: '/Recipe/AddNewRecipeToApi',
                        data: {
                            EQID: data.EQID,
                            rcpname: data.rcpname
                        },
                        success: function (data) {
                            setTimeout(function () {
                                layer.close(loadingIndex);
                            }, 10);
                            var resultData = data.replyItem

                            if (resultData.Result) {
                                layer.close(index)
                                window.rcptable.reload();
                                layer.msg('<em style="color:white;font-style:normal;font-weight:normal">' + 'Recipe Added.' + '</em>');
                                //return false;
                            } else {

                                layer.msg('<em style="color:white;font-style:normal;font-weight:normal">' + 'Error:' + resultData.Message + '</em>');
                                //return true;
                            }

                        },
                        error: function (err) {
                            console.log(err)
                        }
                    });
                }, btn2: function (index, layero) {

                    layer.close(index);
                }

            });
        }
        else if (obj.event == 'downloadbylot') {
            layer.prompt({
                formType: 2,

                title: '请扫入Lot ID',
                area: ['300px', '35px'] //自定义文本域宽高
            }, function (value, index, elem) {


                $.ajax({
                    type: 'post',
                    dataType: 'json',
                    url: '/Recipe/DownloadRecipeByLot',
                    data: {
                        eqid: obj.config.currenteqpid,
                        lotid: value

                    },
                    success: function (data) {


                        if (data.Result) {
                            layer.close(index)
                            window.rcptable.reload();
                            //layer.msg('<em style="color:white;font-style:normal;font-weight:normal">' + 'Recipe download succeed.' + '</em>');
                            layer.open({
                                title: '下载成功Download success'
                                , content: "Group:'" + data.RecipeGroupName + "',\r\nRecipe:'" + data.RecipeName + "'"
                            });

                            //return false;
                        } else {
                            layer.open({
                                title: '下载失败Download fail'
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


    var currentversionid;
    window.data;
    function ShowVersionDetail(versionid) {
        currentversionid = versionid;
        window.data = { "id": versionid }

        layer.open({
            title: 'Recipe Version Upgrade'
            , type: 2
            , btn: ['取消Cancel']
            , content: 'AddVersion'
            , area: ['60%', '90%']
            //, params: { id: currentversionid }

            , success: function (layero, index) {
                //向layer页面传值，传值主要代码
                var body = layer.getChildFrame('body', index);
                //将弹窗页面中属性名id="xxxx"的标签赋值
                body.find("[id='rcpversionID']").val(versionid);
            }
            , yes: function (index) {
                layer.close(index);
                //var res = window["layui-layer-iframe" + index].callback();
                //var data = JSON.parse(res);

                //if (data.Name == null) {
                //    layer.msg('请选择一个Recipe');
                //} else {
                //    //AddNewRecipe(data);
                //    //ReloadRecipeTable();
                //    // layer.msg(res);

                //    layer.close(index);
                //}

            }, btn1: function (index, layero) {

                layer.close(index);
            }

        });

    }

    function ShowBodyEditPage(recipeVersionId) {
        layer.open({
            title: 'Edit Recipe Body'
            , type: 2
            , btn: ['提交Submit', '取消Cancel']
            , content: 'EditVersion?RecipeVersionId=' + recipeVersionId
            , area: ['90%', '90%']

            , success: function (layero, index) {

            }
            , yes: function (index) {
                var res = window["layui-layer-iframe" + index].callback();

                var loadingIndex = layer.load();
                $.ajax({
                    type: 'post',
                    contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
                    url: '/Recipe/EditRecipeBody',
                    data: {
                        RecipeVersionId: recipeVersionId,
                        RecipeBody: res.editedBody
                    },
                    success: function (data) {
                        setTimeout(function () {
                            layer.close(loadingIndex);
                        }, 10);
                        var resultData = data

                        if (resultData.Result) {
                            layer.close(index)
                            window.rcptable.reload();
                            layer.msg('<em style="color:white;font-style:normal;font-weight:normal">' + 'Content modified and saved successfully.' + '</em>');
                            //return false;
                        } else {
                            layer.msg('<em style="color:white;font-style:normal;font-weight:normal">' + 'Error:' + resultData.Message + '</em>');
                            //return true;
                        }

                    },
                    error: function (err) {
                        console.log(err)
                    }
                });


            }, btn2: function (index, layero) {

                layer.close(index);
            }

        });

    }

});