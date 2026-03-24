let branch_modal = new bootstrap.Modal(document.querySelector("#branch_modal"), {
    backdrop: "static",
    keyboard: false,
});

let labor_union_modal = new bootstrap.Modal(document.querySelector("#labor_union_modal"), {
    backdrop: "static",
    keyboard: false,
});


// branch
let rule_1020_no_inp = document.getElementById("rule_1020_no_inp"),
    branch_name_inp = document.getElementById("branch_name_inp"),
    branch_location_inp = document.getElementById("branch_location_inp");

let branch_add_button_container = document.getElementById("branch_add_button_container"),
    branch_update_button_container = document.getElementById("branch_update_button_container");

// labor union
let labor_of_union_inp = document.getElementById("labor_of_union_inp"),
    labor_union_address_inp = document.getElementById("labor_union_address_inp"),
    blr_no_inp = document.getElementById("blr_no_inp");

let labor_union_add_button_container = document.getElementById("labor_union_add_button_container"),
    labor_union_update_button_container = document.getElementById("labor_union_update_button_container");

let hidden_trans_no = document.querySelector(`[data-element="Id"]`).value;

const api = window.endpoint ?? {};

const routeUrl = (path, query = {}) => {
    const url = new URL(path, window.location.origin);
    Object.entries(query).forEach(([key, value]) => {
        if (value !== undefined && value !== null && value !== "") {
            url.searchParams.set(key, value);
        }
    });
    return url.toString();
};


var update = {
    api_request: function (api, params, type, cb) {
        $.ajax({
            type: type, // GET, POST, PUT
            url: api,
            data: params,
            success: function (response) {
                return cb(response);
            },
            error: function (jqXHR, textStatus, errorThrown) {
                console.error("AJAX Text Status:", textStatus);
                console.error("AJAX Error:", errorThrown);

                if (textStatus === "timeout") {
                    alert("Request timed out. Please check your internet connection.");
                } else if (textStatus === "error") {
                    if (jqXHR.status === 0) {
                        alert("Network error. Please check your internet connection.");
                    } else if (jqXHR.status === 401) {
                        alert("Unauthorized Request.");
                    } else {
                        // Handle other HTTP errors as needed
                        alert("An error occurred. Please try again later.");
                    }
                }
            },
        });
    },
    api_request_form: function (api, params, type, cb) {
        $.ajax({
            type: type, // GET, POST, PUT
            url: api,
            data: params,
            contentType: false,
            processData: false,
            success: function (response) {
                return cb(response);
            },
            error: function (jqXHR, textStatus, errorThrown) {
                console.error("AJAX Text Status:", textStatus);
                console.error("AJAX Error:", errorThrown);

                if (textStatus === "timeout") {
                    alert("Request timed out. Please check your internet connection.");
                } else if (textStatus === "error") {
                    if (jqXHR.status === 0) {
                        alert("Network error. Please check your internet connection.");
                    } else if (jqXHR.status === 401) {
                        alert("Unauthorized Request.");
                    } else {
                        // Handle other HTTP errors as needed
                        alert("An error occurred. Please try again later.");
                    }
                }
            },
        });
    },
    location:
    {
        resetLocationPlaceholders: function () {
            const form = document.getElementById("Rule1020UpdateForm");
            if (!form || typeof register === "undefined") {
                return;
            }
            const region = register.getField("EstRegion");
            if (region) {
                region.value = "";
                region.selectedIndex = 0;
            }
            const placeholders = {
                EstRegion: "Select Region",
                EstProvince: "Select Province",
                EstCityMun: "Select City/Municipality",
                EstBrgy: "Select Barangay"
            };

            Object.entries(placeholders).forEach(([fieldKey, label]) => {
                const select = register.getField(fieldKey);
                if (!select) {
                    return;
                }

                let placeholderOption = select.querySelector('option[value=""]');
                if (!placeholderOption) {
                    placeholderOption = document.createElement("option");
                    placeholderOption.value = "";
                    placeholderOption.textContent = label;
                    select.prepend(placeholderOption);
                }

                placeholderOption.selected = true;
                select.value = "";
            });
        },
    },
    add: {
        branch_unit: function (hidden_trans_no, rule_1020, branch_name, branch_location, callback) {
            const params = {
                RegisterId: hidden_trans_no,
                rule1020Number: rule_1020,
                branchName: branch_name,
                branchAddress: branch_location,
            };
            update.api_request(api.branchUnit.add, params, "POST", function (resp) {
                callback(resp);
            });
        },
        labor_union: function (hidden_trans_no, labor_of_union_inp, labor_union_address_inp, blr_no_inp, callback) {
            const params = {
                RegisterId: hidden_trans_no,
                UnionName: labor_of_union_inp,
                UnionAddress: labor_union_address_inp,
                UnionBLR: blr_no_inp,
            };
            update.api_request(api.laborUnion.add, params, "POST", function (resp) {
                callback(resp);
            });
        },
    },
    delete: {
        branch_unit: function (branch_id, callback) {
            const params = {
                id: branch_id,
            };
            update.api_request(api.branchUnit.remove + `?id=${branch_id}`, params, "POST", function (resp) {
                callback(resp);
            });
        },
        labor_union: function (union_id, callback) {
            const params = {
                id: union_id,
            };
            update.api_request(api.laborUnion.remove + `?id=${union_id}`, params, "POST", function (resp) {
                callback(resp);
            });
        },
    },
    get: {
        branch_unit: function (trans_no, callback) {
            const params = {
                Id: trans_no,
            };
            update.api_request(api.branchUnit.get, params, "GET", function (resp) {
                callback(resp);
            });
        },
        labor_union: function (trans_no, callback) {
            const params = {
                Id: trans_no,
            };
            update.api_request(api.laborUnion.get, params, "GET", function (resp) {
                callback(resp);
            });
        },
    },
    update: {
        branch_unit: function (branch_id, rule_1020_no, branch_name, branch_location, callback) {
            const params = {
                id: branch_id,
                rule1020Number: rule_1020_no,
                branchName: branch_name,
                branchAddress: branch_location
            };

            update.api_request(api.branchUnit.update, params, "POST", function (resp) {
                callback(resp);
            });
        },
        labor_union: function (union_id, labor_of_union_inp, labor_union_address_inp, blr_no_inp, callback) {
            const params = {
                Id: union_id,
                UnionName: labor_of_union_inp,
                UnionAddress: labor_union_address_inp,
                UnionBLR: blr_no_inp,
            };
            update.api_request(api.laborUnion.update, params, "POST", function (resp) {
                callback(resp);
            });
        }
    },
    populate: {
        branch_unit: async function (id) {
            update.get.branch_unit(id, function (resp) {
                const tableBody = document.getElementById("est_branch_body");
                tableBody.innerHTML = "";

                const rows = Array.isArray(resp?.data) ? resp.data : [];
                if (rows.length === 0) return;

                rows.forEach((data, index) => {
                    const branchId = data.id ?? data.Id ?? "";
                    const rule1020 = data.rule1020Number ?? data.Rule1020Number ?? "";
                    const branchName = data.branchName ?? data.BranchName ?? "";
                    const branchAddress = data.branchAddress ?? data.BranchAddress ?? "";

                    const row = document.createElement("tr");
                    row.innerHTML = `
                    <td class="row-index text-center"><p>${index + 1}</p></td>
                    <td class="row-index text-center">${rule1020}</td>
                    <td class="row-index text-center">${branchName}</td>
                    <td class="row-index text-center">${branchAddress}</td>
                    <td class="d-flex gap-2">
                        <button class="btn btn-warning text-white update_branch" type="button"
                            data-branch_id="${branchId}"
                            data-rule_1020_no="${rule1020}"
                            data-branch_name="${branchName}"
                            data-branch_location="${branchAddress}">
                            <span class="bi bi-pencil-square"></span>
                        </button>
                        <button class="btn btn-danger remove_branch" type="button" data-branch_id="${branchId}">
                            <span class="bi bi-trash"></span>
                        </button>
                    </td>
                `;
                    tableBody.appendChild(row);
                });
            });
        },
        labor_union: async function (id) {
            update.get.labor_union(id, function (resp) {
                const tableBody = document.getElementById("est_union_body");
                tableBody.innerHTML = "";

                const rows = Array.isArray(resp?.data) ? resp.data : [];
                if (rows.length === 0) return;

                rows.forEach((data, index) => {
                    const unionId = data.id ?? data.unionId ?? data.UnionId ?? data.Id ?? "";
                    const unionName = data.unionName ?? data.UnionName ?? "";
                    const unionAddress = data.unionAddress ?? data.UnionAddress ?? "";
                    const unionBLR = data.unionBLR ?? data.UnionBLR ?? "";

                    const row = document.createElement("tr");
                    row.innerHTML = `

                <td class="row-index text-center"><p>${index + 1}</p></td>
                <td class="row-index text-center">${unionName}</td>
                <td class="row-index text-center">${unionAddress}</td>
                <td class="row-index text-center">${unionBLR}</td>
                <td class="d-flex gap-2">
                    <button class="btn btn-warning text-white update_labor_union" type="button"
                        data-union_id="${unionId}"
                        data-union_name="${unionName}"
                        data-union_address="${unionAddress}"
                        data-union_blr="${unionBLR}">
                        <span class="bi bi-pencil-square"></span>
                    </button>
                    <button class="btn btn-danger remove_labor_union" type="button" data-union_id="${unionId}">
                        <span class="bi bi-trash"></span>
                    </button>
                </td>
            `;
                    tableBody.appendChild(row);
                });
            });
        }

    },
    clear: {
        branch_input: function () {
            rule_1020_no_inp.value = "";
            branch_name_inp.value = "";
            branch_location_inp.value = "";
        },
        labor_union: function () {
            labor_of_union_inp.value = "";
            labor_union_address_inp.value = "";
            blr_no_inp.value = "";
        },
        cancel: function () {
            branch_add_button_container.style.display = "block";
            branch_update_button_container.style.display = "none";
            update.clear.branch_input();
        },
        cancel_labor_union: function () {
            labor_union_add_button_container.style.display = "block";
            labor_union_update_button_container.style.display = "none";
            update.clear.labor_union();
        },
    }
};
document.addEventListener("DOMContentLoaded", update.location.resetLocationPlaceholders());

$(document)
    .off("click", "#est_closed")
    .on("click", "#est_closed", function () {
        const isClosed = $(this).is(":checked");
        const closureSection = $("#estabClosed");
        const dateFields = [
            '[data-element="EstClosureDate"]',
            '[data-element="EstReopeningDate"]',
            '[data-element="EstFiledClosureDate"]'
        ];

        closureSection.toggleClass("d-none", !isClosed);

        dateFields.forEach((selector) => {
            const field = $(selector);
            field.prop("required", isClosed);

            if (!isClosed) {
                field.val("");
            }
        });
    })
    // Branch Unit Modal
    .off("click", "#open_branch_modal_btn")
    .on("click", "#open_branch_modal_btn", function () {
        branch_modal.show();
        update.populate.branch_unit(hidden_trans_no);
    })

    .off("click", "#branch_add_button")
    .on("click", "#branch_add_button", function () {
        if (!branch_name_inp.value.trim() || !branch_location_inp.value.trim()) {
            Swal.fire({
                title: "Invalid",
                text: "Please fill out all required fields",
                icon: "error",
            });
        } else {
            update.add.branch_unit(hidden_trans_no, rule_1020_no_inp.value, branch_name_inp.value, branch_location_inp.value, function (resp) {
                if (resp.success) {
                    Swal.fire({
                        title: "Success",
                        text: resp.message,
                        icon: "success",
                    });
                    update.clear.branch_input();
                    update.populate.branch_unit(hidden_trans_no);
                } else {
                    Swal.fire({
                        title: "Invalid",
                        text: resp.message,
                        icon: "error",
                    });
                }
            });
        }
    })

    // populate inputs
    .off("click", ".update_branch")
    .on("click", ".update_branch", function () {
        const { branch_id, rule_1020_no, branch_name, branch_location } = $(this).data();

        // hide add buttons / show update and cancel buttons
        branch_add_button_container.style.display = "none";
        branch_update_button_container.style.display = "block";

        // populate values
        rule_1020_no_inp.value = rule_1020_no;
        branch_name_inp.value = branch_name;
        branch_location_inp.value = branch_location;

        $(document)
            .off("click", "#branch_update_button")
            .on("click", "#branch_update_button", function () {
                if (!branch_name_inp.value.trim() || !branch_location_inp.value.trim()) {
                    Swal.fire({
                        title: "Invalid",
                        text: "Please fill out all required fields",
                        icon: "error",
                    });
                } else {
                    update.update.branch_unit(branch_id, rule_1020_no_inp.value, branch_name_inp.value, branch_location_inp.value, function (resp) {
                        if (resp.success) {
                            Swal.fire({
                                title: "Success",
                                text: resp.message,
                                icon: "success",
                            });
                            update.clear.cancel();
                            update.populate.branch_unit(hidden_trans_no);
                        } else {
                            Swal.fire({
                                title: "Invalid",
                                text: resp.message,
                                icon: "error",
                            });
                        }
                    });
                }
            });
    })

    .off("click", "#branch_cancel_button")
    .on("click", "#branch_cancel_button", function () {
        update.clear.cancel();
    })

    .off("click", ".remove_branch")
    .on("click", ".remove_branch", function () {
        const { branch_id } = $(this).data();

        update.delete.branch_unit(branch_id, function (resp) {
            if (resp.status) {
                Swal.fire({
                    title: "Success",
                    text: resp.message,
                    icon: "success",
                });
                update.clear.branch_input();
                update.populate.branch_unit(hidden_trans_no);
            }
        });
    })

    // Labor Union Modal
    .off("click", "#open_labor_union_modal_btn")
    .on("click", "#open_labor_union_modal_btn", function () {
        update.populate.labor_union(hidden_trans_no);
    })

    .off("click", "#labor_union_add_button")
    .on("click", "#labor_union_add_button", function () {
        if (!labor_of_union_inp.value.trim() || !labor_union_address_inp.value.trim() || !blr_no_inp.value.trim()) {
            Swal.fire({
                title: "Invalid",
                text: "Please fill out all required fields",
                icon: "error",
            });
        } else {
            update.add.labor_union(hidden_trans_no, labor_of_union_inp.value, labor_union_address_inp.value, blr_no_inp.value, function (resp) {
                if (resp.success) {
                    Swal.fire({
                        title: "Success",
                        text: resp.message,
                        icon: "success",
                    });
                    update.clear.labor_union();
                    update.populate.labor_union(hidden_trans_no);
                } else {
                    Swal.fire({
                        title: "Invalid",
                        text: resp.message,
                        icon: "error",
                    });
                }
            });
        }
    })

    // populate inputs
    .off("click", ".update_labor_union")
    .on("click", ".update_labor_union", function () {
        const { union_id, union_name, union_address, union_blr } = $(this).data();

        // hide add buttons / show update and cancel buttons
        labor_union_add_button_container.style.display = "none";
        labor_union_update_button_container.style.display = "block";

        // populate values
        labor_of_union_inp.value = union_name;
        labor_union_address_inp.value = union_address;
        blr_no_inp.value = union_blr;

        $(document)
            .off("click", "#labor_union_update_button")
            .on("click", "#labor_union_update_button", function () {
                if (!labor_of_union_inp.value.trim() || !labor_union_address_inp.value.trim() || !blr_no_inp.value.trim()) {
                    Swal.fire({
                        title: "Invalid",
                        text: "Please fill out all required fields",
                        icon: "error",
                    });
                } else {
                    update.update.labor_union(union_id, labor_of_union_inp.value, labor_union_address_inp.value, blr_no_inp.value, function (resp) {
                        if (resp.success) {
                            Swal.fire({
                                title: "Success",
                                text: resp.message,
                                icon: "success",
                            });
                            update.clear.cancel_labor_union();
                            update.populate.labor_union(hidden_trans_no);
                        } else {
                            Swal.fire({
                                title: "Invalid",
                                text: resp.message,
                                icon: "error",
                            });
                        }
                    });
                }
            });
    })

    .off("click", "#labor_union_cancel_button")
    .on("click", "#labor_union_cancel_button", function () {
        update.clear.cancel_labor_union();
    })

    .off("click", ".remove_labor_union")
    .on("click", ".remove_labor_union", function () {
        const { union_id } = $(this).data();

        update.delete.labor_union(union_id, function (resp) {
            if (resp.status) {
                Swal.fire({
                    title: "Success",
                    text: resp.message,
                    icon: "success",
                });
                update.clear.labor_union();
                update.populate.labor_union(hidden_trans_no);
            }
        });
    })