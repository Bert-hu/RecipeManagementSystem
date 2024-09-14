layui.use(['layer', 'table', 'form', 'upload', 'element', 'jquery', 'flow'], function () {
    var layer = layui.layer
        , table = layui.table
        , form = layui.form
        , upload = layui.upload
        , element = layui.element
        , $ = layui.jquery
        , flow = layui.flow;

  
    var processselect = document.getElementById("process");
    var equipmentSel = xmSelect.render({
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

            SelectEquipment(arr[0].value);
            countDown = 0;
        },
    })
    EquipmentTimer();
    function SelectEquipment(eqid) {

        var loadingIndex = layer.load();


        CleanLogField();
        
        setTimeout(function () {
            loadLogs(eqid);
            LoadEquipmentInfo(eqid);
            layer.close(loadingIndex);
        }, 10);
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


                    equipmentSel.update({
                        data: seldata,

                    });
                    equipmentSel.setValue([
                        seldata[0]
                    ])

                    SelectEquipment(seldata[0].value);
                },
                error: function () {
                }
            });


        } catch (error) {
            //处理错误
        }
    }



    var countDown = 0; // 倒计时时间，单位：秒
   
    function EquipmentTimer() {
        element.progress('reloadeqinfobar', '100%');

        countDown = 0;
        // 定时器
        var timer;
        var percent = 0;
        var interval = 1000; // 定时器间隔，单位：毫秒

        timer = setInterval(function () {

            if (countDown <= 0) {
                //clearInterval(timer);
                element.progress('reloadeqinfobar', '100%');
                // 倒计时结束后执行你的函数
                LoadEquipmentInfo();
                countDown = 30;
            } else {
                countDown--;
                percent = (countDown / 30) * 100;
                element.progress('reloadeqinfobar', percent + '%');
            }
        }, interval);
    }



    function LoadEquipmentInfo(eqid) {

        console.log(eqid);
        if (eqid === undefined) {
            eqid = equipmentSel.getValue()[0].value;
            console.log(eqid);
        }
        $.ajax({
            url: '/CommonProduction/GetEquipmentInfo',//控制器活动,返回一个分部视图,并且给分部视图传递数据.
            data: {
                equipmentid: eqid
            },//传给服务器的数据(即后台AddUsers()方法的参数,参数类型要一致才可以)
            type: 'POST',
            contentType: 'application/x-www-form-urlencoded; charset=UTF-8',//数据类型必须有
            async: false,
            success: function (data) {
                form.val('eqinfoform', data);
            },
            error: function (message) {
            }
        });
    }


    var logContainer = document.getElementById('logContainer');
    var logCardContainer = document.getElementById('logCardContainer');
    var currentlogid;

    function CleanLogField() {
        currentlogid = null;
        while (logContainer.firstChild) {
            logContainer.removeChild(logContainer.firstChild);
        }
    }

    function loadLogs(eqid) {
        if (eqid === undefined) eqid = equipmentSel.getValue()[0].value;

        $.ajax({
            url: '/CommonProduction/GetNewLog', // 替换成您的日志API的实际URL
            type: 'POST',
            data: {
                equipmentid: eqid,
                logid: currentlogid
            },
            success: function (data) {
                if (data.length > 0) {
                    currentlogid = data[data.length - 1].ID;
                    // 遍历数组并逐个将每个元素附加到日志容器中
                    data.forEach(logEntry => {
                        const logDiv = document.createElement('div');
                        logDiv.textContent = logEntry.STRCREATE_TIME + " " + logEntry.ACTION + " " + logEntry.RESULT + " " + logEntry.MESSAGE;
                        if (logEntry.RESULT.toUpperCase() != 'TRUE') {
                            logDiv.style.color = 'red';
                        }
                        logContainer.appendChild(logDiv);
                    });

                    // 滚动到底部以显示最新内容
                    logCardContainer.scrollTop = logContainer.scrollHeight;
                }

            },
            error: function (error) {
                console.error('加载日志失败: ' + error);
            }
        });
    }
    // 定时加载新日志
    setInterval(loadLogs, 10000);




    $("#downloadrecipebysnbtn").click(function () {
        layer.prompt({
            formType: 2,

            title: '请扫入Panel ID',
            area: ['300px', '35px'] //自定义文本域宽高
        }, function (value, index, elem) {
            layer.close(index);
            var loadingIndex = layer.load();
            $.ajax({
                type: 'post',
                dataType: 'json',
                url: '/Common/DownloadRecipeByPanelID',
                data: {
                    equipmentid: equipmentSel.getValue()[0].value,
                    panelid: value
                },
                success: function (data) {
                    setTimeout(function () {
                        loadLogs();
                        LoadEquipmentInfo();
                        layer.close(loadingIndex);
                    }, 10);
                    if (data.Result) {
                        //layer.msg('<em style="color:white;font-style:normal;font-weight:normal">' + 'Recipe download succeed.' + '</em>');
                        layer.open({
                            title: '下载成功'
                            , content: "Group:'" + data.RecipeGroupName + "',\r\nRecipe:'" + data.RecipeName + "'"
                        });
                        WaitAndRefresh();
                    } else {
                        layer.close(loadingIndex);
                        layer.open({
                            title: '下载失败'
                            , content: data.Message
                        });
                    }

                },
                error: function (err) {
                    console.log(err)
                }
            });

        });
    });
    
    $("#switchrecipebysnbtn").click(function () {
        layer.prompt({
            formType: 2,

            title: '请扫入Panel ID',
            area: ['300px', '35px'] //自定义文本域宽高
        }, function (value, index, elem) {
            layer.close(index);
            var loadingIndex = layer.load();
            $.ajax({
                type: 'post',
                dataType: 'json',
                url: '/Common/SwitchRecipeByPanelID',
                data: {
                    equipmentid: equipmentSel.getValue()[0].value,
                    panelid: value
                },
                success: function (data) {
                    setTimeout(function () {
                        loadLogs();
                        LoadEquipmentInfo();
                        layer.close(loadingIndex);
                    }, 10);
                    if (data.Result) {
                        //layer.msg('<em style="color:white;font-style:normal;font-weight:normal">' + 'Recipe download succeed.' + '</em>');
                        layer.open({
                            title: '下载成功'
                            , content: "Group:'" + data.RecipeGroupName + "',\r\nRecipe:'" + data.RecipeName + "'"
                        });
                        WaitAndRefresh();
                    } else {
                        layer.close(loadingIndex);
                        layer.open({
                            title: '下载失败'
                            , content: data.Message
                        });
                    }

                },
                error: function (err) {
                    console.log(err)
                }
            });

        });
    });

});