var dataTable;

$(document).ready(function () {
    loadDataTable();
})

function loadDataTable() {
    dataTable = $('#tblPoHeadApproved').DataTable({
        responsive: true,
        ajax: { url: '/admin/pohead/GetAllApproved' },
        columns: [
            { data: 'transId', width: '25%' },
            { data: 'estName', width: '15%' },
            { data: 'estRegistrationDate', width: '15%' },
            { data: 'estEvalName', width: '15%' },
            { data: 'estStatus', width: '10%' },
            {
                data: 'id',
                render: data => `
                    <div class="w-75 btn-group" role="group">
                        <a href="/admin/pohead/approveditem?id=${data}" class="btn btn-primary mx-2">
                            <i class="bi bi-pencil-square"></i>View
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