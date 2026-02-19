var dataTable;

$(document).ready(function () {
    loadDataTable();
})

function loadDataTable() {
    dataTable = $('#tblManageUser').DataTable({
        "responsive": true,
        "ajax": { url: '/Admin/User/GetAllUser' },
        "columns": [
            { data: 'estName', "width": "20%" },
            { data: 'estUsername', "width": "10%" },
            { data: 'userName', "width": "25%" },
            { data: 'estProvince', "width": "10%" },
            { data: "role", "width": "10%" },
            {
                data: 'id',
                "render": function (data) {
                    return `
                    <div class="w-75 btn-group" role="group">
                    <a href="/admin/user/upsert?id=${data}" class="btn btn-primary mx-2"><i class="bi bi-pencil-square"></i>Edit</a>
                    <a onClick=Delete('/admin/user/delete/${data}') class="btn btn-danger mx-2"><i class="bi bi-trash-fill"></i>Edit</a>
                    </div>
                    `
                },
                "width": "25%"
            }
        ]
    });
}
