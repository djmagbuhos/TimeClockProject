using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TimeClock.Models;
using TimeClock.ViewModel;

namespace TimeClock.Services
{
    public interface IDataAccessService
    {
        Task<List<VMEmployee>> GetAllEmployeeAsync();
        Task<VMEmployee> GetEmployeeByIdAsync(int Id);

        Task<VMEmployee> AddEmployeeAsync(VMEmployee model);
        Task<VMEmployee> UpdateEmployeeAsync(VMEmployee model);
        Task<bool> DeleteEmployeeAsync(int Id);

        //
        Task<List<VMUsers>> GetAllUsersAsync();
        Task<VMUsers> GetAllUsersByIdAsync(int Id);
        //
        Task<List<VMTimeLogs>> GetAllTimeLogsAsync();
        Task<VMTimeLogs> GetAllTimeLogsByIdAsync(int Id);
        //
        Task<List<VMStatus>> GetAllStatusAsync();
        Task<VMStatus> GetAllStatusByIdAsync(int Id);
        //
        Task<List<VMPosition>> GetAllPositionAsync();
        Task<VMPosition> GetAllPositionByIdAsync(int Id);
        //
        Task<List<VMRoles>> GetAllRolesAsync();
        Task<VMRoles> GetAllRolesByIdAsync(int Id);      




    }

    public class DataAccessService : IDataAccessService
    {
        private readonly IMemoryCache _cache;
        private readonly ApplicationDbContext _context;

        public DataAccessService(ApplicationDbContext context, IMemoryCache cache)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        //-------------GET ALL EMPLOYEES------------------------------------------//

        public async Task<List<VMEmployee>> GetAllEmployeeAsync()
        {
            var dataList = await _context.Employee
                .Join(_context.Position,
                      employee => employee.PositionID,
                      position => position.Id,
                      (employee, position) => new { employee, position }) // Anonymous type to hold both entities
                .Select(m => new VMEmployee()
                {
                    Id = m.employee.Id,
                    FirstName = m.employee.FirstName,
                    LastName = m.employee.LastName,
                    DateOfBirth = m.employee.DateOfBirth,
                    Gender = m.employee.Gender,
                    EmployeeName = m.employee.FirstName + " " + m.employee.LastName,
                    PositionName = m.position.description,
                    ProfilePicture = m.employee.ProfilePicture
                })
                .ToListAsync();

            return dataList;
        }

        //------------------------------------------------------------------------//

        public async Task<VMEmployee> GetEmployeeByIdAsync(int Id)
        {
            var data = await (from employee in _context.Employee
                              select employee).Where(x => x.Id == Id)
                    .Select(m => new VMEmployee()
                    {
                        Id = m.Id,
                        FirstName = m.FirstName,
                        LastName = m.LastName,
                        EmployeeName = m.FirstName + " " + m.LastName
                    }
                    )
                    .SingleOrDefaultAsync();

            return data;

        }

        public async Task<List<VMUsers>> GetAllUsersAsync() 
        {
            var data = await _context.Users
                .Select(u => new VMUsers()
                { 
                    Id = u.Id,
                    EmpId = u.EmpId,
                    Email = u.Email,
                    UserName = u.UserName,
                    CreatedAt = u.CreatedAt,
                    PasswordHash = u.PasswordHash
                }
                )
                .ToListAsync();

            return data;
        }

        public async Task<VMUsers> GetAllUsersByIdAsync(int Id)
        {
            var data = await _context.Users.Where(x => x.Id == Id)
                .Select(u => new VMUsers()
                {
                    Id = u.Id,
                    EmpId = u.EmpId,
                    Email = u.Email,
                    UserName = u.UserName,
                    CreatedAt = u.CreatedAt,
                    PasswordHash = u.PasswordHash
                }
                )
                .SingleOrDefaultAsync();

            return data;
        }

        public async Task<List<VMTimeLogs>> GetAllTimeLogsAsync()
        {
            var data = await _context.TimeLogs
                .Select(t => new VMTimeLogs()
                {
                   Id = t.Id,
                   EmpId= t.EmpId,
                   LogDate = t.LogDate,
                   TimeIN = t.TimeIN,
                   TimeOUT = t.TimeOUT,
                   StatusID = t.StatusID,
                   Total = t.Total,
                }
                )
                .ToListAsync();

            return data;
        }

        public async Task<VMTimeLogs> GetAllTimeLogsByIdAsync(int Id)
        {
            var data = await _context.TimeLogs
                .Select(t => new VMTimeLogs()
                {
                    Id = t.Id,
                    EmpId = t.EmpId,
                    LogDate = t.LogDate,
                    TimeIN = t.TimeIN,
                    TimeOUT = t.TimeOUT,
                    StatusID = t.StatusID,
                    Total = t.Total,
                }
                )
                .SingleOrDefaultAsync();

            return data;
        }

        public async Task<List<VMStatus>> GetAllStatusAsync()
        {
            var data = await _context.Status
                .Select(s => new VMStatus()
                {
                    Id = s.Id,
                    StatusName = s.StatusName,
                }
                )
                .ToListAsync();

            return data;
        }

        public async Task<VMStatus> GetAllStatusByIdAsync(int Id)
        {
            var data = await _context.Status
                .Select(s => new VMStatus()
                {
                    Id = s.Id,
                    StatusName = s.StatusName,
                }
                )
                .SingleOrDefaultAsync();

            return data;
        }
        public async Task<List<VMPosition>> GetAllPositionAsync()
        {
            var data = await _context.Position
                .Select(p => new VMPosition()
                {
                    Id = p.Id,
                    description = p.description,
                }
                )
                .ToListAsync();

            Console.WriteLine($"Positions Count: {data.Count}"); // Debugging

            return data;
        }
        public async Task<VMPosition> GetAllPositionByIdAsync(int Id)
        {
            var data = await _context.Position
                .Select(p => new VMPosition()
                {
                    Id = p.Id,
                    description = p.description,
                }
                )
                .SingleOrDefaultAsync();

            return data;

        }

        public async Task<List<VMRoles>> GetAllRolesAsync()
        {
            var data = await _context.Roles
                .Select(r => new VMRoles()
                {
                    Id = r.Id,
                    description = r.description,
                }
                )
                .ToListAsync();

            return data;
        }
        public async Task<VMRoles> GetAllRolesByIdAsync(int id)
        {
            var data = await _context.Roles
                .Select(r => new VMRoles()
                {
                    Id = r.Id,
                    description = r.description,
                }
                )
                .SingleOrDefaultAsync();

            return data;
        }

        public async Task<bool> DeleteEmployeeAsync(int Id)
        {
            int result = 0;
            var data = await _context.Employee.FindAsync(Id);
            if (data != null)
            {
                var deleteResult = _context.Employee.Remove(data);
                result = await _context.SaveChangesAsync();
            }

            return (result > 0);
        }


        [HttpPost]
        public async Task<VMEmployee> UpdateEmployeeAsync(VMEmployee model)
        {
            // Fetch the existing employee from the database to retain the current profile picture
            var existingEmployee = await _context.Employee.FirstOrDefaultAsync(e => e.Id == model.Id);

            // If no new profile picture is provided, keep the existing one
            if (model.ProfilePicture == null || model.ProfilePicture.Length == 0)
            {
                model.ProfilePicture = existingEmployee.ProfilePicture;
            }

            // Map the updated values to the employee object
            existingEmployee.FirstName = model.FirstName;
            existingEmployee.LastName = model.LastName;
            existingEmployee.Gender = model.Gender;
            existingEmployee.PositionID = model.PositionID;
            existingEmployee.DateOfBirth = model.DateOfBirth;
            existingEmployee.ProfilePicture = model.ProfilePicture; 
            existingEmployee.CreatedAt = DateTime.Now;

            _context.Employee.Update(existingEmployee);
            await _context.SaveChangesAsync();

            return model;
        }


        [HttpPost]
        public async Task<VMEmployee> AddEmployeeAsync(VMEmployee model)
        {
            var data = new Employee()
            {
                //Id = 0,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Gender = model.Gender,
                PositionID = model.PositionID,
                DateOfBirth = model.DateOfBirth,
                //CreatedAt = model.CreatedAt,
                CreatedAt = DateTime.Now,
                ProfilePicture = model.ProfilePicture
            };

            await _context.Employee.AddAsync(data);
            await _context.SaveChangesAsync();
            model.Id = data.Id;

            return model;
        }


    }
}
