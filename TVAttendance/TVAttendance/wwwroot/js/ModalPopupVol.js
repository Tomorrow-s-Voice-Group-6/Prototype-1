﻿let show = document.getElementById("modal-show").value
let modal = new bootstrap.Modal(document.getElementById("new-volunteer-modal"), { keyboard: false, backdrop: "static" })

if (show == "display") {
    modal.show()
    }
    else {
    modal.hide()
}
