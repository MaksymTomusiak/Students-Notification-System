using System.Net;
using System.Net.Http.Json;
using Api.Dtos;
using Domain.Categories;
using Domain.CourseCategories;
using Domain.Courses;
using Domain.Users;
using FluentAssertions;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using Tests.Common;
using Tests.Data;

namespace Api.Tests.Integration.Courses;

public class CoursesControllerTests : BaseIntegrationTest, IAsyncLifetime
{
    private readonly Category _mainCategory = CategoriesData.MainCategory;
    private readonly Category _secondaryCategory = CategoriesData.SecondaryCategory;
    private readonly Course _mainCourse;
    private readonly Course _secondaryCourse;
    private readonly CourseCategory _mainCourseCategory;
    private readonly User _mainUser;
    private const string Password = "TestPass123!";
    private const string Email = "test@te.st";
    private const string UserName = "testUserName";
    
    public CoursesControllerTests(IntegrationTestWebFactory factory) : base(factory)
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(Password, BCrypt.Net.BCrypt.GenerateSalt());
        _mainUser = UsersData.NewUser(Email, UserName, passwordHash);
        _mainCourse = CoursesData.MainCourse(_mainUser.Id);
        _secondaryCourse = CoursesData.SecondaryCourse(_mainUser.Id);
        _mainCourseCategory = CourseCategoriesData.New(_mainCourse.Id, _mainCategory.Id);
    }

    [Fact]
    public async Task ShouldCreateCourse()
    {
        // Arrange
        const string courseName = "Test course name";
        const string courseDescription = "Test course description";
        var startDate = DateTime.UtcNow + TimeSpan.FromHours(1);
        var finishDate = DateTime.UtcNow + TimeSpan.FromHours(10);
        var creatorId = _mainUser.Id;
        var categoryId = _mainCategory.Id;
        const string language = "English";
        const string requirements = "Test requirements";
        var request = new MultipartFormDataContent
        {
            { new StringContent(courseName), "Name" },
            { new StringContent(courseDescription), "Description" },
            { new StringContent(startDate.ToString("o")), "StartDate" },
            { new StringContent(finishDate.ToString("o")), "FinishDate" },
            { new StringContent(creatorId.ToString()), "CreatorId" },
            { new StringContent(categoryId.Value.ToString()), "CategoriesIds" },
            { new StringContent(language), "Language" },
            { new StringContent(requirements), "Requirements" },
        };
        
        // Act
        var response = await Client.PostAsync("courses/add", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var responseCourse = await response.ToResponseModel<CourseDto>();
        var courseId = new CourseId(responseCourse.Id);

        var dbCourse = await Context.Courses.FirstAsync(x => x.Id == courseId);
        dbCourse.Should().NotBeNull();
        dbCourse.Name.Should().Be(courseName);
        dbCourse.ImageUrl.Should().Be(string.Empty);
        dbCourse.Description.Should().Be(courseDescription);
        dbCourse.StartDate.Should().BeSameDateAs(startDate);
        dbCourse.FinishDate.Should().BeSameDateAs(finishDate);
        dbCourse.Language.Should().Be(language);
        dbCourse.Requirements.Should().Be(requirements);
        dbCourse.CreatorId.Should().Be(creatorId);
        
        var dbCourseCategory = await Context.CourseCategories.FirstOrDefaultAsync(x => x.CourseId == courseId && x.CategoryId == categoryId);
        dbCourseCategory.Should().NotBeNull();
    }
    
    
    [Fact]
    public async Task ShouldNotCreateCourseBecauseCourseAlreadyExists()
    {
        // Arrange
        var courseName = _mainCourse.Name;
        const string courseDescription = "Test course description";
        var startDate = DateTime.UtcNow + TimeSpan.FromHours(1);
        var finishDate = DateTime.UtcNow + TimeSpan.FromHours(10);
        var creatorId = _mainUser.Id;
        var categoryId = _mainCategory.Id;
        const string language = "English";
        const string requirements = "Test requirements";
        var request = new MultipartFormDataContent
        {
            { new StringContent(courseName), "Name" },
            { new StringContent(courseDescription), "Description" },
            { new StringContent(startDate.ToString("o")), "StartDate" },
            { new StringContent(finishDate.ToString("o")), "FinishDate" },
            { new StringContent(creatorId.ToString()), "CreatorId" },
            { new StringContent(categoryId.Value.ToString()), "CategoriesIds" },
            { new StringContent(language), "Language" },
            { new StringContent(requirements), "Requirements" },
        };
        
        // Act
        var response = await Client.PostAsync("courses/add", request);

        // Assert 
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task ShouldNotCreateCourseBecauseCreatorNotFound()
    {
        // Arrange
        const string courseName = "Test course name";
        const string courseDescription = "Test course description";
        var startDate = DateTime.UtcNow + TimeSpan.FromHours(1);
        var finishDate = DateTime.UtcNow + TimeSpan.FromHours(10);
        var creatorId = Guid.NewGuid();
        var categoryId = _mainCategory.Id;
        const string language = "English";
        const string requirements = "Test requirements";
        var request = new MultipartFormDataContent
        {
            { new StringContent(courseName), "Name" },
            { new StringContent(courseDescription), "Description" },
            { new StringContent(startDate.ToString("o")), "StartDate" },
            { new StringContent(finishDate.ToString("o")), "FinishDate" },
            { new StringContent(creatorId.ToString()), "CreatorId" },
            { new StringContent(categoryId.Value.ToString()), "CategoriesIds" },
            { new StringContent(language), "Language" },
            { new StringContent(requirements), "Requirements" },
        };
        // Act
        var response = await Client.PostAsync("courses/add", request);

        // Assert 
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldNotCreateCourseBecauseCategoryNotFound()
    {
        // Arrange
        const string courseName = "Test course name";
        const string courseDescription = "Test course description";
        var startDate = DateTime.UtcNow + TimeSpan.FromHours(1);
        var finishDate = DateTime.UtcNow + TimeSpan.FromHours(10);
        var creatorId = _mainUser.Id;
        var categoryId = Guid.NewGuid();
        const string language = "English";
        const string requirements = "Test requirements";
        var request = new MultipartFormDataContent
        {
            { new StringContent(courseName), "Name" },
            { new StringContent(courseDescription), "Description" },
            { new StringContent(startDate.ToString("o")), "StartDate" },
            { new StringContent(finishDate.ToString("o")), "FinishDate" },
            { new StringContent(creatorId.ToString()), "CreatorId" },
            { new StringContent(categoryId.ToString()), "CategoriesIds" },
            { new StringContent(language), "Language" },
            { new StringContent(requirements), "Requirements" },
        };

        // Act
        var response = await Client.PostAsync("courses/add", request);

        // Assert 
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldUpdateCourse()
    {
        // Arrange
        var requestCourseId = _mainCourse.Id;
        const string courseName = "Test course name";
        const string courseDescription = "Test course description";
        var startDate = DateTime.UtcNow + TimeSpan.FromHours(1);
        var finishDate = DateTime.UtcNow + TimeSpan.FromHours(10);
        var creatorId = _mainUser.Id;
        var categoryId = _secondaryCategory.Id;
        const string language = "English updated";
        const string requirements = "Test requirements updated";
        var request = new MultipartFormDataContent
        {
            { new StringContent(requestCourseId.Value.ToString()), "Id" },
            { new StringContent(courseName), "Name" },
            { new StringContent(courseDescription), "Description" },
            { new StringContent(startDate.ToString("o")), "StartDate" },
            { new StringContent(finishDate.ToString("o")), "FinishDate" },
            { new StringContent(creatorId.ToString()), "CreatorId" },
            { new StringContent(categoryId.Value.ToString()), "CategoriesIds" },
            { new StringContent(language), "Language" },
            { new StringContent(requirements), "Requirements" },
        };

        // Act
        var response = await Client.PutAsync("courses/update", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var responseCourse = await response.ToResponseModel<CourseDto>();
        var responseCourseId = new CourseId(responseCourse.Id);
        responseCourseId.Should().Be(requestCourseId);

        var dbCourse = await Context.Courses.FirstAsync(x => x.Id == responseCourseId);
        dbCourse.Should().NotBeNull();
        dbCourse.Name.Should().Be(courseName);
        dbCourse.ImageUrl.Should().Be(_mainCourse.ImageUrl);
        dbCourse.Description.Should().Be(courseDescription);
        dbCourse.StartDate.Should().BeSameDateAs(startDate);
        dbCourse.FinishDate.Should().BeSameDateAs(finishDate);
        dbCourse.CreatorId.Should().Be(creatorId);
        dbCourse.Language.Should().Be(language);
        dbCourse.Requirements.Should().Be(requirements);
        
        var dbCourseCategory = await Context.CourseCategories.FirstOrDefaultAsync(x => x.CourseId == responseCourseId && x.CategoryId == categoryId);
        dbCourseCategory.Should().NotBeNull();
    }

    [Fact]
    public async Task ShouldNotUpdateCourseBecauseCourseNotFound()
    {
        // Arrange
        var requestCourseId = Guid.NewGuid();
        const string courseName = "Test course name";
        const string courseDescription = "Test course description";
        var startDate = DateTime.UtcNow + TimeSpan.FromHours(1);
        var finishDate = DateTime.UtcNow + TimeSpan.FromHours(10);
        var creatorId = _mainUser.Id;
        var categoryId = _mainCategory.Id;
        const string language = "English updated";
        const string requirements = "Test requirements updated";
        var request = new MultipartFormDataContent
        {
            { new StringContent(requestCourseId.ToString()), "Id" },
            { new StringContent(courseName), "Name" },
            { new StringContent(courseDescription), "Description" },
            { new StringContent(startDate.ToString("o")), "StartDate" },
            { new StringContent(finishDate.ToString("o")), "FinishDate" },
            { new StringContent(creatorId.ToString()), "CreatorId" },
            { new StringContent(categoryId.Value.ToString()), "CategoriesIds" },
            { new StringContent(language), "Language" },
            { new StringContent(requirements), "Requirements" },
        };

        // Act
        var response = await Client.PutAsync("courses/update", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldNotUpdateCourseBecauseCategoryNotFound()
    {
        // Arrange
        var requestCourseId = _mainCourse.Id;
        const string courseName = "Test course name";
        const string courseDescription = "Test course description";
        var startDate = DateTime.UtcNow + TimeSpan.FromHours(1);
        var finishDate = DateTime.UtcNow + TimeSpan.FromHours(10);
        var creatorId = _mainUser.Id;
        var categoryId = Guid.NewGuid();
        const string language = "English updated";
        const string requirements = "Test requirements updated";
        var request = new MultipartFormDataContent
        {
            { new StringContent(requestCourseId.Value.ToString()), "Id" },
            { new StringContent(courseName), "Name" },
            { new StringContent(courseDescription), "Description" },
            { new StringContent(startDate.ToString("o")), "StartDate" },
            { new StringContent(finishDate.ToString("o")), "FinishDate" },
            { new StringContent(creatorId.ToString()), "CreatorId" },
            { new StringContent(categoryId.ToString()), "CategoriesIds" },
            { new StringContent(language), "Language" },
            { new StringContent(requirements), "Requirements" },
        };

        // Act
        var response = await Client.PutAsync("courses/update", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldDeleteCourse()
    {
        // Arrange
        var courseId = _secondaryCourse.Id;

        // Act
        var response = await Client.DeleteAsync($"courses/delete/{courseId}");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var responseCourse = await response.ToResponseModel<CourseDto>();
        responseCourse.Id.Should().Be(courseId.Value);

        var dbCourse = await Context.Courses.FirstOrDefaultAsync(x => x.Id == courseId);
        dbCourse.Should().BeNull();
    }

    [Fact]
    public async Task ShouldNotDeleteCourseBecauseCourseNotFound()
    {
        // Arrange
        var courseId = Guid.NewGuid();

        // Act
        var response = await Client.DeleteAsync($"courses/delete/{courseId}");

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    public async Task InitializeAsync()
    {
        await UserManager.CreateAsync(_mainUser);
        await Context.Categories.AddRangeAsync(_mainCategory, _secondaryCategory);
        await Context.Courses.AddRangeAsync(_mainCourse, _secondaryCourse);
        await Context.CourseCategories.AddAsync(_mainCourseCategory);

        await SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        Context.CourseCategories.RemoveRange(Context.CourseCategories);
        Context.Courses.RemoveRange(Context.Courses);
        Context.Categories.RemoveRange(Context.Categories);
        Context.Users.RemoveRange(Context.Users);

        await SaveChangesAsync();
    }
}