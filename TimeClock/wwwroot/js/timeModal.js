function fetchTodaysLogs() {
    $.ajax({
        url: "/Home/GetTodaysLogs",
        type: "GET",
        dataType: "json",
        success: function (data) {
            console.log("Fetched data:", data); // Debugging output
            let tableContainer = $("#table-container");
            let table = $(".hc-table");
            let noRecordText = $("#home-norec");

            // Handle no records
            if (!data || data.length === 0) {
                table.find("tbody").html('<tr><td colspan="4">No Active Records</td></tr>');
                table.show();
                return;
            } else {
                table.show();
                noRecordText.hide();
            }

            // Ensure table structure exists
            if (!table.length) {
                tableContainer.html(`
                    <table class="hc-table">
                        <thead>
                            <tr>
                                <th>Date</th>
                                <th>Time In</th>
                                <th>Time Out</th>
                                <th>Total</th>
                            </tr>
                        </thead>
                        <tbody></tbody>
                    </table>
                `);
                table = $(".hc-table");
            } else {
                table.find("tbody").empty(); // Clear existing rows
            }

            let tableBody = table.find("tbody");

            let formatTime = (dateTimeStr) => {
                if (!dateTimeStr) return "-";
                let dateObj = new Date(dateTimeStr);
                return dateObj.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" });
            };

            data.forEach(log => {
                let date = new Date(log.logDate).toISOString().split("T")[0];
                let timeIn = formatTime(log.timeIN);
                let timeOut = log.timeOUT ? formatTime(log.timeOUT) : "-";
                let total = log.total !== null ? log.total.toFixed(2) : "-";

                let row = `<tr>
                            <td>${date}</td>
                            <td>${timeIn}</td>
                            <td>${timeOut}</td>
                            <td>${total}</td>
                          </tr>`;
                tableBody.append(row);
            });
        },
        error: function (xhr, status, error) {
            console.error("Error fetching logs:", error);
        }
    });
}



$(document).ready(function () {
    // Function to clear input fields inside a modal
    function clearModalInputs(modalId) {
        $(modalId).find("input[type='text'], input[type='password']").val("");
    }

    //LOG IN MODAL
    if (sessionStorage.getItem("EmpId") && sessionStorage.getItem("RoleId") == "2") {
        fetchTodaysLogs();
    };

    //LOG IN SITE (NAV PART)
    $("#loginSiteModal").hide();

    $("#home-loginlink").on("click", function () {
        $("#loginSiteModal").fadeIn();
    })

    $("#loginSiteModal input[type='submit']").on("click", function () {
        var username = $("#site-username").val();
        var password = $("#site-password").val();

        $.ajax({
            url: "/Home/LoginSite",
            type: "POST",
            data: { username: username, password: password },
            success: function (response) {
                sessionStorage.clear();

                // Store session data
                sessionStorage.setItem("EmpId", response.empId);
                sessionStorage.setItem("RoleId", response.roleId);
                sessionStorage.setItem("EmployeeName", response.employeeName);
                sessionStorage.setItem("PositionName", response.positionName);

                if (response.redirectUrl) {
                    window.location.href = response.redirectUrl;
                } else {
                    location.reload(); // Reload to reflect changes for non-admin users
                }
            },
            error: function (xhr) {
                alert(xhr.responseJSON.message);
            }
        });
    });

    $(".close, #loginSiteModal").on("click", function (event) {
        if ($(event.target).is("#loginSiteModal") || $(event.target).hasClass("close")) {
            $("#loginSiteModal").fadeOut();
            clearModalInputs("#loginSiteModal"); // Clear when modal closes
        }
    });

    // LOG IN MODAL
    $("#loginModal").hide();

    $(".checkin-btn").on("click", function () {
        // Clear session on Clock-In button click
        clearModalInputs("#loginModal");
        $("#loginModal").fadeIn();
 
    });

    $(".close, #loginModal").on("click", function (event) {
        if ($(event.target).is("#loginModal") || $(event.target).hasClass("close")) {
            $("#loginModal").fadeOut();
            clearModalInputs("#loginModal"); 
        }
    });

    $("#login-btn").on("click", function () {
        var username = $("#ci-username").val();
        var password = $("#ci-password").val();

        $.ajax({
            url: "/Home/Login",
            type: "POST",
            data: { username: username, password: password },
            success: function (response) {
                sessionStorage.clear();


                // Store session data
                sessionStorage.setItem("EmpId", response.empId);
                sessionStorage.setItem("RoleId", response.roleId);
                sessionStorage.setItem("EmployeeName", response.employeeName);
                sessionStorage.setItem("PositionName", response.positionName);

                // Close modal and clear fields
                $("#loginModal").fadeOut();
                clearModalInputs("#loginModal");

                // Fetch logs after successful login
                fetchTodaysLogs();

                if (response.redirectUrl) {
                    window.location.href = response.redirectUrl;
                } else {
                    window.location.href = window.location.href; 
                }
            },
            error: function (xhr) {
                alert(xhr.responseJSON.message);
            }
        });
    });


    // LOG OUT MODAL
    $("#logoutModal").hide();

    $(".checkout-btn").on("click", function () {
        clearModalInputs("#logoutModal"); // Clear fields BEFORE opening modal
        $("#logoutModal").fadeIn();
    });

    $("#logoutModal input[type='submit']").on("click", function () {
        var username = $("#co-username").val();
        var password = $("#co-password").val();

        $.ajax({
            url: "/Home/ClockOut",
            type: "POST",
            data: { username: username, password: password },
            success: function (response) {
                alert(response.message);

                // Store session data for the logged-in user
                sessionStorage.setItem("EmpId", response.empId);
                sessionStorage.setItem("RoleId", response.roleId);
                sessionStorage.setItem("EmployeeName", response.employeeName);
                sessionStorage.setItem("PositionName", response.positionName);

                // Reload page to refresh the time logs
                location.reload();
            },
            error: function (xhr) {
                alert(xhr.responseJSON.message);
            }
        });
    });




    $(".close, #logoutModal").on("click", function (event) {
        if ($(event.target).is("#logoutModal") || $(event.target).hasClass("close")) {
            $("#logoutModal").fadeOut();
            clearModalInputs("#logoutModal"); // Clear fields when modal closes
        }
    });


    //VIEW EMPLOYEE EDIT EMPLOYEE MODAL
    $("#viewprofile").hide();

    $(".el-view-btn").click(function () {
        // Get data
        const employeeId = $(this).attr("data-id");
        const firstName = $(this).attr("data-firstname");
        const lastName = $(this).attr("data-lastname");
        const position = $(this).attr("data-position");
        const dob = $(this).attr("data-dob");
        const gender = $(this).attr("data-gender");
        const profilePicture = $(this).attr("data-pfp"); 

        // Set data
        $(".vp-id").text(employeeId);  // Display ID in the modal view section
        $(".vp-firstname").text(firstName);
        $(".vp-lastname").text(lastName);
        $(".vp-position").text(position);
        $(".vp-dob").text(dob);
        $(".vp-gender").text(gender);
        $(".vp-img").attr("src", profilePicture); 

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
    $("#deleteModalEmployee").hide();

    // Show delete modal
    $(".el-del-btn").on("click", function () {
        const employeeId = $(this).data("id");
        const firstName = $(this).data("firstname");
        const lastName = $(this).data("lastname");
        const fullName = lastName + ", " + firstName;
        $(".del-empname").text(fullName);

        // Save the emp ID
        $("#deleteModalEmployee").data("employee-id", employeeId);

        $("#deleteModalEmployee").fadeIn();
    });

    // Close modal
    $(".close, .del-close").on("click", function () {
        $("#deleteModalEmployee").fadeOut();
    });

    $(window).on("click", function (event) {
        if ($(event.target).is("#deleteModalEmployee")) {
            $("#deleteModalEmployee").fadeOut();
        }
    });

    // Confirm delete
    $(".del-confirm-employee").on("click", function () {
        const employeeId = $("#deleteModalEmployee").data("employee-id");

        $.ajax({
            url: "/Admin/DeleteEmployee",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(employeeId),  // Pass ID directly
            success: function (response) {
                if (response.success) {
                    alert("Employee deleted successfully!");
                    location.reload();
                } else {
                    alert("Error: " + response.message);
                }
            },
            error: function (xhr, status, error) {
                console.log(xhr.responseText);  // Log error for debugging
                alert("Failed to delete employee: " + xhr.responseText);
            }
        });

        $("#deleteModalEmployee").fadeOut();
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

    /////////----------------------------------------------------

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


    //////DELETE USER MODAL

    $("#deleteModalUser").hide();

    $(".userlist-delete").on("click", function () {
        $("#deleteModalUser").fadeIn();
    });


    $(".close").on("click", function () {
        $("#deleteModalUser").fadeOut();
    });

    $(window).on("click", function (event) {
        if ($(event.target).is("#deleteModalUser")) {
            $("#deleteModalUser").fadeOut();
        }
    });

});
