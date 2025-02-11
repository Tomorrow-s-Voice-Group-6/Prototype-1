//Age Correction
let youngest = document.getElementById("young-dob")
let oldest = document.getElementById("old-dob")

youngest.addEventListener("focusout", function () {
    if (oldest.value != "")
    {
        AgeSwitch()
    }

    console.log(oldest.value, typeof(oldest.value))
})

oldest.addEventListener("focusout", function () {
    if (youngest.value != "")
    {
        AgeSwitch()
    }
})

function AgeSwitch() {
    var bigNum = parseInt(oldest.value)
    var smallNum = parseInt(youngest.value)

    if (bigNum < smallNum) {
        youngest.value = bigNum
        oldest.value = smallNum
    }
    else {
        youngest.value = smallNum
        oldest.value = bigNum
    }
}

//Date Correction
let fromDate = document.getElementById("date-older")
let toDate = document.getElementById("date-newer")

toDate.addEventListener("change", function () {
    if (fromDate.value != "") {
        DateSwitch()
    }
})

fromDate.addEventListener("change", function () {
    if (toDate.value != "") {
        DateSwitch()
    }
})

function DateSwitch() {
    var oldestDate = new Date(fromDate.value)
    var recentDate = new Date(toDate.value)

    if (oldestDate > recentDate) {
        toDate.value = oldestDate.toISOString().substring(0, 10)
        fromDate.value = recentDate.toISOString().substring(0, 10)
    }
}