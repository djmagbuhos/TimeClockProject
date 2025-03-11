using System.ComponentModel.DataAnnotations;

namespace TimeClock.ViewModel
{
    public class VMRoles
    {
        public int Id { get; set; }


        [Required]
        [MaxLength(50)]
        public string description { get; set; }
    }
}
