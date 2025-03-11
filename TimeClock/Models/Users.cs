using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TimeClock.Models
{
    public class Users
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        

        [ForeignKey(nameof(Roles))]
        public int? RoleId { get; set; }

        [ForeignKey(nameof(Employee))]
        public int? EmpId { get; set; }


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
