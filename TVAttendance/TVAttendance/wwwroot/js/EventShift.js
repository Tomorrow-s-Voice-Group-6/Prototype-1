document.addEventListener("DOMContentLoaded", function () {
    let btnCreate = document.getElementById("btn-create");
    let shiftDateInput = document.getElementById("new-shift-date");

    let eventStart = document.getElementById("event-start").value; 
    let eventEnd = document.getElementById("event-end").value; 

   
    btnCreate.disabled = true;

    shiftDateInput.addEventListener("change", function () {
        let shiftDate = shiftDateInput.value;
        let errorMsg = "";
        console.log("Event Start:", eventStart);
        console.log("Event End:", eventEnd);

        console.log("Shift Date:", shiftDate);

        //shift date is before event start date
        if (shiftDate < eventStart) {
            errorMsg = "Error: You cannot create a shift before the event start date!";
        } else if (shiftDate > eventEnd) {
            errorMsg = "Error: You cannot create a shift after the event end date!";
        }

        if (errorMsg) {
            toastr.error(errorMsg);
            btnCreate.disabled = true;
        } else {
            btnCreate.disabled = false;
        }
    });
});