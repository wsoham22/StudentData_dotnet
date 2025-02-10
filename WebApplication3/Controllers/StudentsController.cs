using Microsoft.AspNetCore.Mvc;
using System.Data;
using Dapper;
using MyAspNetCoreApp.Models;
using NLog;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication3.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentsController : ControllerBase
    {
        private readonly IDbConnection _dbConnection;
        private readonly IMemoryCache _cache;  // Injecting IMemoryCache
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger(); // NLog Logger

        public StudentsController(IDbConnection dbConnection, IMemoryCache cache)
        {
            _dbConnection = dbConnection;
            _cache = cache;
        }

        // Logging method with dynamic method name
        private void LogAction(string action, string details)
        {
            if (action == "Insert")
            {
                Logger.Trace($"{action} operation: {details}");
            }
            else
            {
                Logger.Info($"{action} operation: {details}");
            }
        }

        // Logging method for async log insertion
        //private async Task LogActionAsync(string action, string details)
        //{
        //    if (action == "Insert" || action == "Update" || action == "Delete")
        //    {
        //        var log = new
        //        {
        //            Action = action,
        //            Details = details
        //        };

        //        await _dbConnection.ExecuteAsync(
        //            "CreateLog",  // Stored procedure name
        //            log,  // Object containing the correct parameters
        //            commandType: CommandType.StoredProcedure
        //        );
        //    }
        //}
        [HttpGet]
        public async Task<ApiResponse<IEnumerable<Student>>> GetStudents(
    string gender = null,
    string firstName = null,
    string lastName = null,
    string division = null,
    string teacherName = null,
    bool includeTeacherNames = true)
        {
            const string cacheKey = "students_list";  // Cache key to identify the data
            IEnumerable<Student> students;

            // Try to get the cached students list
            if (!_cache.TryGetValue(cacheKey, out students))
            {
                // Fetch students from the database with optional filter parameters
                students = await _dbConnection.QueryAsync<Student>(
                    "GetFilteredStudents",
                    new
                    {
                        Gender = gender,
                        FirstName = firstName,
                        LastName = lastName,
                        Division = division,
                        TeacherName = teacherName,
                        IncludeTeacherNames = includeTeacherNames
                    },
                    commandType: CommandType.StoredProcedure
                );

                // Log the data fetch
                Logger.Info("Fetched students from the database with applied filters.");

                // Cache the students list
                _cache.Set(cacheKey, students, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)  // Cache expires after 30 seconds
                });
            }
            else
            {
                // Log cache hit
                Logger.Info("Retrieved students from cache.");
            }

            // Return the filtered students list
            return ApiResponse<IEnumerable<Student>>.CreateSuccessResponse(students);
        }



        [HttpGet("{id}")]
        public async Task<ApiResponse<Student>> GetStudent(int id)
        {
            var student = await _dbConnection.QueryFirstOrDefaultAsync<Student>(
                "GetStudentById",
                new { Id = id },
                commandType: CommandType.StoredProcedure
            );

            if (student == null)
            {
                Logger.Warn($"StudentsController => GetStudent => No student found with ID: {id}");
                return ApiResponse<Student>.CreateErrorResponse(1, "Student not found");
            }

            Logger.Info($"StudentsController => GetStudent => Fetched details for student with ID: {id}");
            return ApiResponse<Student>.CreateSuccessResponse(student);
        }
        [HttpPost("createStudent")]
        public async Task<ApiResponse<Student>> Create([FromBody] Student student)
        {

            try
            {
                var parameters = new
                {
                    FirstName = student.FirstName,
                    LastName = student.LastName,
                    Email = student.Email,
                    DateOfBirth = student.DateOfBirth,
                    Gender = student.Gender,
                    Division = student.Division
                };

                var result = await _dbConnection.ExecuteAsync(
                    "AddStudent", 
                    parameters,   
                    commandType: CommandType.StoredProcedure
                );

                // Invalidate the cache (if applicable)
                _cache.Remove("students_list");

                return ApiResponse<Student>.CreateSuccessResponse(student);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error inserting new student.");
                return ApiResponse<Student>.CreateErrorResponse(2, "An error occurred while creating the student.");
            }
        }





        [HttpPatch("{id}")]
        public async Task<ApiResponse<Student>> Edit(int id, [FromBody] Student student)
        {
            // Check if the Id in the URL matches the Id in the student object (optional validation)
            if (!ModelState.IsValid)
            {
                Logger.Warn($"StudentsController => Edit => Edit operation failed: ID mismatch or invalid model state for student with ID: {id}");
                return ApiResponse<Student>.CreateErrorResponse(1, "Invalid student data.");
            }

            try
            {
                // Fetch the current student data from the database
                var existingStudent = await _dbConnection.QueryFirstOrDefaultAsync<Student>(
                    "SELECT * FROM Students WHERE Id = @Id", new { Id = id });

                if (existingStudent == null)
                {
                    Logger.Warn($"StudentsController => Edit => Student with ID: {id} not found.");
                    return ApiResponse<Student>.CreateErrorResponse(3, "Student not found.");
                }

                // Prepare the update object, using existing values for unchanged fields
                var updateFields = new
                {
                   Id = id,
                    FirstName = string.IsNullOrEmpty(student.FirstName) ? existingStudent.FirstName : student.FirstName,
                    LastName = string.IsNullOrEmpty(student.LastName) ? existingStudent.LastName : student.LastName,
                    Email = string.IsNullOrEmpty(student.Email) ? existingStudent.Email : student.Email,
                    DateOfBirth = student.DateOfBirth == default(DateTime) ? existingStudent.DateOfBirth : student.DateOfBirth,
                    Gender = string.IsNullOrEmpty(student.Gender) ? existingStudent.Gender : student.Gender,
                    Division = string.IsNullOrEmpty(student.Division) ? existingStudent.Division : student.Division
                };

                // Update student using a stored procedure
                await _dbConnection.ExecuteAsync(
                    "UpdateStudent",
                    updateFields,
                    commandType: CommandType.StoredProcedure
                );

                Logger.Info($"StudentsController => Edit => Updated student with ID: {student.Id}");

                // Log the action asynchronously
                //await LogActionAsync("Update", $"Updated student with ID: {student.Id}");

                // Invalidate the cache to force refresh the list
                _cache.Remove("students_list");

                return ApiResponse<Student>.CreateSuccessResponse(student);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"StudentsController => Edit => Error updating student with ID: {student.Id}");
                return ApiResponse<Student>.CreateErrorResponse(2, "An error occurred while updating the student.");
            }
        }



        [HttpDelete("delete/{id}")]
        public async Task<ApiResponse<Student>> Delete(int id)
        {
            try
            {
                // Delete student record
                var affectedRows = await _dbConnection.ExecuteAsync(
                    "DeleteStudent",
                    new { Id = id },
                    commandType: CommandType.StoredProcedure
                );

                if (affectedRows == 0)
                {
                    // If no rows were affected, the student was not found
                    Logger.Warn($"StudentsController => Delete => No student found with ID: {id}");
                    return ApiResponse<Student>.CreateErrorResponse(1, "Student not found.");
                }

                Logger.Info($"StudentsController => Delete => Deleted student with ID: {id}");

                // Log the action asynchronously
                //await LogActionAsync("Delete", $"Deleted student with ID: {id}");

                // Invalidate the cache to force refresh the list
                _cache.Remove("students_list");

                return ApiResponse<Student>.CreateSuccessResponse(null);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error during student deletion: {ex.Message}");
                return ApiResponse<Student>.CreateErrorResponse(2, "An error occurred while deleting the student.");
            }
        }
    }
}