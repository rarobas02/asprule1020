const Branches = []
const LaborUnion = []


const register = {
    endpoints:
    {
        province: '/Client/Register/GetProvDist',
        city: '/Client/Register/GetCityMun',
        barangay: '/Client/Register/GetBrgy',
        usernameIsExistt: '/Client/Register/IsUsernameExist'
    },
    placeholders:
    {
        province: 'Select Province',
        city: 'Select City/Municipality',
        barangay: 'Select Barangay'
    },
    review: function () {
        const form = document.getElementById("registerNew");

        // Get all input and select elements inside the form
        const form_elements = form.querySelectorAll("input, select");

        const excluded_ids = ["userName", "estTechInfo1Other", "estTechInfo2Other", "estIsHaveBranchUnits", "estIsHaveLaborUnion", "certify", "certifydpa"];
        const excluded_names = ["trans_col1[]", "est_name_col2[]", "location_col3[]", "est_union_name[]", "est_union_address[]", "est_union_blr[]"];

        const no_input_text = "No Input";

        let tech_info_1_checkbox = [];
        let tech_info_2_checkbox = [];

        // Iterate through each form element and log the ID
        form_elements.forEach(function (element) {
            let value = "";
            if (element.type === "checkbox" && element.name === "estTechInfo1[]") {
                // For checkboxes, check if it's checked and add the value to the array
                if (element.checked) {
                    tech_info_1_checkbox.push(element.value);
                }
            } else if (element.type === "checkbox" && element.name === "estTechInfo2[]") {
                // For checkboxes, check if it's checked and add the value to the array
                if (element.checked) {
                    tech_info_2_checkbox.push(element.value);
                }
            } else {
                value = document.getElementById(element.id).value;
                // get file name only
                if (element.type == "file") {
                    value = value.split("\\").pop();
                }

                //   console.log(element.id);
                //   console.log({ value });
                // Exclude specific IDs (customize as needed)
                if (!excluded_ids.includes(element.id) && !excluded_names.includes(element.name)) {
                    const reviewElement = document.getElementById(`review_${element.id}`);
                    // Check if reviewElement is not null or undefined
                    if (reviewElement) {
                        if (value.trim() !== "") {
                            reviewElement.innerHTML = value;
                        } else {
                            reviewElement.innerHTML = no_input_text;
                        }
                    }
                }
            }
        });
        // Display comma-separated list for checked checkboxes
        if (tech_info_1_checkbox.length > 0) {
            // console.log(tech_info_1_checkbox.join(", "));
            document.getElementById("reviewEstTechInfo1").innerHTML = tech_info_1_checkbox.join(", ");
        } else {
            document.getElementById("reviewEstTechInfo1").innerHTML = no_input_text;
        }

        // Display comma-separated list for checked checkboxes
        if (tech_info_2_checkbox.length > 0) {
            // console.log(tech_info_2_checkbox.join(", "));
            document.getElementById("reviewEstTechInfo2").innerHTML = tech_info_2_checkbox.join(", ");
        } else {
            document.getElementById("reviewEstTechInfo2").innerHTML = no_input_text;
        }
    },
    branch_unit: function () {
        let values = [];

        // Iterate through each row
        document.querySelectorAll("#est_branch_body tr").forEach(function (row) {
            let rowValues = {};

            // Get values from input fields in the current row
            rowValues.trans_col1 =
                row.querySelector('[name="trans_col1[]"]')?.value || "";
            rowValues.est_name_col2 =
                row.querySelector('[name="est_name_col2[]"]')?.value || "";
            rowValues.location_col3 =
                row.querySelector('[name="location_col3[]"]')?.value || "";

            // Add the values to the array
            values.push(rowValues);
        });

        return values;
    },

    stepper: function () {
        let current = 1;
        let current_step, next_step;
        const steps = document.querySelectorAll("fieldset").length;

        document.querySelectorAll(".next").forEach(function (btn) {
            btn.addEventListener("click", function () {
                current_step = this.parentElement;
                next_step = current_step.nextElementSibling;

                const username_inp = document.getElementById("Username");
                const password_inp = document.getElementById("password");

                if (username_inp.value.length < 6) {
                    Swal.fire({
                        title: "Error!",
                        html: `<span><strong>Username must be at least 6 characters long.</strong></span>`,
                        icon: "error",
                        confirmButtonColor: "#dc3545",
                        confirmButtonText: "Close",
                        allowOutsideClick: false,
                    });
                    reg.validation.input(username_inp);
                    return;
                }

                if (!reg.validation.password(password_inp.value)) {
                    Swal.fire({
                        title: "Error!",
                        html: `<span><strong>The password must be alphanumeric and at least 8 characters long.</strong></span>`,
                        icon: "error",
                        confirmButtonColor: "#dc3545",
                        confirmButtonText: "Close",
                        allowOutsideClick: false,
                    });
                    reg.validation.input(password_inp);
                    return;
                }

                reg.get.username(username_inp, function (resp) {
                    if (resp) {
                        Swal.fire({
                            title: "Error!",
                            html: `<span><strong>Username not available. Please, create another username.</strong></span>`,
                            icon: "error",
                            confirmButtonColor: "#dc3545",
                            confirmButtonText: "Close",
                            allowOutsideClick: false,
                        });
                        reg.validation.input(username_inp);
                    } else {
                        if (validateRequiredFields(current_step)) {
                            next_step.style.display = "block";
                            current_step.style.display = "none";

                            reg.populate.review();
                            branches = reg.populate.branch_unit();
                            labor_unions = reg.populate.labor_union();

                            const branchContainer = document.getElementById("review_branch_unit_container");
                            const branchBody = document.getElementById("review_est_branch_body");

                            if (branches.length > 0) {
                                branchContainer.style.display = "block";
                                branchBody.innerHTML = branches
                                    .map(data => `
                  <tr>
                    <td class="text-center">${data.trans_col1}</td>
                    <td class="text-center">${data.est_name_col2}</td>
                    <td class="text-center">${data.location_col3}</td>
                  </tr>
                `)
                                    .join("");
                            } else {
                                branchContainer.style.display = "none";
                                branchBody.innerHTML = "";
                            }

                            const unionContainer = document.getElementById("review_labor_union_container");
                            const unionBody = document.getElementById("review_est_union_body");

                            if (labor_unions.length > 0) {
                                unionContainer.style.display = "block";
                                unionBody.innerHTML = labor_unions
                                    .map(data => `
                  <tr>
                    <td class="text-center">${data.est_union_name}</td>
                    <td class="text-center">${data.est_union_address}</td>
                    <td class="text-center">${data.est_union_blr}</td>
                  </tr>
                `)
                                    .join("");
                            } else {
                                unionContainer.style.display = "none";
                                unionBody.innerHTML = "";
                            }
                        }
                    }
                });
            });
        });

        document.querySelectorAll(".previous").forEach(function (btn) {
            btn.addEventListener("click", function () {
                current_step = this.parentElement.parentElement;
                next_step = current_step.previousElementSibling;

                next_step.style.display = "block";
                current_step.style.display = "none";
            });
        });

        function validateRequiredFields(step) {
            let valid = true;

            step.querySelectorAll("input[required], select[required]").forEach(function (el) {
                const value = el.value;

                if (value === null || value.trim() === "") {
                    el.classList.add("is-invalid");
                    valid = false;
                } else {
                    el.classList.remove("is-invalid");
                }
            });

            return valid;
        }
    },
    password: function (input) {
        // Regular expression for alphanumeric and other special characters, and at least 8 characters
        const regex = /^(?=.*[0-9])(?=.*[a-zA-Z])[a-zA-Z0-9!@#$%^&*()_+{}\[\]:;<>,.?~\\/-]{8,}$/;

        // Test the input against the regular expression
        return regex.test(input);
    },
    labor_union: function () {
        let uvalues = [];

        // Iterate through each row
        document.querySelectorAll("#est_union_body tr").forEach(function (row) {
            let UnionrowValues = {};

            // Get values from input fields in the current row
            UnionrowValues.est_union_name =
                row.querySelector('[name="est_union_name[]"]')?.value || "";
            UnionrowValues.est_union_address =
                row.querySelector('[name="est_union_address[]"]')?.value || "";
            UnionrowValues.est_union_blr =
                row.querySelector('[name="est_union_blr[]"]')?.value || "";

            // Add the values to the array
            uvalues.push(UnionrowValues);
        });

        return uvalues;
    },
    location: function ()
    {
        const module = this;

        document.addEventListener('DOMContentLoaded', () => {
            const controls = module.getLocationControls();
            if (!controls) {
                return;
            }

            const { region, province, city, barangay } = controls;

            module.resetSelect(province, module.placeholders.province);
            module.resetSelect(city, module.placeholders.city);
            module.resetSelect(barangay, module.placeholders.barangay);

            region.addEventListener('change', async (event) => {
                const regionValue = event.target.value?.trim();

                module.resetSelect(province, module.placeholders.province);
                module.resetSelect(city, module.placeholders.city);
                module.resetSelect(barangay, module.placeholders.barangay);

                if (!regionValue) {
                    return;
                }

                try {
                    const provinces = await module.fetchAdministrativeData(
                        `${module.endpoints.province}?estRegion=${encodeURIComponent(regionValue)}`
                    );

                    module.populateSelect(province, provinces, (item) => ({
                        value: item.provinceDistrict ?? '',
                        label: item.provinceDistrict ?? ''
                    }));

                    province.disabled = provinces.length === 0;
                } catch (error) {
                    console.error('Unable to load province list.', error);
                }
            });

            province.addEventListener('change', async (event) => {
                const provinceValue = event.target.value?.trim();

                module.resetSelect(city, module.placeholders.city);
                module.resetSelect(barangay, module.placeholders.barangay);

                if (!provinceValue) {
                    return;
                }

                try {
                    const cities = await module.fetchAdministrativeData(
                        `${module.endpoints.city}?estProvince=${encodeURIComponent(provinceValue)}`
                    );

                    module.populateSelect(city, cities, (item) => ({
                        value: item.cityMunicipality ?? '',
                        label: item.cityMunicipality ?? ''
                    }));

                    city.disabled = cities.length === 0;
                } catch (error) {
                    console.error('Unable to load city/municipality list.', error);
                }
            });

            city.addEventListener('change', async (event) => {
                const cityValue = event.target.value?.trim();

                module.resetSelect(barangay, module.placeholders.barangay);

                if (!cityValue) {
                    return;
                }

                try {
                    const barangays = await module.fetchAdministrativeData(
                        `${module.endpoints.barangay}?estCityMun=${encodeURIComponent(cityValue)}`
                    );

                    module.populateSelect(barangay, barangays, (item) => ({
                        value: item.barangay ?? '',
                        label: item.barangay ?? ''
                    }));

                    barangay.disabled = barangays.length === 0;
                } catch (error) {
                    console.error('Unable to load barangay list.', error);
                }
            });
        });
    },
    getLocationControls: function ()
    {
        const region = document.getElementById('estRegion');
        const province = document.getElementById('estProvince');
        const city = document.getElementById('estCityMun');
        const barangay = document.getElementById('estBrgy');

        if (!region || !province || !city || !barangay) {
            console.warn('Register location controls are missing from the DOM.');
            return null;
        }

        return { region, province, city, barangay };
    },
    resetSelect: function (selectElement, placeholderText, shouldDisable = true)
    {
        if (!selectElement) {
            return;
        }

        selectElement.innerHTML = '';

        const placeholderOption = document.createElement('option');
        placeholderOption.value = '';
        placeholderOption.textContent = placeholderText;
        placeholderOption.disabled = true;
        placeholderOption.selected = true;

        selectElement.appendChild(placeholderOption);
        selectElement.disabled = shouldDisable;
    },
        populateSelect: function (selectElement, data, getOption) {
        if (!selectElement || !Array.isArray(data)) {
            return;
        }

        data.forEach((item) => {
            const optionConfig = getOption(item);
            const value = optionConfig?.value ?? '';
            const label = optionConfig?.label ?? '';

            if (!value || !label) {
                return;
            }

            const option = document.createElement('option');
            option.value = value;
            option.textContent = label;
            selectElement.appendChild(option);
        });
    },
        fetchAdministrativeData: async function (url) {
        const response = await fetch(url, {
            headers: { Accept: 'application/json' },
            cache: 'no-cache'
        });

        if (!response.ok) {
            throw new Error(`Request failed with status ${response.status}`);
        }

        const payload = await response.json();
        if (!payload.status || !Array.isArray(payload.data)) {
            return [];
        }

        return payload.data;
    }
};

register.location();