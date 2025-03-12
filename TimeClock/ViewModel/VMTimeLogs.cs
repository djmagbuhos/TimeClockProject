using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using TimeClock.Models;

namespace TimeClock.ViewModel
{
    public class VMTimeLogs
    {
        public int Id { get; set; }

        public int? EmpId { get; set; }

        public DateTime LogDate { get; set; }

        public DateTime? TimeIN { get; set; }
        public DateTime? TimeOUT { get; set; }

        public int? StatusID { get; set; }

        public decimal? Total {  get; set; }

        public string EmployeeName { get; set; }
        public string StatusDescription { get; set; }
    }

    public class EditTimeLogVM
    {
        public int Id { get; set; }
        public DateTime? TimeIN { get; set; }
        public DateTime? TimeOUT { get; set; }
        public int StatusID { get; set; }
    }

}
