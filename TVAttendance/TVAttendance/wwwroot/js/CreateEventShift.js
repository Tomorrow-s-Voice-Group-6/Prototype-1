document.addEventListener("DOMContentLoaded", function () {
    //Get html values from hidden viewdata's or input values
    let btnCreate = document.getElementById("btn-create");
    let shiftDateInput = document.getElementById("new-shift-date");

    let eventStart = document.getElementById("event-start").value;
    let eventEnd = document.getElementById("event-end").value;

    //Add an event listener when the user changes the event to check the date
    shiftDateInput.addEventListener("change", function () {
        let shiftDate = shiftDateInput.value;
        let errorMsg = "";

        //shift date is before event start date
        if (shiftDate < eventStart) {
            errorMsg = "Error: You cannot create a shift before the event start date!";
        }
        //shift date is after event end date
        else if (shiftDate > eventEnd) {
            errorMsg = "Error: You cannot create a shift after the event end date!";
        }

        //If there was any errors, display them and disable the 
        //create button until the error has been fixed by the user
        if (errorMsg) {
            toastr.error(errorMsg);
            btnCreate.disabled = true;
        } else {
            btnCreate.disabled = false;
        }
    });

});