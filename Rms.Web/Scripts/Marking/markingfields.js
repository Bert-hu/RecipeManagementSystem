
layui.use(['layer', 'table', 'form', 'upload'], function () {
    var layer = layui.layer
        , table = layui.table
        , form = layui.form;

    var itemstable = table.render({
        elem: '#markingfields'
        , url: '/MarkingManage/GetMarkingFields'
        , page: true
        , id: "markingfields"
        , limit: 1000
        , limits: [1000]
        , height: 'full-195'
        , defaultToolbar: ['filter', 'print', 'exports']
        , cols: [[
            { type: 'checkbox', fixed: 'left' }
            , { field: 'NAME', title: 'Name' }
            , { field: 'TYPE', title: 'Type' }
            , { field: 'REMARK', title: 'Remark' }
            , { field: 'SAMPLE', title: 'Sample' }
            //, { field: 'CREATOR', title: '修改者' }
            //, { field: 'CREATE_TIME', title: '修改时间', templet: '<div>{{ FormDate(d.CREATE_TIME, "yyyy-MM-dd HH:mm:ss") }}</div>' }

        ]]
        , done: function (res, curr, count) {

        }
    });
});