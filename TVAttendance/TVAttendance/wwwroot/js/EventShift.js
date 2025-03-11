//On button create click, some toastr validation

document.getElementById("btn-create").addEventListener("click", function (e) {
    e.preventDefault();
    let shiftStart = (document.getElementById("shift-start").value);
    let shiftEnd = (document.getElementById("shift-end").value);

    if (shiftStart && shiftEnd) {
        let shiftStartTime = new Date(shiftDate);
        let shiftEndTime = new Date(shiftDate);

        let [startHour, startMinute] = shiftStart.split(":");
        let [endHour, endMinute] = shiftEnd.split(":");

        shiftStartTime.setHours(startHour, startMinute, 0);
        shiftEndTime.setHours(endHour, endMinute, 0);

        if (shiftStartTime >= shiftEndTime) {
            toastr.error("Error: Shift start time must be before shift end time.");
        } else if (shiftEndTime <= shiftStartTime) {
            toastr.error("Error: Cannot create a shift that has an end time less than a start time.");
        } else if (shiftEndTime == shiftStartTime) {
            toastr.error("Error: Cannot create a shift the same start and end time.");
        }
        else {
            this.form.submit();
        }
    }
});
