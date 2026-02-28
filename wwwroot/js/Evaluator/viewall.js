var dataTable;

$(document).ready(function () {
    loadDataTable();
})

function loadDataTable() {
    dataTable = $('#tblViewAll').DataTable({
        responsive: true,
        ajax: { url: '/admin/evaluator/GetAll' },
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
                        <a href="/admin/evaluator/viewitem?id=${data}" class="btn btn-primary mx-2">
                            <i class="bi bi-pencil-square"></i>View
                        </a>
                    </div>`,
                width: '25%'
            }
        ]
    });
}