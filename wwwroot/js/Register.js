let Branches = []
let LaborUnion = []


const register = {
    reviewTargets: {
        Email: "reviewEmail"
    },
    getField: function (key) {
        if (!key) {
            return null;
        }

        return document.querySelector(`[data-element="${key}"]`);
    },
    togglePasswordVisibility: function (toggleId, inputKey) {
        const toggle = document.getElementById(toggleId);
        const input = this.getField(inputKey);

        if (!toggle || !input) return;

        const icon = toggle.querySelector("i");

        toggle.addEventListener("click", function () {
            const isPassword = input.type === "password";
            input.type = isPassword ? "text" : "password";

            if (icon) {
                icon.classList.toggle("fa-eye", !isPassword);
                icon.classList.toggle("fa-eye-slash", isPassword);
            }
        });
    },
    validation: {
        input: function (input) {
            input.addClass("is-invalid");
            input.focus();
            // Scroll to the input field's position
            $("html, body").animate(
                {
                    scrollTop: input.offset().top,
                },
                500
            );
        },
        password: function (input) {
            // Regular expression for alphanumeric and other special characters, and at least 8 characters
            const regex = /^(?=.*[0-9])(?=.*[a-zA-Z])[a-zA-Z0-9!@#$%^&*()_+{}\[\]:;<>,.?~\\/-]{8,}$/;

            // TEst the input against the regular expression
            return regex.test(input);
        },
    },
    endpoints:
    {
        province: '/Client/Register/GetProvDist',
        city: '/Client/Register/GetCityMun',
        barangay: '/Client/Register/GetBrgy',
        usernameIsExist: '/Client/Register/IsUsernameExist',
        emailIsExist: '/Client/Register/IsEmailExist'
    },
    placeholders:
    {
        province: 'Select Province',
        city: 'Select City/Municipality',
        barangay: 'Select Barangay'
    },
    review: function () {
        // Get the form element
        const form = document.getElementById("Rule1020RegistrationForm");

        // Get all input and select elements inside the form
        const form_elements = form.querySelectorAll("input, select");

        const excluded_keys = ["password","confirmPassword","EstTechInfo1OtherCheckBox", "EstTechInfo2OtherCheckBox", "EstIsHaveLaborUnion", "EstIsHaveBranchUnits", "certify", "certifydpa"];
        const excluded_names = ["EstBranchRule1020Number[]", "EstBranchName[]", "EstBranchEstName[]", "EstUnionName[]", "EstUnionAddress[]", "EstUnionBLR[]"];

        const no_input_text = "No Input";

        let tech_info_1_checkbox = [];
        let tech_info_2_checkbox = [];

        // Iterate through each form element and log the ID
        form_elements.forEach(function (element) {
            let value = "";
            if (element.type === "checkbox" && element.name === "EstTechInfo1") {
                // For checkboxes, check if it's checked and add the value to the array
                if (element.checked) {
                    tech_info_1_checkbox.push(element.value);
                }
            } else if (element.type === "checkbox" && element.name === "EstTechInfo2") {
                // For checkboxes, check if it's checked and add the value to the array
                if (element.checked) {
                    tech_info_2_checkbox.push(element.value);
                }
            } else {
                const fieldKey = element.dataset?.element ?? element.id;

                if (!fieldKey) {
                    return;
                }

                value = element.value ?? "";
                // get file name only
                if (element.type == "file") {
                    value = value.split("\\").pop();
                }

                //console.log(element.id);
                //console.log({ value });
                // Exclude specific IDs (customize as needed)
                if (!excluded_keys.includes(fieldKey) && !excluded_names.includes(element.name)) {
                    const reviewId = register.reviewTargets[fieldKey] ?? `review${fieldKey}`;
                    const reviewElement = document.getElementById(reviewId);
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
    toggleDivAndRequired: function ({
        selectId,
        triggerValue,
        divId,
        inputId
    }) {
        const selectEl = this.getField(selectId);
        const divEl = document.getElementById(divId);
        const inputEl = this.getField(inputId);

        if (!selectEl || !divEl || !inputEl) return;

        selectEl.addEventListener("change", function () {
            if (this.value === triggerValue) {
                divEl.classList.remove("d-none");
                inputEl.setAttribute("required", "required");
            } else {
                divEl.classList.add("d-none");
                inputEl.removeAttribute("required");
                inputEl.value = ""; // optional: clear value
            }
        });
    },
    toggleDivOnCheckbox: function (checkboxId, divId, inputKey) {
        const checkbox = document.getElementById(checkboxId);
        const div = document.getElementById(divId);
        const input = inputKey ? this.getField(inputKey) : null;

        if (!checkbox || !div) return;
        const handleToggle = () => {
            if (checkbox.checked) {
                div.classList.remove('d-none');
                if (input) {
                    input.setAttribute('required', 'required');
                }
            } else {
                div.classList.add('d-none');
                if (input) {
                    input.removeAttribute('required');
                    input.value = '';
                }
            }
        };

        handleToggle();
        checkbox.addEventListener('change', handleToggle);
    },
    hideDivOnSelect: function ({ selectId, hideValue, divId }) {
        const selectEl = this.getField(selectId);
        const divEl = document.getElementById(divId);

        if (!selectEl || !divEl) {
            return;
        }

        const toggleVisibility = () => {
            const shouldHide = selectEl.value === hideValue;
            divEl.classList.toggle("d-none", shouldHide);
            divEl.setAttribute("aria-hidden", shouldHide ? "true" : "false");
        };

        toggleVisibility();
        selectEl.addEventListener("change", toggleVisibility);
    },
    branch_unit: function () {
        const values = [];

        document.querySelectorAll("#EstBranchDiv .branch-unit-div").forEach((row) => {
            const rowValues = {
                EstBranchRule1020Number: row.querySelector('[name="EstBranchRule1020Number[]"]')?.value?.trim() || "",
                EstBranchName: row.querySelector('[name="EstBranchName[]"]')?.value?.trim() || "",
                EstBranchEstName: row.querySelector('[name="EstBranchEstName[]"]')?.value?.trim() || ""
            };

            values.push(rowValues);
        });

        return values;
    },
    labor_union: function () {
        const uvalues = [];

        document.querySelectorAll("#EstLaborUnionDiv .labor-union-div").forEach((row) => {
            const EstUnionRowValues = {
                EstUnionName: row.querySelector('[name="EstUnionName[]"]')?.value?.trim() || "",
                EstUnionAddress: row.querySelector('[name="EstUnionAddress[]"]')?.value?.trim() || "",
                EstUnionBLR: row.querySelector('[name="EstUnionBLR[]"]')?.value?.trim() || ""
            };

            uvalues.push(EstUnionRowValues);
        });

        return uvalues;
    },
    stepper: function () {
        var current = 1,
            current_step,
            next_step,
            steps;
        steps = $("fieldset").length;
        register.review();
        const usernameInput = this.getField("UserName");
        if (usernameInput) {
            register.attachAvailabilityValidation({
                input: usernameInput,
                endpoint: register.endpoints.usernameIsExist,
                queryParam: "UserName",
                unavailableMessage: "Username is already taken."
            });

        }

		const emailInput = this.getField("Email");
        if (emailInput) {
            register.attachAvailabilityValidation({
                input: emailInput,
                endpoint: register.endpoints.emailIsExist,
				queryParam: "EstEmail",
                unavailableMessage: "Establishment email is already taken."
            });
        }

		$(".next").click(async function () {
            let current = $(this).closest("fieldset");
            let next = current.next("fieldset");
            if (!next.length) {
                return;
            }

			const usernameInputEl = register.getField("UserName");
			const emailInputEl = register.getField("Email");

			const isUsernameAvailable = await register.validateAvailability({
				input: usernameInputEl,
				endpoint: register.endpoints.usernameIsExist,
				queryParam: "UserName",
				unavailableMessage: "Username is already taken."
			});

			if (isUsernameAvailable) {
				const message = usernameInputEl?.dataset?.availabilityMessage || "Username is already taken.";
				register.showAvailabilityError?.(message);
				if (typeof Swal !== "undefined") {
					Swal.fire({
						title: "Error!",
						theme: "bootstrap-5",
						html: `<span><strong>${message}</strong></span>`,
						icon: "error",
						confirmButtonColor: "#dc3545",
						confirmButtonText: "Close",
						allowOutsideClick: false
					});
				}
				usernameInputEl?.focus();
				return;
			}

			const isEmailAvailable = await register.validateAvailability({
				input: emailInputEl,
				endpoint: register.endpoints.emailIsExist,
				queryParam: "EstEmail",
				unavailableMessage: "Establishment email is already taken."
			});

			if (isEmailAvailable) {
				const message = emailInputEl?.dataset?.availabilityMessage || "Establishment email is already taken.";
				if (typeof Swal !== "undefined") {
					Swal.fire({
						title: "Error!",
						theme: "bootstrap-5",
						html: `<span><strong>${message}</strong></span>`,
						icon: "error",
						confirmButtonColor: "#dc3545",
						confirmButtonText: "Close",
						allowOutsideClick: false
					});
				}
				emailInputEl?.focus();
				return;
			}

            let branches = register.branch_unit();
            let laborUnions = register.labor_union();
            if (branches.length > 0)
            {
                let branchContainer = document.getElementById("review_branch_unit_container");
                branchContainer.classList.remove("d-none");
                branchContainer.style.display = "block";
                // Use map to create an array of table rows
                let rows = branches.map(function (data) {
                    return `<tr>
							<td class="text-center">${data.EstBranchRule1020Number}</td>
							<td class="text-center">${data.EstBranchName}</td>
							<td class="text-center">${data.EstBranchEstName}</td>
                        </tr>`;
                });

                // Append the array of rows to the table body
                $("#review_est_branch_body").html(rows.join(""));
            }
            else
            {
                const branchContainer = document.getElementById("review_branch_unit_container");
                branchContainer.classList.add("d-none");
                branchContainer.style.display = "none";
                $("#review_est_branch_body").html("");
            }
            if (laborUnions.length > 0) {
                const laborContainer = document.getElementById("review_labor_union_container");
                laborContainer.classList.remove("d-none");
                laborContainer.style.display = "block";
                // Use map to create an array of table rows
                let rows_union = laborUnions.map(function (data_union) {
                    return `<tr>
							<td class="text-center">${data_union.EstUnionName}</td>
							<td class="text-center">${data_union.EstUnionAddress}</td>
							<td class="text-center">${data_union.EstUnionBLR}</td>
                        </tr>`;
                });

                // Append the array of rows to the table body
                $("#review_est_union_body").html(rows_union.join(""));
            }
            else
            {
                const laborContainer = document.getElementById("review_labor_union_container");
                laborContainer.classList.add("d-none");
                laborContainer.style.display = "none";
                $("#review_est_union_body").html("");
            }
			if (!validateRequiredFields(current)) {
				return;
			}

			register.review();
			current.addClass("d-none");
			next.removeClass("d-none");
        });

        $(".previous").click(function () {
            const current = $(this).closest("fieldset");
            const prev = current.prev("fieldset");
            if (!prev.length) {
                return;
            }

            current.addClass("d-none");
            prev.removeClass("d-none");
        });

        function validateRequiredFields(step) {
            var valid = true;

            // Check required input fields
            step.find("input[required], select[required]").each(function () {
                var elementValue = $(this).val();

                if (elementValue === null || elementValue.trim() === "") {
                    $(this).addClass("is-invalid");
                    valid = false;
                } else {
                    $(this).removeClass("is-invalid");
                }
            });

            return valid;
        }
    },
    formatNumber: function (input) {
        // Remove all non-numeric characters except decimal point
        let value = input.value.replace(/[^\d.]/g, '');

        // Ensure only one decimal point exists
        let parts = value.split('.');
        if (parts.length > 2) {
            parts = [parts[0], parts.slice(1).join('')];
        }

        // Limit decimal places to 2
        if (parts[1] && parts[1].length > 2) {
            parts[1] = parts[1].substring(0, 2);
        }

        // Format integer part with commas
        parts[0] = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, ',');

        // Combine parts
        input.value = parts.join('.');
    },
    preventFutureDate: function (inputId) {
        const bindHandler = () => {
            const input = typeof inputId === 'string' ? register.getField(inputId) : inputId;

            if (!input) {
                return;
            }

            const today = new Date();
            today.setHours(0, 0, 0, 0);
            const todayValue = today.toISOString().split('T')[0];

            input.setAttribute('max', todayValue);

            input.addEventListener('change', () => {
                if (input.value && input.value > todayValue) {
                    input.value = todayValue;
                }
            });
        };

        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', bindHandler, { once: true });
        } else {
            bindHandler();
        }
    },
    preventPastDate: function (inputId) {
        const bindHandler = () => {
            const input = typeof inputId === 'string' ? register.getField(inputId) : inputId;

            if (!input) {
                return;
            }

            const today = new Date();
            today.setHours(0, 0, 0, 0);
            const todayValue = today.toISOString().split('T')[0];

            input.setAttribute('min', todayValue);

            input.addEventListener('change', () => {
                if (input.value && input.value < todayValue) {
                    input.value = todayValue;
                }
            });
        };

        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', bindHandler, { once: true });
        } else {
            bindHandler();
        }
    },

    password: function (input) {
        // Regular expression for alphanumeric and other special characters, and at least 8 characters
        const regex = /^(?=.*[0-9])(?=.*[a-zA-Z])[a-zA-Z0-9!@#$%^&*()_+{}\[\]:;<>,.?~\\/-]{8,}$/;

        // TEst the input against the regular expression
        return regex.test(input);
    },

    location: function () {
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
                        `${module.endpoints.province}?EstRegion=${encodeURIComponent(regionValue)}`
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
    getLocationControls: function () {
        const region = this.getField('EstRegion');
        const province = this.getField('EstProvince');
        const city = this.getField('EstCityMun');
        const barangay = this.getField('EstBrgy');

        if (!region || !province || !city || !barangay) {
            console.warn('Register location controls are missing from the DOM.');
            return null;
        }

        return { region, province, city, barangay };
    },
    resetSelect: function (selectElement, placeholderText, shouldDisable = true) {
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
        //if (!expectArray) {
        //    return payload;
        //}
        if (!payload.status || !Array.isArray(payload.data)) {
            return [];
        }

        return payload.data;
    },
    setFieldValidationResult: function (inputEl, isValid, message = "This field is required.") {
        const feedback = inputEl.nextElementSibling?.classList.contains("invalid-feedback")
            ? inputEl.nextElementSibling
            : null;

        if (isValid === null) {
            inputEl.classList.remove("is-valid", "is-invalid");
            if (feedback) {
                feedback.textContent = message;
            }
            return;
        }

        if (isValid) {
            inputEl.classList.remove("is-invalid");
            inputEl.classList.add("is-valid");
            if (feedback) {
                feedback.textContent = "";
            }
        } else {
            inputEl.classList.remove("is-valid");
            inputEl.classList.add("is-invalid");
            if (feedback) {
                feedback.textContent = message;
            }
        }
    },
    validateInputIfExist: async function (inputValue, requestInput) {
        try {
            const response = await fetch(`${register.endpoints.usernameIsExist}?${requestInput}=${encodeURIComponent(inputValue)}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (!response.ok) {
                throw new Error('Network response was not ok');
            }

            const result = await response.json();

            if (result.status === false) {
                // Username already exists
                console.error(result.data);
                return {
                    isValid: false,
                    message: result.data
                };
            }

            // Username is available
            return {
                isValid: true,
                message: "Username is available."
            };

        } catch (error) {
            console.error('Error validating username:', error);
            return {
                isValid: false,
                message: "An error occurred while validating username."
            };
        }
    },
    validateAvailability: async function ({ input, endpoint, queryParam, unavailableMessage = "Value is already taken." }) {
        const inputEl = typeof input === "string" ? this.getField(input) : input;

        if (!inputEl || !endpoint || !queryParam) {
            return;
        }

        const value = inputEl.value.trim();

        if (!value) {
            register.setFieldValidationResult(inputEl, null);
            return;
        }

        register.setFieldValidationResult(inputEl, null);

        try {
            const url = `${endpoint}?${encodeURIComponent(queryParam)}=${encodeURIComponent(value)}`;
            const response = await fetch(url, { headers: { Accept: "application/json" }, cache: "no-cache" });

            if (!response.ok) {
                throw new Error(`Request failed with status ${response.status}`);
            }

            const payload = await response.json();

            if (payload?.status) {
                register.setFieldValidationResult(inputEl, true);
            } else {
                const message = typeof payload?.data === "string" ? payload.data : unavailableMessage;
                register.setFieldValidationResult(inputEl, false, message);
            }
        } catch (error) {
            console.error("Unable to validate field availability.", error);
            register.setFieldValidationResult(inputEl, false, "Unable to validate right now.");
        }
    },
    attachAvailabilityValidation: function ({ input, endpoint, queryParam, unavailableMessage }) {
        if (!input || !endpoint || !queryParam) {
            return;
        }

        const validate = () => register.validateAvailability({ input, endpoint, queryParam, unavailableMessage });
        input.addEventListener("blur", validate);
        input.addEventListener("input", () => register.setFieldValidationResult(input, null));
    },
    validateFile: function (inputFile, errorMessageId, maxSizeInBytes, allowedExtensions) {
        var fileSize = inputFile.files[0].size; // in bytes
        var fileExtension = inputFile.files[0].name.split('.').pop().toLowerCase();

        if (!allowedExtensions.includes(fileExtension)) {
            $(errorMessageId).text('Only ' + allowedExtensions.join(', ') + ' files are allowed.');
            inputFile.value = ''; // Clear the file input
            return;
        }

        if (fileSize > maxSizeInBytes) {
            $(errorMessageId).text('File size exceeds the limit (' + (maxSizeInBytes / (1024 * 1024)) + ' MB). Please choose a smaller file.');
            inputFile.value = ''; // Clear the file input
        } else {
            $(errorMessageId).text(''); // Clear any previous error messages
        }
    }
};

Ladda.bind(".ladda-button");
register.location();

register.toggleDivAndRequired({
    selectId: "EstLegalOrg",
    triggerValue: "OTHERS",
    divId: "EstLegalOrgOtherDiv",
    inputId: "EstLegalOrgOther"
});
register.togglePasswordVisibility("togglePassword", "password");
register.togglePasswordVisibility("toggleConfirmPassword", "confirmPassword");
register.toggleDivAndRequired({
    selectId: "EstBusinessNature",
    triggerValue: "Others,Please Specify",
    divId: "EstOtherBusNatureDiv",
    inputId: "EstOtherBusNature"
});
register.hideDivOnSelect({ selectId: "EstType", hideValue: "BRANCH ONLY", divId: "EstBisnessUnitDiv" });
register.toggleDivOnCheckbox("EstTechInfo1OtherCheckBox", "EstTechInfo1OtherDiv", "EstTechInfo1Other");
register.toggleDivOnCheckbox("EstTechInfo2OtherCheckBox", "EstTechInfo2OtherDiv", "EstTechInfo2Other");
register.toggleDivOnCheckbox("EstIsHaveBranchUnits", "EstBranchUnitContent");
register.toggleDivOnCheckbox("EstIsHaveLaborUnion", "EstLaborUnionContent");
register.preventFutureDate("EstSECDateIssued");
register.preventFutureDate("EstBisPermitDateIssued");
register.preventPastDate("EstBisPermitValidityDate");
register.preventFutureDate("EstOwnerValidIDDateIssued");
register.preventPastDate("EstOwnerValidIDDateExpire");
register.stepper();

document.addEventListener("input", function (e) {
    if (e.target.matches('[data-element="EstMaleCount"], [data-element="EstFemaleCount"]')) {
        const input = e.target;

        // Remove non-digits
        let value = input.value.replace(/\D+/g, "");

        // Remove leading zeros
        value = value.replace(/^0+/, "");

        // If empty after cleaning, set to "0"
        if (value === "") {
            value = "0";
        }

        input.value = value;

        const maleCount = parseInt(register.getField("EstMaleCount")?.value, 10) || 0;
        const femaleCount = parseInt(register.getField("EstFemaleCount")?.value, 10) || 0;

        const result = maleCount + femaleCount;

        const totalEmployeesInput = register.getField("EstTotalEmployees");
        if (totalEmployeesInput) {
            totalEmployeesInput.value = result;
        }
    }
});


$(document)
    .off("submit", "#Rule1020RegistrationForm")
    .on("submit", "#Rule1020RegistrationForm", function (event) {
        Ladda.create(document.getElementById("registerSubmit")).start();

        // Check if the checkbox is checked
        var certify = $("#certify").is(":checked");
        var certifydpa = $("#certifydpa").is(":checked");

        if (!certify || !certifydpa) {
            alert("Please agree to the terms and conditions.");
            Ladda.stopAll();
            event.preventDefault(); // Prevent the form from submitting
        }
    })
    .off("change", '[data-element="EstType"]')
    .on("change", '[data-element="EstType"]', function () {
        var select = register.getField("EstType");
        var input = document.getElementById("EstBisnessUnitDiv");
        if (select.value === "SINGLE ESTABLISHMENT") {
            input.style.display = "block";
            input.required = true;
            $("#EstBranchDiv").empty();
        } else {
            input.style.display = "none";
            input.required = false;
            $("#EstBranchDiv").empty();
        }
        // reset branches
        Branches = [];
        $("#EstBranchDiv").empty();
    })
    .off("input", '[data-element="EstCurrentCap"]')
    .on("input", '[data-element="EstCurrentCap"]', function () {
        let EstCurrentCapInput = register.getField("EstCurrentCap");
        register.formatNumber(EstCurrentCapInput);
    })
    .off("input", '[data-element="EstTotalAssets"]')
    .on("input", '[data-element="EstTotalAssets"]', function () {
        let EstTotalAssetsInput = register.getField("EstTotalAssets");
        register.formatNumber(EstTotalAssetsInput);
    })
    .off("click", '[data-element="EstIsPeza"]')
    .on("click", '[data-element="EstIsPeza"]', function () {
        const EstIsPeza = register.getField("EstIsPeza");
        const EstPezaNoExpiry = document.getElementById("EstPezaNoExpiry");
        const EstBisPermitValidityDate = register.getField("EstBisPermitValidityDate");
        const EstBisPermitValidityDateRequiredSign = document.getElementById("EstBisPermitValidityDateRequiredSign");
        const permitLabels = document.querySelectorAll(".EstBisPermitNumberLabelText");

        EstPezaNoExpiry.style.display = EstIsPeza.checked ? "none" : "block";
        EstBisPermitValidityDate.value = '';
        EstBisPermitValidityDate.required = !EstIsPeza.checked;
        EstBisPermitValidityDateRequiredSign.textContent = EstIsPeza.checked ? '' : '*';

        permitLabels.forEach(label => {
            label.textContent = EstIsPeza.checked ? "PEZA Certificate" : "Business or Mayor's Permit";
        });
    })
    .off("click", "#EstAddBranchBtn")
    .on("click", "#EstAddBranchBtn", function (event) {
        // Create a new div element for the branch unit form fields
        const branchUnitDiv = document.createElement("div");
        branchUnitDiv.className =
            "branch-unit-div rounded border border-1 p-4 my-4";

        // Set the innerHTML of the new div
        branchUnitDiv.innerHTML = `

          <div class="row mb-2">
            <div class="col-lg-4 col-md-12 col-sm-12 form-outline mb-4">
              <label for="EstBranchRule1020Number" class="form-label">Rule 1020 Application #</label>
              <input type="text" class="form-control form-control-sm EstBranchRule1020Number" name="EstBranchRule1020Number[]"
                required />
            </div>
            <div class="col-lg-4 col-md-12 col-sm-12 form-outline mb-4">
              <label for="EstBranchName" class="form-label">Branch Establishment Name</label>
              <input type="text" class="form-control form-control-sm EstBranchName" name="EstBranchName[]"
                required />
            </div>
            <div class="col-lg-4 col-md-12 col-sm-12 form-outline mb-4">
              <label for="EstBranchEstName" class="form-label">Establishment Name</label>
              <input type="text" class="form-control form-control-sm EstBranchEstName" name="EstBranchEstName[]"
                required />
            </div>
          </div>
          <div class="row mb-2">
            <div class="d-flex align-items-end">
              <button class="btn btn-danger ms-auto removeBranchUnitBtn" type="button">Remove</button>
            </div>
          </div>
  `;

        // Append the new div to the subcontractorContainer
        document.getElementById("EstBranchDiv").prepend(branchUnitDiv);

        branchUnitDiv.querySelector('.removeBranchUnitBtn').addEventListener('click', function () {
            branchUnitDiv.remove();
        });
    })
    .off("click", "#EstAddUnionBtn")
    .on("click", "#EstAddUnionBtn", function (event) {
        // Create a new div element for the subcontractor form fields
        const laborUnionDiv = document.createElement("div");
        laborUnionDiv.className =
            "labor-union-div rounded border border-1 p-4 my-4";

        // Set the innerHTML of the new div
        laborUnionDiv.innerHTML = `

          <div class="row mb-2">
            <div class="col-lg-4 col-md-12 col-sm-12 form-outline">
              <label for="EstUnionName" class="form-label">Name of Labor union</label>
              <input type="text" class="form-control form-control-sm EstUnionName" name="EstUnionName[]"
                required />
            </div>
            <div class="col-lg-4 col-md-12 col-sm-12 form-outline">
              <label for="EstUnionAddress" class="form-label">Address of Labor union</label>
              <input type="text" class="form-control form-control-sm EstUnionAddress" name="EstUnionAddress[]"
                required />
            </div>
            <div class="col-lg-4 col-md-12 col-sm-12 form-outline">
              <label for="EstUnionBLR" class="form-label">BLR Registration number</label>
              <input type="text" class="form-control form-control-sm EstUnionBLR" name="EstUnionBLR[]"
                required />
            </div>
          </div>
          <div class="row mb-2">
            <div class="d-flex align-items-end">
              <button class="btn btn-danger ms-auto removeLaborUniohnBtn" type="button">Remove</button>
            </div>
          </div>
  `;

        // Append the new div to the subcontractorContainer
        document.getElementById("EstLaborUnionDiv").prepend(laborUnionDiv);
        laborUnionDiv.querySelector('.removeLaborUniohnBtn').addEventListener('click', function () {
            laborUnionDiv.remove();
        });
    })
    .off("click", "#EstIsHaveBranchUnits")
    .on("click", "#EstIsHaveBranchUnits", function (event) {
        const branchContainer = $("#EstBranchDiv");
        const checkbox = event.currentTarget;

        if (!checkbox.checked && branchContainer.length) {
            branchContainer.empty();
        }
    })
    .off("click", "#EstIsHaveLaborUnion")
    .on("click", "#EstIsHaveLaborUnion", function (event) {
        const laborUnionContainer = $("#EstLaborUnionDiv");
        const checkbox = event.currentTarget;

        if (!checkbox.checked && laborUnionContainer.length) {
            laborUnionContainer.empty();
        }
    })
    .off("change", '[data-element="EstSECFile"]')
    .on("change", '[data-element="EstSECFile"]', function (event) {
        register.validateFile(this, '#EstSECFileErrorMessege', 25 * 1024 * 1024, ['pdf']);
    })
    .off("change", '[data-element="EstBisPermitFile"]')
    .on("change", '[data-element="EstBisPermitFile"]', function (event) {
        register.validateFile(this, '#EstBisPermitFileErrorMessege', 25 * 1024 * 1024, ['pdf']);
    })
    .off("change", '[data-element="EstOwnerValidIDFile"]')
    .on("change", '[data-element="EstOwnerValidIDFile"]', function (event) {
        register.validateFile(this, '#EstOwnerValidIDFileErrorMessege', 25 * 1024 * 1024, ['pdf']);
    })

