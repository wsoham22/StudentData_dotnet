using Dapper;
using MyAspNetCoreApp.Models;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace MyAspNetCoreApp.Repositories;

public class StudentsController
{
    private readonly IStudentRepository _studentRepository;

    public StudentsController(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }

    public async Task<ApiResponse<IEnumerable<Student>>> GetStudents()
    {
        var students = await _studentRepository.GetStudentsAsync();
        return ApiResponse<IEnumerable<Student>>.CreateSuccessResponse(students);
    }

    public async Task<ApiResponse<Student>> GetStudent(int id)
    {
        var student = await _studentRepository.GetStudentAsync(id);
        if (student == null)
        {
            return ApiResponse<Student>.CreateErrorResponse(1, "Student not found");
        }
        return ApiResponse<Student>.CreateSuccessResponse(student);
    }

    public async Task<ApiResponse<Student>> Create(Student student)
    {
        var createdStudent = await _studentRepository.CreateStudentAsync(student);
        return ApiResponse<Student>.CreateSuccessResponse(createdStudent);
    }

    public async Task<ApiResponse<Student>> Edit(int id, Student student)
    {
        var updatedStudent = await _studentRepository.UpdateStudentAsync(id, student);
        return ApiResponse<Student>.CreateSuccessResponse(updatedStudent);
    }

    public async Task<ApiResponse<Student>> Delete(int id)
    {
        var isDeleted = await _studentRepository.DeleteStudentAsync(id);
        if (!isDeleted)
        {
            return ApiResponse<Student>.CreateErrorResponse(1, "Student not found.");
        }
        return ApiResponse<Student>.CreateSuccessResponse(null);
    }
}

