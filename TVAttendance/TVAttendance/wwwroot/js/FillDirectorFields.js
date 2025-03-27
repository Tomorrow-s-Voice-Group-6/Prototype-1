document.getElementById("fill").addEventListener("click", function (e) {
    e.preventDefault(); // Prevents the form from being submitted
    document.getElementById("dir-first").value = "Janet";
    document.getElementById("dir-last").value = "Stone";
    document.getElementById("dir-email").value = "JStone@hotmail.com";
    document.getElementById("phoneNumber").value = "2890391873";
});