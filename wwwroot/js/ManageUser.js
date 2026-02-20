var dataTable;

$(document).ready(function () {
    loadDataTable();
})

function loadDataTable() {
    dataTable = $('#tblManageUser').DataTable({
        "responsive": true,
        "ajax": { url: '/Admin/User/GetAllUser' },
        "columns": [
            { data: null, "render": function (data, type, row) { return row.firstName + ' ' + row.middleName + ' ' + row.lastName; }, "width": "20%" },
            { data: 'userName', "width": "10%" },
            { data: 'email', "width": "25%" },
            { data: 'estProvince', "width": "10%" },
            { data: "role", "width": "10%" },
            {
                data: 'id',
                "render": function (data) {
                    return `
                    <div class="w-75 btn-group" role="group">
                    <a href="/admin/user/create?id=${data}" class="btn btn-primary mx-2"><i class="bi bi-pencil-square"></i>Edit</a>
                    <a onClick=Delete('/admin/user/delete/${data}') class="btn btn-danger mx-2"><i class="bi bi-trash-fill"></i>Edit</a>
                    </div>
                    `
                },
                "width": "25%"
            }
        ]
    });
}
