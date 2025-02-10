using System.ComponentModel.DataAnnotations;

namespace MyAspNetCoreApp.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; }  // Made required
        [Required]
        public string LastName { get; set; }   // Made required

        [Required]
        [EmailAddress]
        public string Email { get; set; } // Made required and added email validation

        public DateTime DateOfBirth { get; set; }

        [Required]
        public string Gender { get; set; }  // Made required

        [Required]
        public string Division { get; set; }  // Made required

        public DateTime CreatedAt { get; set; }

        // Foreign key reference to the Teacher
        public int? TeacherId { get; set; }
        public Teacher? Teacher { get; set; } // Navigation property

        // Added property for teacher division (to be populated from DB)
        public string TeacherDivision { get; set; }

        // Property to store concatenated teacher names (comma-separated or JSON array)
        public string TeacherNames { get; set; } // Teacher names stored here
    }

    public class Teacher
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }  // Made required

        [Required]
        public string Subject { get; set; }  // Made required

        public string Division { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation property for related students
        public ICollection<Student> Students { get; set; }
    }

    public class ApiResponse<T>
    {
        public T Student { get; set; }
        public int ErrorCode { get; set; }
        public string ErrorMessage { get; set; }

        public static ApiResponse<T> CreateSuccessResponse(T result)
        {
            return new ApiResponse<T>
            {
                Student = result,
                ErrorCode = 0,
                ErrorMessage = null
            };
        }

        public static ApiResponse<T> CreateErrorResponse(int errorCode, string errorMessage)
        {
            return new ApiResponse<T>
            {
                Student = default,
                ErrorCode = errorCode,
                ErrorMessage = errorMessage
            };
        }
    }
}
