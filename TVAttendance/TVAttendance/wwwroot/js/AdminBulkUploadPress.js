document.addEventListener("DOMContentLoaded", function () {
    if (sessionStorage.getItem("openBulkUpload") === "true") {
        sessionStorage.removeItem("openBulkUpload");

        const bulkCollapse = document.getElementById("collapseBulkSinger");

        if (bulkCollapse) {
            new bootstrap.Collapse(bulkSection, { toggle: true });   
            setTimeout(function () {
                bulkSection.scrollIntoView({ behavior: "smooth", block: "end" });
            }, 500); // Adjust timing if needed
        }
        }
    });