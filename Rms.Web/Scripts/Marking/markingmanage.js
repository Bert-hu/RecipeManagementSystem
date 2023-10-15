
layui.use(['layer', 'table', 'form', 'upload'], function () {
    var layer = layui.layer
        , table = layui.table
        , form = layui.form;

    var markingrecipe = table.render({
        elem: '#markingrecipe'
        , url: '/MarkingManage/GetMarkingRecipe'
        , page: true
        , id: "markingrecipe"
        , limit: 1000
        , limits: [1000]
        , height: 'full-150'
        , defaultToolbar: []
        , toolbar: '#toolbar'
        , cols: [[

            { field: 'EQID', title: 'EQID', width: 100 }
            , { field: 'EQNAME', title: '设备名', width: 100 }
            , { field: 'RECIPENAME', title: '机种名', width: 300 }
            , { field: 'EFFECTIVE_VERSION', title: '生效版', width: 80 }
            , { field: 'LATEST_VERSION', title: '最新版', width: 80 }

        ]]
        , done: function (res, curr, count) {
            $.ajax({
                type: 'post',
                dataType: 'json',
                url: '/MarkingManage/GetMarkingFields',
                data: {
                    page: 1,
                    limit: 9999
                },
                success: function (data) {
                    $('#field').html('');
                    data.data.forEach(function (value, index, array) {
                        $('#field').append(new Option(value.NAME, value.NAME));

                    });
                },
                error: function (err) {
                    console.log(err)
                }
            });
        }
    });

    var markingitem = table.render({
        elem: '#markingitem'
        //, url: '/MarkingManage/GetMarkingItems'

        // , page: true
        , id: "markingitem"
        , limit: 1000
        , limits: [1000]
        , height: '400'
        , defaultToolbar: []
        //, toolbar: '#toolbar'
        , cols: [[

            { field: 'TEXTINDEX', title: 'Text', width: 70 }
            , { field: 'TEXTORDER', title: 'Order', width: 70 }
            , { field: 'CONTENT', title: 'Content', width: 200 }
            , { field: 'TYPE', title: 'Type', width: 80 }
            , { field: 'START_INDEX', title: 'Start', width: 70 }
            , { field: 'LENGTH', title: 'Length', width: 80 }
            , { fixed: 'right', title: '', width: 50, align: 'center', toolbar: '#configtoolbar' }

        ]]
        , done: function (res, curr, count) {

        }
    });
    table.on('row(markingrecipe)', function (obj) {

        var data = obj.data;//data为当前点击行的数据
        //console.log(obj);
        UpdateSubpage(data.RECIPEID);




        //标注选中样式
        obj.tr.addClass('selected').siblings().removeClass('selected');
    });



    table.on('toolbar(markingrecipe)', function (obj) {
        var id = obj.config.id;
        switch (obj.event) {
            case 'search':
                var filter = $("#searchfield").val();

                table.reload('markingrecipe', {
                    url: '/MarkingManage/GetMarkingRecipe',
                    where: {
                        filter: filter,
                    }
                });
                $("#searchfield").val(filter);
                break;
        }
    });

    table.on('tool(markingitem)', function (obj) { //注：tool 是工具条事件名，recipe 是 table 原始容器的属性 lay-filter="对应的值"
        var selectdata = obj.data //获得当前行数据
            , layEvent = obj.event; //获得 lay-event 对应的值
        if (layEvent === 'delete') {
            layer.confirm('确认删除？', {

                btn: ['确定', '取消']
                , yes: function (index, layero) {
                    DeleteConfigItem(selectdata);
                    layer.close(index);
                }
            },);
        }

    });

    form.on('radio(marktype)', function (data) {
        if (data.value === 'Code') {
            // 选到Code时移除disabled属性
            $('#field').attr("disabled", false).removeClass("layui-btn-disabled");
            $('#startindex').attr("disabled", false).removeClass("layui-btn-disabled");
            $('#length').attr("disabled", false).removeClass("layui-btn-disabled");
            $('#fixedtext').attr("disabled", true).addClass("layui-btn-disabled");
        } else {
            // 选到Fixed时添加disabled属性
            $('#field').attr("disabled", true).addClass("layui-btn-disabled");;
            $('#startindex').attr("disabled", true).addClass("layui-btn-disabled");
            $('#length').attr("disabled", true).addClass("layui-btn-disabled");
            $('#fixedtext').attr("disabled", false).removeClass("layui-btn-disabled");

        }
        form.render();
    });

    //版本选择
    form.on('select(version)', function (data) {
        ReloadItemTable(data.value);

    });

    //监听提交
    form.on('submit(additem)', function (data) {
        //layer.alert(JSON.stringify(data.field), {
        //    title: '最终的提交信息'
        //})
        $.ajax({
            type: 'post',
            dataType: 'json',
            url: '/MarkingManage/AddMarkingConfig',
            data: {
                data: data.field
            },
            success: function (res) {

                layer.msg('<em style="color:white;font-style:normal;font-weight:normal">' + res.message + '</em>');
                if (res.result) {
                    ReloadItemTable(data.field.version);

                }
            },
            error: function (err) {
                console.log(err)
            }
        });
        return false;//不刷新
    });
    //提交验证
    form.verify({
        fixedtext: function (value, item) {
            // 获取 Type 的值
            var marktype = $('input[name="marktype"]:checked').val();
            // 如果 Type 为 Fixed，则验证 Fixed Text 是否为空
            if (marktype === 'Fixed') {
                if (!value) {
                    return 'Fixed Text 不能为空';
                }
            }
        }
    });

    layui.$('#addversion').on('click', function () {
        var recipeid = $("#recipeid").val();
        if (!!recipeid) {
            layer.confirm('确认新增？', {

                btn: ['确定', '取消']
                , yes: function (index, layero) {
                    //TODO: Add new version
                    AddMarkingVersion(recipeid);
                    layer.close(index);
                }
            },);

        } else {
            layer.msg('<em style="color:white;font-style:normal;font-weight:normal">' + '请选择机种' + '</em>');
        }

        return false;//不刷新
    });

    layui.$('#submitversion').on('click', function () {

        var versionid = $('#version').val();
        console.log(versionid);
        if (!!versionid) {
            layer.confirm('确认提交？', {

                btn: ['确定', '取消']
                , yes: function (index, layero) {
                    SubmitVersion(versionid);

                    layer.close(index);
                }
            },);

        } else {
            layer.msg('<em style="color:white;font-style:normal;font-weight:normal">' + '请选择版本' + '</em>');
        }

        return false;//不刷新
    });


    function UpdateSubpage(recipeid) {
        //
        $.ajax({
            type: 'post',
            dataType: 'json',
            url: '/MarkingManage/GetSubpageData',
            data: {
                recipeid: recipeid
            },
            success: function (res) {
                console.log(res);
                $("#lg_recipeinfo").html(res.eqp.ID + ' ' + res.eqp.NAME + ' ' + res.recipe.NAME);
                $("#recipeid").val(res.recipe.ID);//更新当前记录的RECIPEID
                $('#version').html('');
                res.markingversions.forEach(function (value, index, array) {
                    $('#version').append(new Option(value.VERSION, value.ID));
                });
                if (res.markingversions.length > 0) {
                    ReloadItemTable(res.markingversions[0].ID);
                }

                form.render('select');
            },
            error: function (err) {
                console.log(err)
            }
        });

    }
    function AddMarkingVersion(recipeid) {
        $.ajax({
            type: 'post',
            dataType: 'json',
            url: '/MarkingManage/AddMarkingVersion',
            data: {
                recipeid: recipeid
            },
            success: function (data) {
                layer.msg('<em style="color:white;font-style:normal;font-weight:normal">' + data.message + '</em>');
                if (data.result) {
                    //ReloadItemTable(markingversion);
                    UpdateSubpage(recipeid);
                }
            },
            error: function (err) {
                console.log(err)
            }
        });

    }

    function SubmitVersion(versionid) {
        $.ajax({
            type: 'post',
            dataType: 'json',
            url: '/MarkingManage/SubmitVersion',
            data: {
                markingversionid: versionid
            },
            success: function (data) {
                layer.msg('<em style="color:white;font-style:normal;font-weight:normal">' + data.message + '</em>');
                if (data.result) {

                    UpdateSubpage(recipeid);
                }
            },
            error: function (err) {
                console.log(err)
            }
        });

    }
    function ReloadItemTable(markingversionid) {
        table.reload('markingitem', {
            url: '/MarkingManage/GetMarkingConfigs'
            , where: {
                markingversionid: markingversionid
            }, done: function (res, curr, count) {
                ReloadPreviewDie(markingversionid);
            }
            //,height: 300
        });
    }
    function ReloadPreviewDie(markingversionid) {
        $.ajax({
            type: 'post',
            dataType: 'json',
            url: '/MarkingManage/GetMarkingTexts',
            data: {
                markingversionid: markingversionid
            },
            success: function (data) {
                //layer.msg('<em style="color:white;font-style:normal;font-weight:normal">' + data.message + '</em>');
                console.log(data);
                if (data.result) {
                    const jsonData = data.markingtexts;
                    const sampleDiv = document.querySelector('.sample');
                    sampleDiv.innerHTML = '';
                    const keys = Object.keys(jsonData);
                    const numOfLines = keys.length > 4 ? keys.length : 4;
                    const baseFontSize = 1.5;
                    const maxHeightRem = 17;

                    const lineHeightPercent = (100 / numOfLines).toFixed(2);
                    sampleDiv.style.height = `${maxHeightRem}rem`;

                    keys.forEach((key, index) => {
                        const lineContent = jsonData[key];

                        const fontSize = `${baseFontSize * (10 / numOfLines)}rem`;
                        const lineDiv = document.createElement('div');
                        lineDiv.textContent = lineContent;
                        lineDiv.style.fontSize = fontSize;

                        sampleDiv.appendChild(lineDiv);
                    });
                }
            },
            error: function (err) {
                console.log(err)
            }
        });
    }
    function DeleteConfigItem(config) {
        $.ajax({
            type: 'post',
            dataType: 'json',
            url: '/MarkingManage/DeleteMarkingConfig',
            data: {
                configid: config.ID
            },
            success: function (res) {

                layer.msg('<em style="color:white;font-style:normal;font-weight:normal">' + res.message + '</em>');
                if (res.result) {
                    ReloadItemTable(config.MARKING_VERSION_ID);

                }
            },
            error: function (err) {
                console.log(err)
            }
        });
    }
});