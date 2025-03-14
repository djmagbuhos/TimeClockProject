using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace TimeClock.Models
{
    [Index(nameof(UserName), IsUnique = true)]
    [Index(nameof(Email), IsUnique = true)]
    [Index(nameof(EmpId), IsUnique = true)]
    public class Users
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(Roles))]
        public int? RoleId { get; set; }

        [ForeignKey(nameof(Employee))]
        public int EmpId { get; set; }


        [Required]
        [MaxLength(50)]
        public string UserName { get; set; }

        [Required]
        [MaxLength(50)]
        public string PasswordHash { get; set; }

        [Required]
        [MaxLength(50)]
        public string Email { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

}
