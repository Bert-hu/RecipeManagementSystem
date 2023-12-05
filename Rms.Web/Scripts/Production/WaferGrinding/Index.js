layui.use(['layer', 'table', 'form', 'upload', 'element', 'jquery', 'flow'], function () {
    var layer = layui.layer
        , table = layui.table
        , form = layui.form
        , upload = layui.upload
        , element = layui.element
        , $ = layui.jquery
        , flow = layui.flow;

    var equipmentSel = xmSelect.render({
        el: '#equipments',
        //initValue: [0],
        radio: true,
        clickClose: true,
        model: {
            icon: 'hidden',
            label: {
                type: 'text'
            }
        },
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

            var loadingIndex = layer.load();

            console.log(change);
            console.log(arr);
            console.log(equipmentSel.getValue()[0].value);
            CleanLogField();

            setTimeout(function () {
                loadLogs(arr[0].value);
                LoadEquipmentInfo(arr[0].value);
                layer.close(loadingIndex);
            }, 10);
        },

    })

    flow.load({
        elem: '#historymsg' //流加载容器
        , scrollElem: '#historymsg' //滚动条所在元素，一般不用填，此处只是演示需要。
        , done: function (page, next) { //执行下一页的回调

            //模拟数据插入
            setTimeout(function () {
                var lis = [];
                for (var i = 0; i < 30; i++) {
                    lis.push('<li>' + ((page - 1) * 30 + i + 1) + '</li>')
                }

                //执行下一页渲染，第二参数为：满足“加载更多”的条件，即后面仍有分页
                //pages为Ajax返回的总页数，只有当前页小于总页数的情况下，才会继续出现加载更多
                next(lis.join(''), page < 10); //假设总页数为 10
            }, 1000);
        }
    });

    Initialization();

    async function Initialization() {
        try {
            let result1 = await $.ajax({
                type: 'post',
                dataType: 'json',
                url: '/WaferGrinding/GetEquipments',
                data: {

                },
                success: function (data) {
                    var seldata = data.map((item, index) => ({
                        name: item.ID + '---' + item.NAME,
                        value: item.ID,
                        selected: index === 0
                    }));

                    equipmentSel.update({
                        data: seldata,
                    });
                    EquipmentTimer();
                    // 初始加载日志
                    loadLogs();
                },
                error: function () {
                }
            });


        } catch (error) {
            //处理错误
        }
    }

    function EquipmentTimer() {
        element.progress('reloadeqinfobar', '100%');

        var countDown = 0; // 倒计时时间，单位：秒
        var timer; // 定时器
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




    $("#downloadbylotbtn").click(function () {
        layer.prompt({
            formType: 2,

            title: '请扫入Lot ID',
            area: ['300px', '35px'] //自定义文本域宽高
        }, function (value, index, elem) {
            layer.close(index);
            var loadingIndex = layer.load();
            $.ajax({
                type: 'post',
                dataType: 'json',
                url: '/CommonProduction/DownloadRecipeByLot',
                data: {
                    eqid: equipmentSel.getValue()[0].value,
                    lotid: value
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
    $("#portalotstart").click(function () {
        PortLotStart("A");
    });

    $("#portblotstart").click(function () {
        PortLotStart("B");
    });

    function PortLotStart(port) {
        layer.prompt({
            formType: 2,

            title: '请扫入Lot ID',
            area: ['300px', '35px'] //自定义文本域宽高
        }, function (value, index, elem) {
            layer.close(index);
            var loadingIndex = layer.load();
            $.ajax({
                type: 'post',
                dataType: 'json',
                url: '/WaferGrinding/PortLotStart',
                data: {
                    equipmentid: equipmentSel.getValue()[0].value,
                    lotid: value,
                    port: port
                },
                success: function (data) {
                    setTimeout(function () {
                        loadLogs();
                        LoadEquipmentInfo();
                        layer.close(loadingIndex);
                    }, 10);
                    if (data.Result) {
                        layer.open({
                            title: '设备启动成功'
                            , content: data.Message
                        });

                    } else {
                        layer.open({
                            title: '失败'
                            , content: data.Message
                        });
                    }

                },
                error: function (err) {
                    layer.close(loadingIndex);
                    console.log(err)
                }
            });

        });
    }

    $("#lotend").click(function () {
        layer.prompt({
            formType: 2,

            title: 'Lot End:请扫入Lot ID',
            area: ['300px', '35px'] //自定义文本域宽高
        }, function (value, index, elem) {
            layer.close(index);
            var loadingIndex = layer.load();
            $.ajax({
                type: 'post',
                dataType: 'json',
                url: '/WaferGrinding/LotEnd',
                data: {
                    equipmentid: equipmentSel.getValue()[0].value,
                    lotid: value,
                },
                success: function (data) {
                    setTimeout(function () {
                        loadLogs();
                        LoadEquipmentInfo();
                        layer.close(loadingIndex);
                    }, 10);
                    if (data.Result) {
                        layer.open({
                            title: '设备启动成功'
                            , content: data.Message
                        });

                    } else {
                        layer.open({
                            title: '失败'
                            , content: data.Message
                        });
                    }
                },
                error: function (err) {
                    layer.close(loadingIndex);
                    console.log(err)
                }
            });

        });
    });

});