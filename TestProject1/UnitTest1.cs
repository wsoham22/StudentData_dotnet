using Moq;
using Xunit;
using MyAspNetCoreApp.Models;
using MyAspNetCoreApp.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;

namespace MyAspNetCoreApp.Tests
{
    public class StudentsControllerTests
    {
        private readonly Mock<IStudentRepository> _mockStudentRepo;
        private readonly StudentsController _controller;

        public StudentsControllerTests()
        {
            _mockStudentRepo = new Mock<IStudentRepository>();
            _controller = new StudentsController(_mockStudentRepo.Object);
        }

        [Fact]
        public async Task GetStudents_ReturnsSuccessResponse()
        {
            // Arrange
            var students = new List<Student>
            {
                new Student { Id = 1, FirstName = "John", LastName = "Doe", Email = "john.doe@example.com" },
                new Student { Id = 2, FirstName = "Jane", LastName = "Doe", Email = "jane.doe@example.com" }
            };

            _mockStudentRepo.Setup(repo => repo.GetStudentsAsync()).ReturnsAsync(students);

            // Act
            var result = await _controller.GetStudents();

            // Assert
            result.ErrorCode.Should().Be(0);
            result.Student.Should().HaveCount(2);
            result.Student.Should().BeEquivalentTo(students);
        }

        [Fact]
        public async Task GetStudent_ReturnsStudent_WhenStudentExists()
        {
            // Arrange
            var student = new Student { Id = 1, FirstName = "John", LastName = "Doe", Email = "john.doe@example.com" };
            _mockStudentRepo.Setup(repo => repo.GetStudentAsync(1)).ReturnsAsync(student);

            // Act
            var result = await _controller.GetStudent(1);

            // Assert
            result.ErrorCode.Should().Be(0);
            result.Student.Should().BeEquivalentTo(student);
        }

        [Fact]
        public async Task GetStudent_ReturnsError_WhenStudentNotFound()
        {
            // Arrange
            _mockStudentRepo.Setup(repo => repo.GetStudentAsync(999)).ReturnsAsync((Student)null);

            // Act
            var result = await _controller.GetStudent(999);

            // Assert
            result.ErrorCode.Should().Be(1);
            result.ErrorMessage.Should().Be("Student not found");
        }

        [Fact]
        public async Task CreateStudent_ReturnsCreatedStudent()
        {
            // Arrange
            var student = new Student { FirstName = "John", LastName = "Doe", Email = "john.doe@example.com" };
            var createdStudent = new Student { Id = 1, FirstName = "John", LastName = "Doe", Email = "john.doe@example.com" };
            _mockStudentRepo.Setup(repo => repo.CreateStudentAsync(student)).ReturnsAsync(createdStudent);

            // Act
            var result = await _controller.Create(student);

            // Assert
            result.ErrorCode.Should().Be(0);
            result.Student.Should().BeEquivalentTo(createdStudent);
        }

        [Fact]
        public async Task EditStudent_ReturnsUpdatedStudent()
        {
            // Arrange
            var student = new Student { Id = 1, FirstName = "John", LastName = "Doe", Email = "john.doe@example.com" };
            var updatedStudent = new Student { Id = 1, FirstName = "John", LastName = "Smith", Email = "john.smith@example.com" };
            _mockStudentRepo.Setup(repo => repo.UpdateStudentAsync(1, student)).ReturnsAsync(updatedStudent);

            // Act
            var result = await _controller.Edit(1, student);

            // Assert
            result.ErrorCode.Should().Be(0);
            result.Student.FirstName.Should().Be("John");
            result.Student.LastName.Should().Be("Smith");
        }

        [Fact]
        public async Task DeleteStudent_ReturnsSuccess_WhenDeleted()
        {
            // Arrange
            _mockStudentRepo.Setup(repo => repo.DeleteStudentAsync(1)).ReturnsAsync(true);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            result.ErrorCode.Should().Be(0);
            result.Student.Should().BeNull();
        }

        [Fact]
        public async Task DeleteStudent_ReturnsError_WhenStudentNotFound()
        {
            // Arrange
            _mockStudentRepo.Setup(repo => repo.DeleteStudentAsync(999)).ReturnsAsync(false);

            // Act
            var result = await _controller.Delete(999);

            // Assert
            result.ErrorCode.Should().Be(1);
            result.ErrorMessage.Should().Be("Student not found.");
        }
    }
}
