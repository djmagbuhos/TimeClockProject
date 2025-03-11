$(document).ready(function () {
    //LOG IN MODAL
    $("#loginModal").hide();

    $(".ci-btn").on("click", function () {
        $("#loginModal").fadeIn();
    });

    $(".close").on("click", function () {
        $("#loginModal").fadeOut();
    });

    $(window).on("click", function (event) {
        if ($(event.target).is("#loginModal")) {
            $("#loginModal").fadeOut();
        }
    });

    // LOG OUT MODAL
    $("#logoutModal").hide();

    $(".co-btn").on("click", function () {
        $("#logoutModal").fadeIn();
    });

    $(".close").on("click", function () {
        $("#logoutModal").fadeOut();
    });

    $(window).on("click", function (event) {
        if ($(event.target).is("#logoutModal")) {
            $("#logoutModal").fadeOut();
        }
    });

    //VIEW EMPLOYEE EDIT EMPLOYEE MODAL
    $("#viewprofile").hide();
    // Open the modal and show profile content
    $(".el-view-btn").click(function () {
        // Get the employee details from the button's data attributes
        const employeeId = $(this).attr("data-id");
        const firstName = $(this).attr("data-firstname");
        const lastName = $(this).attr("data-lastname");
        const position = $(this).attr("data-position");
        const dob = $(this).attr("data-dob");
        const gender = $(this).attr("data-gender");
        const profilePicture = $(this).attr("data-pfp"); // Get profile picture

        // Set the modal fields using the data from the button
        $(".vp-id").text(employeeId);  // Display ID in the modal view section
        $(".vp-firstname").text(firstName);
        $(".vp-lastname").text(lastName);
        $(".vp-position").text(position);
        $(".vp-dob").text(dob);
        $(".vp-gender").text(gender);
        $(".vp-img").attr("src", profilePicture); // Set profile image in modal

        // Store employeeId in a variable for future use
        const vpId = employeeId;

        // Show the modal
        $("#viewprofile").show();
        $(".vp-content").show();
        $(".vp-edit-container").hide();

        // Set the ID in the edit form
        $(".vp-edit-container input[name='Id']").val(vpId); // Use vpId variable here
    });

    // ---------EDIT EMPLOYEE----------------- //
    $(".vp-edit").click(function () {
        // Get the Date of Birth and format it for the input type="date"
        const dob = $(".vp-dob").text().trim();
        const formattedDob = new Date(dob).toISOString().split('T')[0]; // Get just the date part

        // Copy values from View Profile to Edit Form
        $(".vp-edit-container input[name='FirstName']").val($(".vp-firstname").text().trim());
        $(".vp-edit-container input[name='LastName']").val($(".vp-lastname").text().trim());
        $(".vp-edit-container input[name='DateOfBirth']").val(formattedDob); // Set the date input
        $(".vp-edit-container select[name='Gender']").val($(".vp-gender").text().toLowerCase().trim());
        $(".vp-edit-container .vp-img").attr("src", $(".vp-img").attr("src"));

        // Get the employee ID from the modal and set it in the form (this will be done after the modal opens)
        //const vpId = $(".vp-id").text().trim();
        //$(".vp-edit-container input[name='Id']").val(vpId); // Use vpId variable to set the form ID

        $(".vp-content").hide();
        $(".vp-edit-container").show();
    });


    // Close the modal when clicking .vp-close or .close
    $(".vp-close, .close").click(function () {
        $("#viewprofile").hide();
    });

    $(window).on("click", function (event) {
        if ($(event.target).is("#viewprofile")) {
            $("#viewprofile").fadeOut();
        }
    });

    // EDIT TIME MODAL
    $("#editModal").hide();

    $(".edit-rec-btn").on("click", function () {
        $("#editModal").fadeIn();
    });

    $(".close").on("click", function () {
        $("#editModal").fadeOut();
    });

    $(window).on("click", function (event) {
        if ($(event.target).is("#editModal")) {
            $("#editModal").fadeOut();
        }
    });

    // ADD TIME MODAL
    $("#addModal").hide();

    $("#sr-addtime").on("click", function () {
        $("#addModal").fadeIn();
    });

    $(".close").on("click", function () {
        $("#addModal").fadeOut();
    });

    $(window).on("click", function (event) {
        if ($(event.target).is("#addModal")) {
            $("#addModal").fadeOut();
        }
    });

    // ADD TIME MODAL
    $("#viewModal").hide();

    $(".view-rec-btn").on("click", function () {
        $("#viewModal").fadeIn();
    });

    $(".close").on("click", function () {
        $("#viewModal").fadeOut();
    });

    $(window).on("click", function (event) {
        if ($(event.target).is("#viewModal")) {
            $("#viewModal").fadeOut();
        }
    });

    // EDIT USER MODAL
    $("#edit-user-modal").hide();

    $(".userlist-edit").on("click", function () {
        $("#edit-user-modal").fadeIn();
    });

    $(".close").on("click", function () {
        $("#edit-user-modal").fadeOut();
    });

    $(window).on("click", function (event) {
        if ($(event.target).is("#edit-user-modal")) {
            $("#edit-user-modal").fadeOut();
        }
    });

    //DELETE CONFIRMATION MODAL
    // DELETE CONFIRMATION MODAL
    $("#deleteModal").hide();


    $(".delete-btn").on("click", function () {
        const employeeId = $(this).attr("data-id");
        const firstName = $(this).attr("data-firstname");
        const lastName = $(this).attr("data-lastname");
        const fullName = lastName + ", " + firstName;
        $(".del-empname").text(fullName);

        // Save the emp ID
        $("#deleteModal").data("employee-id", employeeId);


        $("#deleteModal").fadeIn();
    });

 
    $(".close, .del-close").on("click", function () {
        $("#deleteModal").fadeOut();
    });

   
    $(window).on("click", function (event) {
        if ($(event.target).is("#deleteModal")) {
            $("#deleteModal").fadeOut();
        }
    });

    //confirmation of delete
    $(".del-confirm").on("click", function () {
        const employeeId = $("#deleteModal").data("employee-id");

        // AJAX request to del the employee
        $.ajax({
            url: "/Admin/DeleteEmployee",
            type: "POST",
            data: {
                id: employeeId 
            },
            success: function (response) {
               
                $("#deleteModal").fadeOut();
                window.location.href = "/Admin/EmployeeList";  
            },
            error: function (xhr, status, error) {
                // Handle error (if deletion fails)
                alert("Error deleting employee!");
                $("#deleteModal").fadeOut();
            }
        });
    });

});
