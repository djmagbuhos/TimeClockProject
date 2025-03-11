using System.ComponentModel.DataAnnotations;

namespace TimeClock.ViewModel
{    
    public class VMEmployee
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }

        [Required]
        [MaxLength(100)]
        public string EmployeeName { get; set; }

        [Required]
        [MaxLength(10)]

        public string Gender { get; set; }

        public int PositionID { get; set; }

        public DateTime CreatedAt { get; set; }

        [Required]
        public DateOnly DateOfBirth { get; set; }
        public string PositionName { get; set; }

        public byte[]? ProfilePicture { get; set; }
    }


}
