using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using TimeClock.Models;

namespace TimeClock.ViewModel
{
    public class VMUsers
    {

        public int Id { get; set; }

        public int? EmpId { get; set; }

        public string RoleName { get; set; }
        public int RoleId { get; set; }

        [Required]
        [MaxLength(100)]
        public string EmployeeName { get; set; }

        [Required]
        [MaxLength(50)]
        public string UserName { get; set; }

        [Required]
        [MaxLength(100)]
        public string PasswordHash { get; set; }

        [Required]
        [MaxLength(50)]
        public string Email { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

    }
}
