
//------ADD EMPLOYEE JS--------//
$(document).ready(function () {
    $("#fileInput").change(function (event) {
        var file = event.target.files[0];

        if (file) {
            var reader = new FileReader();
            reader.onload = function (e) {
                $("#imgPreview").attr("src", e.target.result);
            };
            reader.readAsDataURL(file);
        } else {
            $("#imgPreview").attr("src", "/img/default-user.jpg");
        }
    });


    //-----EDIT EMPLOYEE JS--------//
    $("#edit-pfp-file").change(function (event) {
        var file = event.target.files[0];

        if (file) {
            var reader = new FileReader();
            reader.onload = function (e) {
                // Update the image preview with the selected file
                $("#vp-img").attr("src", e.target.result);
            };
            reader.readAsDataURL(file);
        } else {
            // Fallback to default if no file is selected
            $("#vp-img").attr("src", "/img/default-user.jpg");
        }
    });

});