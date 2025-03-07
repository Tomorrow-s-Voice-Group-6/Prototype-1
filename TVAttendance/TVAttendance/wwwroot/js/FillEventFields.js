document.addEventListener("DOMContentLoaded", function () {
    document.getElementById("fill").addEventListener("click", function (e) {
        e.preventDefault();
        document.getElementById("event-name").value = "Bake sale";
        document.getElementById("event-street").value = "24 grassy ave";
        document.getElementById("event-city").value = "Welland";
        document.getElementById("event-postal").value = "A1A2B2";
        document.getElementById("event-province").value = 8;
        document.getElementById("event-start").value = "2025-12-06";
        document.getElementById("event-end").value = "2025-12-09";
    })
});