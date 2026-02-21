var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblManageUser').DataTable({
        responsive: true,
        ajax: { url: '/Admin/User/GetAllUser' },
        columns: [
            { data: null, render: (_, __, row) => `${row.firstName} ${row.middleName ?? ''} ${row.lastName}`, width: "20%" },
            { data: 'userName', width: "10%" },
            { data: 'email', width: "25%" },
            { data: 'estProvince', width: "10%" },
            { data: 'role', width: "10%" },
            {
                data: 'id',
                render: (data) => `
                    <div class="w-75 btn-group" role="group">
                        <a href="/admin/user/create?id=${data}" class="btn btn-primary mx-2"><i class="bi bi-pencil-square"></i>Edit</a>
                        <button type="button" class="btn btn-danger mx-2" onclick="Delete('/admin/user/delete?id=${data}')">
                            <i class="bi bi-trash-fill"></i>Delete
                        </button>
                    </div>`,
                width: "25%"
            }
        ]
    });
}

function getAntiForgeryToken() {
    return $('#ajaxAntiForgeryForm input[name="__RequestVerificationToken"]').val();
}

function Delete(url) {
    Swal.fire({
        title: "Are you sure?",
        text: "You won't be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, delete this User!"
    }).then((result) => {
        if (!result.isConfirmed) {
            return;
        }

        $.ajax({
            url: url,
            type: 'DELETE',
            dataType: 'json',
            headers: {
                'RequestVerificationToken': getAntiForgeryToken()
            },
            success: function (response) {
                if (response.success) {
                    dataTable.ajax.reload(null, false);
                    toastr.success(response.message);
                } else {
                    toastr.error(response.message ?? "Unable to delete the user.");
                }
            },
            error: function (xhr) {
                const message = xhr.responseJSON?.message ?? xhr.responseText ?? "Unexpected error while deleting the user.";
                toastr.error(message);
            }
        });
    });
}
