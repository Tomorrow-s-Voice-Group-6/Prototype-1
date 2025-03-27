let show = document.getElementById("modal-show").value;
let modal = new bootstrap.Modal(document.getElementById("new-director-modal"), { keyboard: false, backdrop: "static" });

if (show == "show") {
    modal.show();
} else {
    modal.hide();
}