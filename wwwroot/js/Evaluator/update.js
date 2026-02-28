var dataTable;

$(document).ready(function () {
    loadDataTable();
})

function loadDataTable() {
    dataTable = $('#tblUpdate').DataTable({
        responsive: true,
        ajax: { url: '/admin/evaluator/GetAllForUpdate' },
        columns: [
            { data: 'transId', width: '15%' },
            { data: 'estName', width: '25%' },
            { data: 'estEvalName', width: '10%' },
            { data: 'estRegistrationDate', width: '15%' },
            { data: 'estPoHeadEvalDate', width: '10%' },
            { data: 'estStatus', width: '10%' },
            {
                data: 'id',
                render: data => `
                    <div class="w-75 btn-group" role="group">
                        <a href="/admin/evaluator/approveditem?id=${data}" class="btn btn-primary mx-2">
                            <i class="bi bi-pencil-square"></i>Review
                        </a>
                    </div>`,
                width: '25%'
            }
        ]
    });
}