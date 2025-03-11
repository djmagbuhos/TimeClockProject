using System.ComponentModel.DataAnnotations;

namespace TimeClock.ViewModel
{
    public class VMStatus
    {
       
        public int Id { get; set; }


        [Required]
        [MaxLength(50)]
        public string StatusName { get; set; }
    }
}
