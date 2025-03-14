using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TimeClock.Models;
using TimeClock.ViewModel;
using System.Security.Cryptography;
using System.Text;

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
        Task<VMUsers> UpdateUsersAsync(VMUsers model);

        Task<bool> DeleteUsersAsync(int Id);
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


        //-------------HOMEPAGE----------------------//
        Task<VMUsers?> ValidateUserAsync(string username, string password);
        Task<TimeLogs?> GetActiveClockIn(int empId);
        Task UpdateClockOut(TimeLogs timeLog);
        Task<List<VMTimeLogs>> GetTodaysLogsAsync(int empId);




        //------------EMPLOYEE SIDE------------------//
        Task<IEnumerable<VMTimeLogs>> GetTimeLogsByEmployeeIdAsync(int empId);
        Task<IEnumerable<VMTimeLogs>> GetFilteredTimeLogsAsync(int empId, int? month, int? day, int? year);
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
            try
            {
                var data = await _context.Employee.FindAsync(Id);
                if (data == null)
                {
                    Console.WriteLine("Employee not found: " + Id);
                    return false;
                }

                _context.Employee.Remove(data);
                int result = await _context.SaveChangesAsync();

                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting employee: " + ex.Message);
                return false;
            }
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
            var data = await _context.Users
                .Where(x => x.Id == Id)
                .Select(u => new VMUsers()
                {
                    Id = u.Id,
                    EmpId = u.EmpId,
                    Email = u.Email,
                    UserName = u.UserName,
                    CreatedAt = u.CreatedAt,
                    PasswordHash = u.PasswordHash
                })
                .SingleOrDefaultAsync();

            if (data == null)
            {
                Console.WriteLine($"No user found with ID: {Id}");
            }
            else
            {
                Console.WriteLine($"User found: {data.Email}");
            }

            return data;
        }

        public async Task<bool> DeleteUsersAsync(int Id)
        {
            int result = 0;
            var data = await _context.Users.FindAsync(Id);
            if (data != null)
            {
                _context.Users.Remove(data);
                result = await _context.SaveChangesAsync();
            }

            return (result > 0);
        }


        public async Task<VMUsers> UpdateUsersAsync(VMUsers model)
        {
            //var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == model.Id);

            var existingUser = await _context.Users.Where(u => u.Id == model.Id).FirstOrDefaultAsync();
            if (existingUser == null)
            {
                return null; // Return null kung walang user na nahanap
            }

            var role = await _context.Roles.Where(r => r.Id == model.RoleId).FirstOrDefaultAsync();
            if (role != null)
            {
                existingUser.RoleId = role.Id;
                existingUser.Email = model.Email;

                _context.Users.Update(existingUser);
                await _context.SaveChangesAsync();

                return model;
            }

            return null;
        }



        public async Task<VMUsers> AddUsersAsync(VMUsers model)
        {
            // Check if the employee exists in the Employee table
            var existingEmployee = await _context.Employee.FirstOrDefaultAsync(e => e.Id == model.EmpId);
            if (existingEmployee == null)
            {
                throw new InvalidOperationException("Unable to create account. This Employee ID does not exist.");
            }

            // Check if EmpId already has a user
            var existingEmpId = await _context.Users.FirstOrDefaultAsync(u => u.EmpId == model.EmpId);
            if (existingEmpId != null)
            {
                throw new InvalidOperationException($"An account already exists for Employee ID {model.EmpId}.");
            }

            // Check if UserName already exists
            var existingUserName = await _context.Users.FirstOrDefaultAsync(u => u.UserName == model.UserName);
            if (existingUserName != null)
            {
                throw new InvalidOperationException($"Username '{model.UserName}' is already taken. Please choose another one.");
            }

            // Check if Email already exists
            var existingEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (existingEmail != null)
            {
                throw new InvalidOperationException($"Email '{model.Email}' is already registered. Use a different email.");
            }

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
                .ThenBy(t => t.TimeIN)
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


        //--------------------------HOMEPAGE----------------------------------//
        public async Task<List<VMTimeLogs>> GetTodaysLogsAsync(int empId)
        {
            var today = DateTime.Today;

            var logs = await _context.TimeLogs
                .Where(t => t.EmpId == empId && t.LogDate == today)
                .Select(t => new VMTimeLogs
                {
                    LogDate = t.LogDate,
                    TimeIN = t.TimeIN,
                    TimeOUT = t.TimeOUT,
                    Total = t.TimeOUT.HasValue && t.TimeIN.HasValue
                        ? (decimal?)Math.Round((decimal)(t.TimeOUT.Value - t.TimeIN.Value).TotalHours, 2)
                        : null // Set null if no TimeOUT
                })
                .ToListAsync();

            // Format date/time in memory
            logs.ForEach(t =>
            {
                t.DateFormatted = t.LogDate.ToString("yyyy-MM-dd");
                t.TimeInFormatted = t.TimeIN.HasValue ? t.TimeIN.Value.ToString("HH:mm:ss") : "";
                t.TimeOutFormatted = t.TimeOUT.HasValue ? t.TimeOUT.Value.ToString("HH:mm:ss") : "";
            });

            return logs ?? new List<VMTimeLogs>();  // Ensure it never returns null
        }


        public async Task<VMUsers?> ValidateUserAsync(string username, string password)
        {
            // Hash the input password before comparison
            string hashedPassword = HashPassword(password);

            var user = await (from u in _context.Users
                              join e in _context.Employee on u.EmpId equals e.Id
                              join p in _context.Position on e.PositionID equals p.Id
                              where u.UserName == username && u.PasswordHash == hashedPassword
                              select new VMUsers
                              {
                                  EmpId = u.EmpId,
                                  RoleId = (int)u.RoleId,
                                  EmployeeName = e.FirstName + " " + e.LastName,
                                  PositionName = p.description 
                              }).FirstOrDefaultAsync();

            return user;
        }


        // Reuse the same HashPassword function as in AdminController
        private static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hashBytes = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        public async Task<TimeLogs?> GetActiveClockIn(int empId)
        {
            return await _context.TimeLogs
                .Where(t => t.EmpId == empId && t.LogDate == DateTime.Today && t.TimeOUT == null)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateClockOut(TimeLogs timeLog)
        {
            _context.TimeLogs.Update(timeLog);
            await _context.SaveChangesAsync();
        }


        //-----------------EMPLOYEE LOGIN SIDE--------------------------//
        public async Task<IEnumerable<VMTimeLogs>> GetTimeLogsByEmployeeIdAsync(int empId)
        {
            return await _context.TimeLogs
                .Where(t => t.EmpId == empId)
                .OrderBy(t => t.LogDate)
                .ThenBy(t => t.TimeIN)
                .Select(t => new VMTimeLogs()
                {
                    LogDate = t.LogDate,
                    TimeIN = t.TimeIN,
                    TimeOUT = t.TimeOUT,
                    Total = t.TimeOUT != null && t.TimeIN != null
                        ? (decimal)(t.TimeOUT.Value - t.TimeIN.Value).TotalHours
                        : 0 // If no TimeOUT, set to 0
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<VMTimeLogs>> GetFilteredTimeLogsAsync(int empId, int? month, int? day, int? year)
        {
            var query = _context.TimeLogs
                .Where(t => t.EmpId == empId);

            if (year.HasValue)
                query = query.Where(t => t.LogDate.Year == year.Value);

            if (month.HasValue && month.Value != 0)
                query = query.Where(t => t.LogDate.Month == month.Value);

            if (day.HasValue && day.Value != 0)
                query = query.Where(t => t.LogDate.Day == day.Value);

            return await query
                .OrderByDescending(t => t.LogDate)
                .Select(t => new VMTimeLogs
                {
                    LogDate = t.LogDate,
                    TimeIN = t.TimeIN,
                    TimeOUT = t.TimeOUT,
                    Total = t.TimeOUT.HasValue && t.TimeIN.HasValue
                        ? (decimal)(t.TimeOUT.Value - t.TimeIN.Value).TotalHours
                        : 0
                })
                .ToListAsync();
        }


    }
}
