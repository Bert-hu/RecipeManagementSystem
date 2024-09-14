layui.use(['jquery', 'layer', 'table', 'form', 'upload', 'element', 'code'], function () {
    var $ = layui.jquery
        , layer = layui.layer
        , table = layui.table
        , form = layui.form
        ;
    //, upload = layui.upload
    //, element = layui.element;

    layui.code({
        encode: true //是否转义html标签。默认不开启
    });

    var vid = window.parent.data.id;
    var rcpname;
    var filetable;
    console.log(window.parent.versiontable)
    loadPage();
    function loadPage() {
        $.ajax({
            url: '/Recipe/GetVersionInfo',
            data: {
                versionid: vid,
            },
            async: false,
            type: 'GET',
            contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
            success: function (data) {
                //console.log(data);
                rcpname = data.projectinfo.NAME;
                var status = GetVersionStatus(data.versioninfo._FLOW_ROLES, data.versioninfo.CURRENT_FLOW_INDEX);
                document.getElementById("info").innerHTML = 'Recipe: ' + data.projectinfo.NAME + " " + data.versioninfo.VERSION + ' ' + status;
                var fff = document.getElementById("versioninfo");
                fff.style.display = "block";
                form.val('versioninfo', {
                    "ID": data.versioninfo.ID,
                    "NAME": data.versioninfo.NAME,
                    "REMARK": data.versioninfo.REMARK // "name": "value"

                });

                var $form = $('#versioninfo'); // 使用选择器替换 formId 为表单区域的 id
                //状态>-1(-1为未提交状态)则锁定表单
                if (data.versioninfo.CURRENT_FLOW_INDEX > -1) {
                    // 遍历表单区域中的每个元素
                    $form.find('input, textarea, select').each(function () {
                        // 设置 readonly 属性为 true
                        $(this).attr('readonly', true);
                    });

                } else {

                    $form.find('input, textarea, select').each(function () {
                        $(this).attr('readonly', false);
                    });
                }
                // 重新渲染 layui 表单
                layui.form.render();

                //var uploadInst = upload.render({
                //    elem: '#test1'
                //    , accept: 'file'
                //    , url: 'UploadRcpFromEQP' //此处用的是第三方的 http 请求演示，实际使用时改成您自己的上传接口即可。
                //    , data: {
                //        versionid: vid
                //        //rcpname: data.projectinfo.NAME
                //    }
                //    , before: function (obj) {
                //        //预读本地文件示例，不支持ie8
                //        //obj.preview(function (index, file, result) {
                //        //    $('#demo1').attr('src', result); //图片链接（base64）
                //        //});

                //        element.progress('demo', '0%'); //进度条复位
                //        window.parent.layer.msg('上传中', { icon: 16, time: 0 });
                //    }
                //    , done: function (res) {
                //        window.parent.layer.msg('<em style="color:white;font-style:normal;font-weight:normal">' + res.message + '</em>', { icon: res.result ? 1 : 4 });

                //        $('#demoText').html(''); //置空上传失败的状态
                //    }
                //    , error: function () {
                //        //演示失败状态，并实现重传
                //        var demoText = $('#demoText');
                //        demoText.html('<span style="color: #FF5722;">上传失败</span> <a class="layui-btn layui-btn-xs demo-reload">重试</a>');
                //        demoText.find('.demo-reload').on('click', function () {
                //            uploadInst.upload();
                //        });
                //    }
                //    //进度条
                //    , progress: function (n, elem, e) {
                //        element.progress('demo', n + '%'); //可配合 layui 进度条元素使用
                //        if (n == 100) {
                //            //layer.msg('上传完毕', { icon: 1 });
                //            filetable.reload();
                //        }
                //    }
                //});

                filetable = table.render({
                    elem: '#filetable'
                    , url: '/Recipe/GetFileTable'
                    //, toolbar: true
                    , id: "filetable"
                    , limit: 1000
                    , limits: [1000]
                    , height: '83'
                    , cols: [[
                        { field: 'NAME', title: 'Recipe Name' }
                        , { field: 'CREATOR', title: '上传者', width: 120 }
                        , { field: 'CREATE_TIME', title: '上传时间', templet: '<div>{{ FormDate(d.CREATE_TIME, "yyyy-MM-dd HH:mm:ss") }}</div>', width: 180 }
                        , { fixed: 'right', title: '操作', width: 200, align: 'center', toolbar: '#filetoolbar' }

                    ]]
                    , where: {
                        versionid: vid
                    }
                    , currentversionid: vid

                });

                //表中工具条
                table.on('tool(filetable)', function (obj) { //注：tool 是工具条事件名，recipe 是 table 原始容器的属性 lay-filter="对应的值"
                    var selectdata = obj.data //获得当前行数据
                        , layEvent = obj.event; //获得 lay-event 对应的值
                    if (layEvent === 'download') {
                        //DownladFile(selectdata.ID);
                    } else if (layEvent === 'delete') {
                        if (confirm('确定删除？Confirm to delete')) {
                            //console.log(selectdata.ID)
                            DeleteFile(selectdata.ID);
                        }
                    }

                });


                function DownladFile(fileid) {
                    var url = "/Recipe/DownladFile?fileid=" + fileid;
                    window.open(url, "_blank");
                }
                function DeleteFile(fileid) {
                    $.ajax({
                        type: 'post',
                        dataType: 'json',
                        url: '/Recipe/DeleteFile',
                        data: {
                            fileid: fileid,
                        },
                        success: function (data) {

                            window.parent.layer.msg('<em style="color:black;font-style:normal;font-weight:normal">' + data.message + '</em>', { icon: data.result ? 1 : 4 });
                            filetable.reload();
                        },
                        error: function () {
                        }
                    });
                }
            },

        });

        $.ajax({
            url: '/Recipe/GetVersionSml',//控制器活动,返回一个分部视图,并且给分部视图传递数据.
            data: {
                RecipeVersionId: vid
            },//传给服务器的数据(即后台AddUsers()方法的参数,参数类型要一致才可以)
            type: 'POST',
            contentType: 'application/x-www-form-urlencoded; charset=UTF-8',//数据类型必须有
            async: false,
            success: function (data) {
                if (data.Result) {
                    var convertedText = data.BodySml.replace(/</g, '&lt;').replace(/>/g, '&gt;');

                    document.getElementById("textarea_recipebody").innerHTML = convertedText;
                    layui.code({
                        //encode: true //是否转义html标签。默认不开启
                    });

                } else {
                    document.getElementById("textarea_recipebody").innerHTML = data.Message;
                }


            },
            error: function (message) {
                alert('error!');
            }
        });

        table.render({
            elem: '#processrecordtable'
            , url: '/Recipe/GetProcessRecord'
            //, toolbar: true
            , id: "processrecordtable"
            , limit: 1000
            , limits: [1000]
            , height: '500'
            , cols: [[
                { field: 'FLOW_INDEX', title: '流程序号Flow No.' }
                , {
                    field: 'ACTION', title: '结果',
                    templet: function (d, s) {
                        var strstate;
                        if (d.ACTION == 0) {
                            strstate = '提交Submit'
                        } else if (d.ACTION == 1) {
                            strstate = '同意Agree'
                        }
                        else {
                            strstate = '否决Reject'
                        }
                        return strstate;
                    }
                }
                , { field: 'REMARK', title: 'Remark' }
                , { field: 'CREATOR', title: 'User', width: 120 }
                , { field: 'CREATE_TIME', title: 'Date Time', templet: '<div>{{ FormDate(d.CREATE_TIME, "yyyy-MM-dd HH:mm:ss") }}</div>', width: 180 }


            ]]
            , where: {
                versionid: vid
            }


        });

        //表单保存
        layui.$('#saveform').on('click', function () {
            var formdata = form.val('versioninfo');
            $.ajax({
                type: 'post',
                dataType: 'json',
                url: '/Recipe/SaveForm',
                data: formdata,
                success: function (data) {
                    window.parent.layer.msg('<em style="color:black;font-style:normal;font-weight:normal">' + data.message + '</em>', { icon: data.result ? 1 : 4 });
                    var index = window.parent.layer.getFrameIndex(window.name)
                    window.parent.layer.close(index);
                    window.parent.versiontable.reload();

                },
                error: function () {
                }
            });
        });


        //load recipe body
        layui.$('#loadbody').on('click', function () {

            $.ajax({
                type: 'post',
                dataType: 'json',
                url: '/Recipe/UploadRcpFromEQP',
                data: {
                    versionid: vid,
                    rcpname: rcpname
                },
                success: function (data) {
                    console.log(data)
                    //console.log('testest')
                    //layer.msg('<em style="color:black;font-style:normal;font-weight:normal">' + data.message + '</em>', { icon: data.result ? 1 : 4 });
                    if (data.res.Result) {
                        layer.msg('Uploaded Successfully!')
                        filetable.reload();
                    } else {
                        filetable.reload();
                        layer.msg('<em style="color:white;font-style:normal;font-weight:normal">' + 'Error:' + data.res.Message + '</em>');
                    }
                },
                error: function (err) {
                    console.log(err)
                }
            });
        });

        layui.$('#loadbodyfromeffectiveversion').on('click', function () {

            $.ajax({
                type: 'post',
                dataType: 'json',
                url: '/Recipe/UploadRcpFromEffectiveVersion',
                data: {
                    versionid: vid,
                    rcpname: rcpname
                },
                success: function (data) {
                    console.log(data)
                    //console.log('testest')
                    //layer.msg('<em style="color:black;font-style:normal;font-weight:normal">' + data.message + '</em>', { icon: data.result ? 1 : 4 });
                    if (data.res.Result) {
                        layer.msg('Uploaded Successfully!')
                        filetable.reload();
                    } else {
                        filetable.reload();
                        layer.msg('<em style="color:white;font-style:normal;font-weight:normal">' + 'Error:' + data.res.Message + '</em>');
                    }
                },
                error: function (err) {
                    console.log(err)
                }
            });
        });
        //表单提交
        form.on('submit(versioninfo)', function (data) {

            if (confirm('确定提交？Confirm to submit?')) {
                var formdata = form.val('versioninfo');
                $.ajax({
                    type: 'post',
                    dataType: 'json',
                    url: '/Recipe/SubmitForm',
                    data: formdata,
                    success: function (data) {
                        console.log(data)
                        if (data.result) {
                            var index = window.parent.layer.getFrameIndex(window.name)
                            window.parent.layer.close(index);
                            //reload version table
                            window.parent.rcptable.reload({
                                where: {
                                    EQID: window.parent.currenteqid
                                }
                            });
                            window.parent.versiontable.reload();
                        } else {
                            window.parent.layer.msg('<em style="color:black;font-style:normal;font-weight:normal">' + data.message + '</em>', { icon: data.result ? 1 : 4 });
                        }

                        //close iframe

                    },
                    error: function () {
                    }
                });
            }
            return false;
        });
    }


    function GetVersionStatus(flows, index) {
        var status = '';
        if (index == -1) {
            status = 'Unsubmitted';
        } else if (index == 100) {
            status = 'Completed';
        } else {
            status = 'Wait ' + flows[index] ;
        }
        return status;
    }

});