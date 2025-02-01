document.addEventListener("DOMContentLoaded", function () {
    const phoneInput = document.getElementsByClassName("phoneNumber");

    phoneInput.addEventListener("input", function () {
        let rawValue = phoneInput.value.replace(/\D/g, "");
        if (rawValue.length > 10) {
            rawValue = rawValue.substring(0, 10);
        }

        let formattedValue = "";
        if (rawValue.length > 6) {
            formattedValue = `${rawValue.substring(0, 3)}-${rawValue.substring(3, 6)}-${rawValue.substring(6)}`;
        } else if (rawValue.length > 3) {
            formattedValue = `${rawValue.substring(0, 3)}-${rawValue.substring(3)}`;
        } else {
            formattedValue = rawValue;
        }

        phoneInput.value = formattedValue;
    });
});
