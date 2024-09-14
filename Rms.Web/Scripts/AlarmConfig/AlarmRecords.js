layui.use(['layer', 'table', 'form', 'upload', 'element'], function () {
    var layer = layui.layer
        , table = layui.table
        , form = layui.form
        , upload = layui.upload
        , element = layui.element;

    var processSel = xmSelect.render({
        el: '#processSelect',
        //initValue: [0],
        radio: true,
        clickClose: true,
        model: {
            icon: 'hidden',
            label: {
                type: 'text'
            }
        },
        tips: '��ѡ��Process',
        block: {
            //�����ʾ����, 0:������
            showCount: 0,
            //�Ƿ���ʾɾ��ͼ��
            showIcon: false,
        },
        on: function (data) {
            //arr:  ��ǰ��ѡ��ѡ�е�����
            var arr = data.arr;
            //change, �˴�ѡ��仯������,����
            var change = data.change;
            //isAdd, �˴β�������������ɾ��
            var isAdd = data.isAdd;
            currentLine = arr[0].value;
            UpdateEquipmentTypeSelect(arr[0].value);
        }
    });

    var equipmentTypeSel = xmSelect.render({
        el: '#equipmentTypeSelect',
        //initValue: [0],
        radio: true,
        clickClose: true,
        model: {
            icon: 'hidden',
            label: {
                type: 'text'
            }
        },
        tips: '��ѡ��EquipmentType',
        block: {
            //�����ʾ����, 0:������
            showCount: 0,
            //�Ƿ���ʾɾ��ͼ��
            showIcon: false,
        },
        on: function (data) {
            //arr:  ��ǰ��ѡ��ѡ�е�����
            var arr = data.arr;
            //change, �˴�ѡ��仯������,����
            var change = data.change;
            //isAdd, �˴β�������������ɾ��
            var isAdd = data.isAdd;
            currentLine = arr[0].value;
            UpdateEquipmentSelect(arr[0].value);
        }
    });
    var equipmentSel = xmSelect.render({
        el: '#equipmentSelect',
        //initValue: [0],
        radio: true,
        clickClose: true,
        model: {
            icon: 'hidden',
            label: {
                type: 'text'
            }
        },
        tips: '��ѡ��Equipment',
        block: {
            //�����ʾ����, 0:������
            showCount: 0,
            //�Ƿ���ʾɾ��ͼ��
            showIcon: false,
        },
        on: function (data) {
            //arr:  ��ǰ��ѡ��ѡ�е�����
            var arr = data.arr;
            //change, �˴�ѡ��仯������,����
            var change = data.change;
            //isAdd, �˴β�������������ɾ��
            var isAdd = data.isAdd;
            currentLine = arr[0].value;
            console.log(arr[0].value);
            ShowAlarmRecordsTable(arr[0].value);
        }
    });

    Initialization();

    async function Initialization() {
        try {
            let result1 = await $.ajax({
                type: 'post',
                dataType: 'json',
                url: '/AlarmConfig/GetProcesses',//'/RecipeMapping/GetLines',
                data: {

                },
                success: function (data) {

                    var seldata = data.map((item, index) => ({
                        name: item,
                        value: item,
                        selected: index === 0
                    }));


                    processSel.update({
                        data: seldata,
                    });

                    UpdateEquipmentTypeSelect(data[0]);
                },
                error: function () {
                }
            });

        } catch (error) {
            //�������
        }
    }
    async function UpdateEquipmentTypeSelect(process) {
        try {
            let result1 = await $.ajax({
                type: 'post',
                dataType: 'json',
                url: '/AlarmConfig/GetEquipmentType',//'/RecipeMapping/GetLines',
                data: {
                    process: process
                },
                success: function (data) {

                    var seldata = data.data.map((item, index) => ({
                        name: item.NAME,
                        value: item.ID,
                        selected: index === 0
                    }));
                    equipmentTypeSel.update({
                        data: seldata,
                    });
                    UpdateEquipmentSelect(data.data[0].ID);
                },
                error: function () {
                }
            });

        } catch (error) {
            //�������
        }
    }

    async function UpdateEquipmentSelect(equipmentType) {
        try {
            let result1 = await $.ajax({
                type: 'post',
                dataType: 'json',
                url: '/AlarmConfig/GetEquipment',//'/RecipeMapping/GetLines',
                data: {
                    equipmentTypeId: equipmentType
                },
                success: function (data) {
                    var seldata = data.data.map((item, index) => ({
                        name: item.ID +'-'+ item.NAME,
                        value: item.ID,
                        selected: index === 0
                    }));
                    equipmentSel.update({
                        data: seldata,
                    });
                    ShowAlarmRecordsTable(data.data[0].ID);
                },
                error: function () { }
            });
        }
        catch (error) {
            //�������
        }
    }

    function ShowAlarmRecordsTable(equipmentId) {
        window.rcptable = table.render({
            elem: '#ALarmRecordsTable'
            , url: '/AlarmConfig/GetAlarmRecords'
            //, toolbar: '#configHeaderToolBar'
            , id: "ALarmRecordsTable"
            , limit: 1000
            , limits: [1000]
            , height: 'full-235'
            , cols: [[
                { field: 'ALTIME', title: 'Time', templet: '<div>{{ FormDate(d.ALTIME, "yyyy-MM-dd HH:mm:ss") }}</div>', width: 180 }
                , { field: 'EQID', title: 'Equipment' }
                , { field: 'ALID', title: 'AlarmCode' }
                , { field: 'ALTX', title: 'Text' }

                // , { field: 'RECIPE_EFFECTIVE_VERSION', title: '��Ч�汾', width: '15%' }
                // { fixed: 'right', width: '15%', align: 'center', toolbar: '#configToolBar' }
            ]]
            , where: {
                equipmentId: equipmentId
            }

            , done: function (data) {

            }
        });

    }








});