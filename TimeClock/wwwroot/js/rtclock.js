
$(document).ready(function () {
    function updateDateTime() {
        let now = new Date();

        // Time (24-hour format)
        let hours = now.getHours();
        let minutes = now.getMinutes();
        let seconds = now.getSeconds();

        // Date (YYYY-MM-DD)
        let year = now.getFullYear();
        let month = now.getMonth() + 1; // Months are 0-based
        let day = now.getDate();

        // Get day name (short format: MON, TUE, WED, etc.)
        let days = ["SUN", "MON", "TUE", "WED", "THU", "FRI", "SAT"];
        let dayName = days[now.getDay()];

        // Add leading zeros if needed
        hours = hours < 10 ? "0" + hours : hours;
        minutes = minutes < 10 ? "0" + minutes : minutes;
        seconds = seconds < 10 ? "0" + seconds : seconds;
        month = month < 10 ? "0" + month : month;
        day = day < 10 ? "0" + day : day;

        // Format Date
        let formattedDate = `${year}-${month}-${day} ${dayName}`;
        let formattedTime = `${hours}:${minutes}:${seconds}`;

        // Update the elements
        $(".real-time-clock").text(formattedTime);
        $(".real-time-date").text(formattedDate);
    }

    // Update every second
    setInterval(updateDateTime, 1000);
    updateDateTime(); // Call immediately to prevent delay
});

