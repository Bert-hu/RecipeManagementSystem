layui.use(['layer', 'table', 'form', 'upload', 'element', 'jquery'], function () {
    var layer = layui.layer
        , table = layui.table
        , form = layui.form
        , upload = layui.upload
        , element = layui.element
        , $ = layui.jquery;

    var equipmentSel = xmSelect.render({
        el: '#equipments',
        //initValue: [0],
        radio: true,
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

            console.log(change);
            console.log(arr);
            currentLine = arr[0].value;

        },

    })


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
                        name: item.ID +'---' +item.NAME,
                        value: item.ID,
                        selected: index === 0
                    }));

                    equipmentSel.update({
                        data: seldata,
                    });
                    EquipmentTimer();
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

   

    function LoadEquipmentInfo() {
        $.ajax({
            url: '/CommonProduction/GetEquipmentInfo',//控制器活动,返回一个分部视图,并且给分部视图传递数据.
            data: {
                equipmentid: equipmentSel.getValue()[0].value
            },//传给服务器的数据(即后台AddUsers()方法的参数,参数类型要一致才可以)
            type: 'POST',
            contentType: 'application/x-www-form-urlencoded; charset=UTF-8',//数据类型必须有
            async: false,
            success: function (data) {
                form.val('eqinfoform', data);
            },
            error: function (message) {
                alert('error!');
            }
        });
    }
});