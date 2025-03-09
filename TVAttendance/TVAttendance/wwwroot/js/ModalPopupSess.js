let show = document.getElementById("modal-show1").value
let modal = new bootstrap.Modal(document.getElementById("new-session-modal"), { keyboard: false, backdrop: "static" })

if (show == "display") {
    modal.show()
    }
    else {
    modal.hide()
}
