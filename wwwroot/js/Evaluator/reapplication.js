var dataTable;

$(document).ready(function () {
    loadDataTable();
})

function loadDataTable() {
    dataTable = $('#tblReapplication').DataTable({
        responsive: true,
        ajax: { url: '/admin/evaluator/GetAllForReapplication' },
        columns: [
            { data: 'transId', width: '25%' },
            { data: 'estName', width: '15%' },
            { data: 'estEvalName', width: '10%' },
            { data: 'estRegistrationDate', width: '15%' },
            { data: 'estStatus', width: '10%' },
            {
                data: 'id',
                render: data => `
                    <div class="w-75 btn-group" role="group">
                        <a href="/admin/evaluator/reapplicationitem?id=${data}" class="btn btn-primary mx-2">
                            <i class="bi bi-pencil-square"></i>Send Email
                        </a>
                    </div>`,
                width: '25%'
            }
        ]
    });
}
function getAntiForgeryToken() {
    return $('#ajaxAntiForgeryForm input[name="__RequestVerificationToken"]').val();
}
function Approve(url) {
    $.ajax({
        url: url,
        type: 'POST',
        success: function (data) {
            toastr.success(data.message);
        }
    });
}