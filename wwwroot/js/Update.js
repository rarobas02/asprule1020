let update = {
    apiRequest: async function (url, options = {}) {
        const {
            method = 'GET',
            data = null,
            headers = {}
        } = options;

        try {
            const config = {
                method,
                headers: {
                    'Content-Type': 'application/json',
                    ...headers
                }
            };

            // Attach body only if there's data (for POST, PUT, etc.)
            if (data) {
                config.body = JSON.stringify(data);
            }

            const response = await fetch(url, config);

            // Handle HTTP errors
            if (!response.ok) {
                throw new Error(`HTTP error! Status: ${response.status}`);
            }

            // Parse JSON
            const result = await response.json();
            return result;

        } catch (error) {
            console.error('API Request Error:', error);
            throw error;
        }
    },
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
    get:
    {
        branch_unit: function (trans_no) {
            update.apiRequest(`/GetBranchUnit?id=${trans_no}`);
        },
        labor_union: function (trans_no) {
            update.apiRequest(`/GetLaborUnion?id=${trans_no}`);
        },
    },
    branch_unit: async function (trans_no) {
        update.get.branch_unit(trans_no, function (resp) {
            // Assuming you have a table body element with the id 'est_branch_body'
            const tableBody = document.getElementById("est_branch_body");

            // Clear the existing content of the table body
            tableBody.innerHTML = "";

            if (resp.data.length > 0) {
                // Loop through the sample data and dynamically append rows to the table
                resp.data.forEach((data, index) => {
                    const row = document.createElement("tr");

                    row.innerHTML = `
                <td class="row-index text-center">
                  <p>${index + 1}</p>
                </td>
                <td class="row-index text-center">${data.one}</td>
                <td class="row-index text-center">${data.two}</td>
                <td class="row-index text-center">${data.three}</td>
                <td class="d-flex gap-2">
                  <button class="btn btn-warning text-white update_branch" type="button" data-branch_id="${data.primo}" data-rule_1020_no="${data.one}" data-branch_name="${data.two}" data-branch_location="${data.three}"><span class="fas fa-edit"></span></button>
                  <button class="btn btn-danger remove_branch" type="button" data-branch_id="${data.primo}"><span class="fas fa-trash-alt"></span></button>
                </td>
              `;

                    tableBody.appendChild(row);
                });
            }
        });
    },
    labor_union: async function (trans_no) {
        update.get.labor_union(trans_no, function (resp) {
            // Assuming you have a table body element with the id 'est_branch_body'
            const tableBody = document.getElementById("est_union_body");

            // Clear the existing content of the table body
            tableBody.innerHTML = "";

            if (resp.data.length > 0) {
                // Loop through the sample data and dynamically append rows to the table
                resp.data.forEach((data, index) => {
                    const row = document.createElement("tr");

                    row.innerHTML = `
                <td class="row-index text-center">
                  <p>${index + 1}</p>
                </td>
                <td class="row-index text-center">${data.union_name}</td>
                <td class="row-index text-center">${data.union_address}</td>
                <td class="row-index text-center">${data.union_blr}</td>
                <td class="d-flex gap-2">
                  <button class="btn btn-warning text-white update_labor_union" type="button" data-union_id="${data.union_id}" data-union_name="${data.union_name}" data-union_address="${data.union_address}" data-union_blr="${data.union_blr}"><span class="fas fa-edit"></span></button>
                  <button class="btn btn-danger remove_labor_union" type="button" data-union_id="${data.union_id}"><span class="fas fa-trash-alt"></span></button>
                </td>
              `;

                    tableBody.appendChild(row);
                });
            }
        });
    },

}
document.addEventListener("DOMContentLoaded", update.resetLocationPlaceholders());

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
    });