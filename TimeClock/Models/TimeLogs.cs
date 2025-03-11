using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Threading;

namespace TimeClock.Models
{
    public class TimeLogs
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(Employee))]
        public int? EmpId { get; set; }

        public DateTime LogDate { get; set; } = DateTime.Now;

        public DateTime? TimeIN { get; set; }
        public DateTime? TimeOUT { get; set; }

        [ForeignKey("Status")]
        public int? StatusID { get; set; }

        public decimal? TotalHours => (TimeIN != null && TimeOUT != null)
            ? (decimal?)(TimeOUT - TimeIN).Value.TotalHours
            : null;

        public decimal? Total { get; internal set; }
    }
}
