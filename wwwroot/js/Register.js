const Register = {
    endpoints: {
        province: '/Client/Register/GetProvDist',
        city: '/Client/Register/GetCityMun',
        barangay: '/Client/Register/GetBrgy'
    },
    placeholders: {
        province: 'Select Province',
        city: 'Select City/Municipality',
        barangay: 'Select Barangay'
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
    getLocationControls: function () {
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
        if (!payload.status || !Array.isArray(payload.data)) {
            return [];
        }

        return payload.data;
    }
};

Register.location();