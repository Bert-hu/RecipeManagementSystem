layui.use(['jquery','layer', 'table', 'form', 'upload', 'element'], function () {
    var $ = layui.jquery
        , layer = layui.layer
        , table = layui.table
        , form = layui.form
        , upload = layui.upload
        , element = layui.element;

    var eqid = window.parent.selectedEQP.eqid;
    var rcpname;
    var filetable;
    var retdata = {};
    console.log('eqid: '+ eqid)
    loadPage();


    function loadPage() {
        table.render({
            elem: '#eqprecipe'
            , height: 'full-100'
            , url: '/Recipe/GetRecipeFromEQP' //数据接口
            , title: 'Please select the Recipe'
            , id: "recipe"
            , parseData: function (res) {
                if (res.count == 0 && !res.data.Result) {
                    //console.log(res.data.Result)
                    layer.msg('<em style="color:white;font-style:normal;font-weight:normal">' + res.data.Message + '</em>');
                    
                }
               
            }
            , cols: [[ //表头
                { type: 'radio', fixed: 'left', height: 'auto' }
                
                , { field: 'NAME', title: 'Name' }
                //, { field: 'Type', title: 'Type' }
                //, { field: 'Url', title: 'Url', hide: true }

            ]]
            , where: {//这里传参 向后台
                "EQID": eqid
            }
            
        });
        table.on('radio(eqprecipe)', function (obj) { //注：test是table原始容器的属性 lay-filter="对应的值"
            var data = obj.data;
            
            retdata.EQID = obj.data.EQUIPMENT_ID;
            retdata.rcpname = obj.data.NAME;
            console.log(retdata)
            //retdata = data;
            
        });
        
        window.callback = function () {
            //retdata.eqpid = $("#eqpid").val();
            //retdata.alias = $("#alias").val();
            //retdata.remark = $("#remarktext").val();
            return JSON.stringify(retdata);
        };
       

        //表单保存
        layui.$('#saveform').on('click', function () {
            var formdata = form.val('versioninfo');
            $.ajax({
                type: 'post',
                dataType: 'json',
                url: '/Recipe/SaveForm',
                data: formdata,
                success: function (data) {
                    layer.msg('<em style="color:white;font-style:normal;font-weight:normal">' + data.message + '</em>', { icon: data.result ? 1 : 4 });
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
                    console.log(data.status)
                    //console.log('testest')
                    //layer.msg('<em style="color:black;font-style:normal;font-weight:normal">' + data.message + '</em>', { icon: data.result ? 1 : 4 });
                    if (data.status == 'True') {
                        layer.msg('Uploaded Successfully!')
                        filetable.reload();
                    } else {
                        filetable.reload();
                        layer.msg('<em style="color:white;font-style:normal;font-weight:normal">' + 'Error:' + data.status + '</em>');
                    }
                },
                error: function (err) {
                    console.log(err)
                }
            });
        });
        //表单提交
        form.on('submit(versioninfo)', function (data) {

            if (confirm('确定提交？')) {
                var formdata = form.val('versioninfo');
                $.ajax({
                    type: 'post',
                    dataType: 'json',
                    url: '/Recipe/SubmitForm',
                    data: formdata,
                    success: function (data) {
                        layer.msg('<em style="color:white;font-style:normal;font-weight:normal">' + data.message + '</em>', { icon: data.result ? 1 : 4 });
                        //close iframe
                        var index = window.parent.layer.getFrameIndex(window.name)
                        window.parent.layer.close(index);
                        //reload version table
                        
                        window.parent.versiontable.reload();
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
            status = '未提交';
        } else if (index == 100) {
            status = '已完成';
        } else {
            status = ' 待 ' + flows[index] + '签核';
        }
        return status;
    }

});