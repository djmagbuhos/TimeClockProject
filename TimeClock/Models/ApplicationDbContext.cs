using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace TimeClock.Models
{
    public class ApplicationDbContext : DbContext
    {
        protected readonly IConfiguration _configuration;
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> contextOptions, IConfiguration configuration) : base(contextOptions)
        {
            _configuration = configuration;
        }

        public DbSet<Employee> Employee { get; set; }
        public DbSet<TimeLogs> TimeLogs { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<Roles> Roles { get; set; }
        public DbSet<Position> Position { get; set; }
        public DbSet<Status> Status { get; set; }



        protected override void OnModelCreating(ModelBuilder builder) 
        {
            base.OnModelCreating(builder);

            /*
            Employee empData = new Employee()
            {
                FirstName = "Juan",
                LastName = "dela Cruz1"
            };
            builder.Entity<Employee>().HasData(empData);

            empData = new Employee()
            {
                FirstName = "Juan",
                LastName = "dela Cruz2"
            };
            builder.Entity<Employee>().HasData(empData);

            empData = new Employee()
            {
                FirstName = "Juan",
                LastName = "dela Cruz3"
            };
            builder.Entity<Employee>().HasData(empData);

            empData = new Employee()
            {
                FirstName = "Juan",
                LastName = "dela Cruz4"
            };
            builder.Entity<Employee>().HasData(empData);
            */

        }


    }
}
