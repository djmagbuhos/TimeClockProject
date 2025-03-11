using System.ComponentModel.DataAnnotations;

namespace TimeClock.ViewModel
{
    public class VMPosition
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string description { get; set; }

    }
}
