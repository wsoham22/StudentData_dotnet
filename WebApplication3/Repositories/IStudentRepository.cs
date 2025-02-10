using MyAspNetCoreApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace MyAspNetCoreApp.Repositories;
public interface IStudentRepository
{
    Task<IEnumerable<Student>> GetStudentsAsync();
    Task<Student> GetStudentAsync(int id);
    Task<Student> CreateStudentAsync(Student student);
    Task<Student> UpdateStudentAsync(int id, Student student);
    Task<bool> DeleteStudentAsync(int id);
}

