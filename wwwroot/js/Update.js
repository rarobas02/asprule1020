(() => {
    const resetLocationPlaceholders = () => {
        const form = document.getElementById("Rule1020UpdateForm");
        if (!form || typeof register === "undefined") {
            return;
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
    };

    document.addEventListener("DOMContentLoaded", resetLocationPlaceholders);
})();

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