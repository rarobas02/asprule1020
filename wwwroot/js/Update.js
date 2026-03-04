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