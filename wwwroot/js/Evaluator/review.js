var dataTable;

$(document).ready(function () {
    loadDataTable();
})

function loadDataTable() {
    dataTable = $('#tblReview').DataTable({
        responsive: true,
        ajax: { url: '/admin/evaluator/GetAllForReview' },
        columns: [
            { data: 'transId', width: '25%' },
            { data: 'estName', width: '15%' },
            {
                data: null,
                render: (_, __, row) =>
                    `${row.estOwnerFirst} ${row.estOwnerMid ?? ''} ${row.estOwnerLast}`,
                width: '10%'
            },
            { data: 'estRegistrationDate', width: '15%' },
            { data: 'estStatus', width: '10%' },
            {
                data: 'id',
                render: data => `
                    <div class="w-75 btn-group" role="group">
                        <a href="/admin/evaluator/reviewitem?id=${data}" class="btn btn-primary mx-2">
                            <i class="bi bi-pencil-square"></i>Review
                        </a>
                    </div>`,
                width: '25%'
            }
        ]
    });
}
