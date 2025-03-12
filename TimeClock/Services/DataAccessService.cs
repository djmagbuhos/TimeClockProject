using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TimeClock.Models;
using TimeClock.ViewModel;

namespace TimeClock.Services
{
    public interface IDataAccessService
    {
        //-------------------EMPLOYEE--------------------//
        Task<List<VMEmployee>> GetAllEmployeeAsync();
        Task<VMEmployee> GetEmployeeByIdAsync(int Id);

        Task<VMEmployee> AddEmployeeAsync(VMEmployee model);
        Task<VMEmployee> UpdateEmployeeAsync(VMEmployee model);
        Task<bool> DeleteEmployeeAsync(int Id);
        //---------------------------------------------//

        //---------------------USER--------------------//
        Task<List<VMUsers>> GetAllUsersAsync();
        Task<VMUsers> GetAllUsersByIdAsync(int Id);
        Task<VMUsers> AddUsersAsync(VMUsers model);
        //--------------------------------------------//

        //------------------TIMELOGS-----------------//
        Task<List<VMTimeLogs>> GetAllTimeLogsAsync(string empId = null, string month = null, string day = null, string year = null);
        Task<bool> AddTimeLogs(VMTimeLogs model);
        Task<bool> UpdateTimeLogs(EditTimeLogVM model);
        Task<bool> DeleteTimeLogAsync(int id);
        //-------------------------------------------//


        //-----------------STATUS--------------------//
        Task<List<VMStatus>> GetAllStatusAsync();
        Task<VMStatus> GetAllStatusByIdAsync(int Id);
        //-------------------------------------------//


        //----------------POSITION--------------------//
        Task<List<VMPosition>> GetAllPositionAsync();
        Task<VMPosition> GetAllPositionByIdAsync(int Id);
        //--------------------------------------------//

        //----------------ROLES------------------------//
        Task<List<VMRoles>> GetAllRolesAsync();
        Task<VMRoles> GetAllRolesByIdAsync(int Id);
        //--------------------------------------------//

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
        //-------------EMPLOYEE---------------------------------------------------//
        //------------------------------------------------------------------------//

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


        //------------END EMPLOYEE------------------------------------------------//
        //------------------------------------------------------------------------//


        //---------------------------------------------------------------------//
        //----------------------- USER ----------------------------------------//
        public async Task<List<VMUsers>> GetAllUsersAsync()
        {
            var data = await (from user in _context.Users
                              join employee in _context.Employee on user.EmpId equals employee.Id
                              join role in _context.Roles on user.RoleId equals role.Id
                              select new VMUsers
                              {
                                  Id = user.Id,
                                  EmpId = employee.Id, // Employee ID
                                  EmployeeName = employee.LastName + ", " + employee.FirstName, // Full Name
                                  RoleName = role.description, // Role ID
                                  Email = user.Email // Email
                              }).ToListAsync();

            Console.WriteLine("Fetched Users Count: " + data.Count);
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

        public async Task<bool> DeleteUsersAsync(int Id)
        {
            int result = 0;
            var data = await _context.Users.FindAsync(Id);
            if (data != null)
            {
                var deleteResult = _context.Users.Remove(data);
                result = await _context.SaveChangesAsync();
            }

            return (result > 0);

        }


        public async Task<VMUsers> UpdateUsersAsync(VMUsers model)
        {
            var data = new Users()
            {
                RoleId = model.RoleId,
                EmpId = model.EmpId,
                UserName = model.UserName,
                PasswordHash = model.PasswordHash,
                Email = model.Email,
                CreatedAt = DateTime.Now
            };
            _context.Users.Update(data);
            await _context.SaveChangesAsync();

            return model;
        }

        [HttpPost]
        public async Task<VMUsers> AddUsersAsync(VMUsers model)
        {
            var data = new Users()
            {
                RoleId = model.RoleId,
                EmpId = model.EmpId,
                UserName = model.UserName,
                PasswordHash = model.PasswordHash,
                Email = model.Email,
                CreatedAt = DateTime.Now
            };

            await _context.Users.AddAsync(data);
            await _context.SaveChangesAsync();
            model.Id = data.Id;

            return model;
        }

        //-------------------------END USER------------------------------------//
        //---------------------------------------------------------------------//

        //-------------------------------------------------------------------------//
        //------------------RECORD LIST (ADMIN)-----------------------------------//

        public async Task<List<VMTimeLogs>> GetAllTimeLogsAsync(string empId = null, string month = null, string day = null, string year = null)
        {
            IQueryable<TimeLogs> query = _context.TimeLogs;

            if (!string.IsNullOrEmpty(empId) && int.TryParse(empId, out int empIdInt))
            {
                query = query.Where(t => t.EmpId == empIdInt);
            }

            if (!string.IsNullOrEmpty(month) && month != "all")
            {
                int monthValue = int.Parse(month);
                query = query.Where(t => t.LogDate.Month == monthValue);
            }

            if (!string.IsNullOrEmpty(day) && day != "all")
            {
                int dayValue = int.Parse(day);
                query = query.Where(t => t.LogDate.Day == dayValue);
            }

            if (!string.IsNullOrEmpty(year) && year != "all")
            {
                int yearValue = int.Parse(year);
                query = query.Where(t => t.LogDate.Year == yearValue);
            }

            var data = await query
                .OrderBy(t => t.LogDate)
                .Select(t => new VMTimeLogs()
                {
                    Id = t.Id,
                    EmpId = t.EmpId,
                    LogDate = t.LogDate,
                    TimeIN = t.TimeIN,
                    TimeOUT = t.TimeOUT,
                    StatusID = t.StatusID,
                    Total = t.Total,
                    EmployeeName = _context.Employee.Where(e => e.Id == t.EmpId).Select(e => e.FirstName + " " + e.LastName).FirstOrDefault(),
                    StatusDescription = _context.Status.Where(s => s.Id == t.StatusID).Select(s => s.StatusName).FirstOrDefault()
                })
                .ToListAsync();

            return data;
        }

        [HttpPost]
        public async Task<bool> AddTimeLogs(VMTimeLogs model)
        {
            if (model == null)
            {
                return false;
            }

            // Ensure LogDate is today or a past date
            if (model.LogDate > DateTime.Today)
            {
                return false; 
            }

            // Ensure TimeOUT is greater than TimeIN
            if (model.TimeOUT <= model.TimeIN)
            {
                return false; 
            }

            var timeLog = new TimeLogs()
            {
                EmpId = model.EmpId,
                LogDate = model.LogDate,
                TimeIN = model.TimeIN,
                TimeOUT = model.TimeOUT,
                StatusID = model.StatusID
            };

            await _context.TimeLogs.AddAsync(timeLog);
            await _context.SaveChangesAsync();

            return true; // Successfully added the record
        }

        public async Task<bool> UpdateTimeLogs(EditTimeLogVM model)
        {
            try
            {
                var timeLog = await _context.TimeLogs.FindAsync(model.Id);
                if (timeLog == null)
                {
                    return false;
                }

                timeLog.TimeIN = model.TimeIN;
                timeLog.TimeOUT = model.TimeOUT;
                timeLog.StatusID = model.StatusID;

                _context.TimeLogs.Update(timeLog);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateTimeLogs: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteTimeLogAsync(int id)
        {
            var timeLog = await _context.TimeLogs.FindAsync(id);
            if (timeLog == null) return false;

            _context.TimeLogs.Remove(timeLog);
            await _context.SaveChangesAsync();
            return true;
        }



        //---------------------END RECORD LIST (ADMIN)--------------------------//
        //---------------------------------------------------------------------//


        //-------------------    STATUS     ----------------------------------//
        //---------------------------------------------------------------------//
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
        //--------------------------END STATUS---------------------------------//
        //---------------------------------------------------------------------//

        //---------------------------------------------------------------------//
        //--------------------    POSITION    --------------------------------//
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
        //---------------------------END POSITION------------------------------//
        //---------------------------------------------------------------------//

        //--------------------------- ROLES -----------------------------------//
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
        //----------------------------END ROLES--------------------------------//
        //---------------------------------------------------------------------//


    }
}
