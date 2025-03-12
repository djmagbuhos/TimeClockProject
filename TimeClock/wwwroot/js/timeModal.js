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
    var statusId = $(this).data("statusid");

    //-----------DELETE EMPLOYEE MODAL------------------//
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

    //--------------END OF DELETE EMPLOYEE MODAL-----------------//


    //----------------- RECORD LIST ADMIN -----------------------//
    //---------------- EDIT TIME MODAL -------------------- //

    $("#editModal").hide();

    $(document).on("click", ".edit-rec-btn", function () {
        var id = $(this).data("id");
        var employeeName = $(this).data("employeename");
        var logDate = $(this).data("logdate");
        var timeIn = $(this).data("timein");
        var timeOut = $(this).data("timeout");
        // Display only
        $("#editempname").text(employeeName);
        $("#editLogDate").text(logDate);

        //value to edit
        $("#editId").val(id);
        $("#editTimeIn").val(timeIn);
        $("#editTimeOut").val(timeOut);
        $("#editStatusId").val(statusId);

        $("#editModal").fadeIn();
    });

    // Close button inside the edit modal
    $("#editModal .close").on("click", function () {
        $("#editModal").fadeOut();
    });

    // Close modal when clicking outside
    $("#editModal").on("click", function (event) {
        if (event.target == this) {
            $(this).fadeOut();
        }
    });

    $("#editForm").submit(function (e) {
        e.preventDefault();
        console.log("Form Data:", formData);
        var formData = $(this).serialize();

        $.ajax({
            url: "/Admin/EditTimeLogs",
            type: "POST",
            data: formData,
            success: function (data) {
                $("#editModal").fadeOut();
                location.reload(); 
            },
            error: function (error) {
                console.error("Error updating record:", error);
                alert("An error occurred while updating the record.");
                console.log("Server Response:", error.responseJSON); // Log the server's response
            }
        });

        console.log($("#editId").val());
    });

    $(window).on("click", function (event) {
        if ($(event.target).is("#editModal")) {
            $("#editModal").fadeOut();
        }
    });

    // -------------------END EDIT TIME MODAL---------------------//


    //-------------------------------------------------------//
    // ------------------ADD TIME MODAL----------------------//
    $("#addModal").hide();

    $("#sr-addtime").on("click", function () {
        $("#addModal").fadeIn();
    });

    $('#addTimeForm').submit(function (e) {
        e.preventDefault();

        var employeeId = parseInt($("#employeeid").val());
        var logDate = $("#logDate").val();
        var timeIn = $("#timein").val();
        var timeOut = $("#timeout").val();
        var statusId = $("#status").val();

        // Get today's date (YYYY-MM-DD format)
        var today = new Date().toISOString().split('T')[0];

        // Validate inputs before sending request
        if (!employeeId || !logDate || !timeIn || !timeOut) {
            alert("Please fill out all fields.");
            return;
        }

        // Ensure logDate is today or in the past
        if (logDate > today) {
            alert("Date must be today or before.");
            return;
        }

        var timeInDateTime = new Date(logDate + "T" + timeIn + ":00");
        var timeOutDateTime = new Date(logDate + "T" + timeOut + ":00");

        // Ensure TimeOUT is greater than TimeIN
        if (timeOutDateTime <= timeInDateTime) {
            alert("Time Out must be greater than Time In.");
            return;
        }

        var timeLogData = {
            EmpId: employeeId,
            LogDate: logDate,
            TimeIN: timeInDateTime.toISOString(),
            TimeOUT: timeOutDateTime.toISOString(),
            StatusID: statusId
        };

        $.ajax({
            type: "POST",
            url: "/Admin/AddTimeLogs",
            data: JSON.stringify(timeLogData),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (data) {
                if (data.redirectUrl) {
                    window.location.href = data.redirectUrl;
                } else {
                    alert("Time log added successfully!");
                    $('#addModal').hide();
                    window.location.reload();
                }
            },
            error: function (xhr) {
                alert("Error: " + xhr.responseJSON.message);
            }
        });
    });

    $(".close").on("click", function () {
        $("#addModal").fadeOut();
    });

    $(window).on("click", function (event) {
        if ($(event.target).is("#addModal")) {
            $("#addModal").fadeOut();
        }
    });

    //------------------END ADD TIME MODAL-------------------//


    //------------------VIEW TIME MODAL-----------------------//
    $("#viewModal").hide();

    $(".view-rec-btn").on("click", function () {
        const id = $(this).attr("data-id");
        const employeeName = $(this).attr("data-employeename");
        const logDate = $(this).attr("data-logdate");
        const timeIn = $(this).attr("data-timein");
        const timeOut = $(this).attr("data-timeout");
        const total = $(this).attr("data-total");
        const statusDescription = $(this).attr("data-statusdescription");

        $(".vp-id").text(id);
        $(".vp-employeename").text(employeeName);
        $(".vp-logdate").text(logDate);
        $(".vp-timein").text(timeIn);
        $(".vp-timeout").text(timeOut);
        $(".vp-total").text(total);
        $(".vp-statusdescription").text(statusDescription);


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
    //---------------------END VIEW TIME MODAL-------------------//

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

    //------------------ DELETE TIME MODAL ----------------------//

    $("#deleteModalRecord").hide();

    $(".del-rec-btn.delete-btn").on("click", function () { // Corrected class
        var logId = $(this).data("id");
        var logDate = $(this).data("logdate");
        var empName = $(this).data("employeename");
        var logTimeIn = $(this).data("timein");
        var logTimeOut = $(this).data("timeout");

        $(".del-rec-name").text(empName);
        $(".dec-rec-date").text(logDate);
        $(".del-rec-timein").text(logTimeIn);
        $(".del-rec-timeout").text(logTimeOut);

        $("#deleteModalRecord").fadeIn();
        $("#deleteModalRecord .del-confirm-rec").data("id", logId);
    });

    $(".del-confirm-rec").on("click", function () {
        var logId = $(this).data("id");

        $.ajax({
            url: "/Admin/DeleteTimeLog", 
            type: "POST", 
            data: { id: logId }, 
            success: function (response) {
                alert(response.message); 
                location.reload(); 
            },
            error: function (xhr) {
                alert("Error deleting record: " + xhr.responseText);
            }
        });

        $("#deleteModalRecord").fadeOut(); 
    });

    $(".close, .del-close").on("click", function () {
        $("#deleteModalRecord").fadeOut();
    });

    $(window).on("click", function (event) {
        if ($(event.target).is("#deleteModalRecord")) {
            $("#deleteModalRecord").fadeOut();
        }
    });


});
