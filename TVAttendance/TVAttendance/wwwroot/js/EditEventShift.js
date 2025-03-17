//Same as create just changed the button name
document.addEventListener("DOMContentLoaded", function () {
    let btnSave = document.getElementById("btn-save");
    let shiftDateInput = document.getElementById("new-shift-date");

    let eventStart = document.getElementById("event-start").value;
    let eventEnd = document.getElementById("event-end").value;

    shiftDateInput.addEventListener("change", function () {
        let shiftDate = shiftDateInput.value;
        let errorMsg = "";

        //shift date is before event start date
        if (shiftDate < eventStart) {
            errorMsg = "Error: You cannot create a shift before the event start date!";
        } else if (shiftDate > eventEnd) {
            errorMsg = "Error: You cannot create a shift after the event end date!";
        }

        if (errorMsg) {
            toastr.error(errorMsg);
            btnSave.disabled = true;
        } else {
            btnSave.disabled = false;
        }
    });
});