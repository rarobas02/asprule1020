var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblReview').DataTable({
        responsive: true,
        ajax: { url: '/admin/evaluator/GetAllForReview' },
        columns: [
            { data: 'transId', width: '25%' },
            { data: 'estName', width: '15%' },
            {
                data: null,
                render: (_, __, row) => `${row.estOwnerFirst} ${row.estOwnerMid ?? ''} ${row.estOwnerLast}`,
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

function getAntiForgeryToken() {
    return $('#ajaxAntiForgeryForm input[name="__RequestVerificationToken"]').val();
}

function prepareEvaluationBinding() {
    // Prefix checkbox names + force bool value
    $(".remarks-check-box").each(function () {
        if (!this.name.startsWith("CheckList.")) {
            this.name = `CheckList.${this.name}`;
        }
        this.value = "true";
    });

    // Prefix remarks input names
    $(".input-remarks").each(function () {
        if (!this.name.startsWith("Remarks.")) {
            this.name = `Remarks.${this.name}`;
        }
    });
}

function buildEvaluationFormData() {
    prepareEvaluationBinding();

    // Ensure unchecked checkboxes are posted as false
    $(".remarks-check-box").each(function () {
        if (!this.checked) {
            $("#evalform").append(
                `<input type="hidden" class="tmp-unchecked" name="${this.name}" value="false" />`
            );
        }
    });

    // Serialize including disabled inputs
    const disabledInputs = $("#evalform :input:disabled").removeAttr("disabled");
    const formData = $("#evalform").serialize();
    disabledInputs.attr("disabled", "disabled");

    $(".tmp-unchecked").remove();
    return formData;
}

var pro_eval = {
    update: {
        status: function (button, recommendation) {
            var formData = buildEvaluationFormData();

            $.ajax({
                type: "POST",
                url: `/Admin/Evaluator/EvaluationResult`,
                data: formData,
                headers: {
                    'RequestVerificationToken': getAntiForgeryToken()
                },
                success: function (response) {
                    if (response.success) {
                        Swal.fire({
                            title: "Success!",
                            html: `<span>Application Number <strong>${response.trans_no}</strong> is recommended <strong>${response.recommendation}</strong></span>`,
                            icon: "success",
                            confirmButtonColor: "#dc3545",
                            confirmButtonText: "Close",
                            allowOutsideClick: false,
                        }).then((result) => {
                            if (result.isConfirmed) {
                                window.location.href = "/admin/evaluator/review";
                            }
                        });
                    }
                    button.html("Submit");
                    button.prop("disabled", false);
                    recommendation.value = "";
                },
                error: function (error) {
                    console.error("Error:", error);
                },
            });
        },
    },
};

$("#submit").click(function () {
    const recommendation = document.getElementById("recom");

    if (!recommendation.value) {
        Swal.fire({
            title: "Invalid",
            text: "Please select a recommendation before proceeding.",
            icon: "error",
        });
        return;
    }

    const button = $(this);
    button.html('<i class="fas fa-spinner fa-spin"></i> Loading');
    button.prop("disabled", true);
    pro_eval.update.status(button, recommendation);
});

$(".remarks-check-box").change(function () {
    const textBox = $(this).closest("dd").find(".input-remarks");
    textBox.prop("disabled", this.checked);
    textBox.val("");
});

