window.currentProcess;

layui.use(['layer', 'table', 'form', 'upload', 'element'], function () {
    var layer = layui.layer
        , table = layui.table
        , form = layui.form
        , upload = layui.upload
        , element = layui.element;
    var processselect = document.getElementById("process");

    getProcess();

    form.on('select(process)', function (data) {
        var selectedValue = data.value; // Get the selected value
        ShowEquipmentTable(selectedValue);
    });

    form.on('input-affix(search)', function (data) {
        var elem = data.elem; // Input box
        var value = elem.value; // Value of the input box
        if (!value) {
            layer.msg('Please enter search content');
            return elem.focus();
        };
        // Simulate search jump
        table.reload('equipmenttable', {
            where: {
                searchText: value
            } // Search field
        });
    });

    $('#searchInput').on('keydown', function (event) {
        if (event.key === 'Enter') {
            // Prevent default behavior (e.g., form submission)
            event.preventDefault();
            // Get the value from the input box
            const searchValue = $(this).val();
            // Execute your search logic here
            table.reload('equipmenttable', {
                where: {
                    searchText: searchValue
                } // Search field
            });
        }
    });

    async function getProcess() {
        try {
            let result1 = await $.ajax({
                type: 'post',
                dataType: 'json',
                url: '/Equipment/GetProcesses',
                data: {},
                success: function (data) {
                    // Clear the processselect element options
                    processselect.innerHTML = "";
                    // Add default option
                    var defaultOption = document.createElement("option");
                    defaultOption.value = "";
                    defaultOption.text = "Please select a process";
                    processselect.add(defaultOption);
                    // Automatically generate other options
                    for (var i = 0; i < data.length; i++) {
                        var option = document.createElement("option");
                        option.text = data[i];
                        processselect.add(option);
                    }
                    form.render('select');

                    ShowEquipmentTable("");
                },
                error: function () {
                }
            });
        } catch (error) {
            // Handle error
        }
    }

    function ShowEquipmentTable(processfilter) {
        window.equipmenttable = table.render({
            elem: '#equipmenttable'
            , url: '/Equipment/GetEQPs'
            , toolbar: '#eqpToolbar'
            , id: "equipmenttable"
            , page: true
            , limit: 1000
            , limits: [1000]
            , height: 'full-235'
            , cols: [[
                { field: 'TYPEPROCESS', title: 'Process' }
                , { field: 'TYPENAME', title: 'Type' }
                , { field: 'TYPETYPE', title: 'Model' }
                , { field: 'TYPEVENDOR', title: 'Vendor' }
                , { field: 'ID', title: 'Equipment ID' }
                , { field: 'NAME', title: 'Name', edit: true }
                , { field: 'RECIPE_TYPE', title: 'Recipe Type' }
                , { fixed: 'right', width: '8%', align: 'center', toolbar: '#eqpLineToolBar' }
            ]]
            , where: {
                processfilter: processfilter,
                searchText: ''
            }
            , done: function (data) {
            }
        });
    }

    table.on('edit(equipmenttable)', function (obj) {
        layer.confirm('Are you sure you want to modify?', { icon: 3, title: 'Modification Confirmation', content: obj.value }, function (index) {
            $.ajax({
                url: '/Equipment/Edit',
                data: {
                    "EQID": obj.data.ID,
                    "TYPEID": obj.data.TYPEID,
                    "value": obj.value,
                    "field": obj.field
                },
                type: 'POST',
                contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
                async: false,
                success: function (data) {
                    layer.msg(data.message);
                },
                error: function (message) {
                    alert('Error!');
                }
            });
            layer.close(index);
        },
            function (index) {
                layer.msg('Cancelled');
                ectable.reload();
            });
    });

    table.on('toolbar(equipmenttable)', function (obj) {
        if (obj.event == 'add') {
            layer.open({
                type: 2,
                content: 'EqpConfig',
                title: 'Add Configuration',
                area: ['80%', '85%'],
                success: function (layero, index) {
                    var iframeWindow = layero.find('iframe')[0].contentWindow;
                }
            }, function (value, index, elem) {
            });
        }
    });

    table.on('tool(equipmenttable)', function (obj) {
        var event = obj.event, // Current cell event property value
            selectdata = obj.data;
        if (event === 'edit') {
            layer.open({
                type: 2,
                content: 'EqpConfig?equipmentId=' + selectdata.ID,
                title: 'Edit Configuration',
                area: ['80%', '85%'],
                success: function (layero, index) {
                    var iframeWindow = layero.find('iframe')[0].contentWindow;
                }
            }, function (value, index, elem) {
            });
        }
    });
});
