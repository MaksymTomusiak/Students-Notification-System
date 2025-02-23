using System.Net;
using System.Net.Http.Json;
using Api.Dtos;
using Domain.Categories;
using Domain.CourseChapters;
using Domain.Courses;
using Domain.Users;
using FluentAssertions;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using Tests.Common;
using Tests.Data;

namespace Api.Tests.Integration.CourseChapters;

public class CourseChaptersControllerTests  : BaseIntegrationTest, IAsyncLifetime
{
    private readonly User _mainUser;
    private readonly Course _mainCourse;
    private const string Password = "TestPass123!";
    private const string Email = "test@te.st";
    private const string UserName = "testUserName";
    private readonly CourseChapter _mainChapter;
    private readonly CourseChapter _secondaryChapter;
    
    public CourseChaptersControllerTests(IntegrationTestWebFactory factory) : base(factory)
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(Password, BCrypt.Net.BCrypt.GenerateSalt());
        _mainUser = UsersData.NewUser(Email, UserName, passwordHash);
        _mainCourse = CoursesData.MainCourse(_mainUser.Id);
        _mainChapter = CourseChaptersData.MainChapter(_mainCourse.Id, 1);
        _secondaryChapter = CourseChaptersData.SecondaryChapter(_mainCourse.Id, 2);
    }
    
    [Fact]
    public async Task ShouldCreateChapter()
    {
        // Arrange
        const string chapterName = "Test chapter name";
        const uint estimatedTime = 5;
        var request = new CourseChapterCreateDto(
            _mainCourse.Id.Value,
            chapterName,
            estimatedTime);

        // Act
        var response = await Client.PostAsJsonAsync("course-chapters/add", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var responseChapter = await response.ToResponseModel<CourseChapterDto>();
        var chapterId = new CourseChapterId(responseChapter.Id);

        var dbChapter = await Context.Chapters.FirstAsync(x => x.Id == chapterId);
        dbChapter.Should().NotBeNull();
        dbChapter.Name.Should().Be(chapterName);
        dbChapter.EstimatedLearningTimeMinutes.Should().Be(estimatedTime);
        dbChapter.Number.Should().Be(3);
    }
    
    [Fact]
    public async Task ShouldNotCreateChapterBecauseNameDuplicated()
    {
        // Arrange
        var chapterName = _mainChapter.Name;
        const uint estimatedTime = 5;
        var request = new CourseChapterCreateDto(
            _mainCourse.Id.Value,
            chapterName,
            estimatedTime);

        // Act
        var response = await Client.PostAsJsonAsync("course-chapters/add", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
    
    [Fact]
    public async Task ShouldNotCreateChapterBecauseCourseNotFound()
    {
        // Arrange
        const string chapterName = "Test chapter name";
        const uint estimatedTime = 5;
        var request = new CourseChapterCreateDto(
            Guid.NewGuid(),
            chapterName,
            estimatedTime);

        // Act
        var response = await Client.PostAsJsonAsync("course-chapters/add", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldUpdateChapter()
    {
        // Arrange
        var chapterId = _mainChapter.Id;
        const string chapterName = "Updated chapter name";
        const uint estimatedTime = 7;
        var request = new CourseChapterUpdateDto(
            chapterId.Value,
            chapterName,
            estimatedTime);

        // Act
        var response = await Client.PutAsJsonAsync("course-chapters/update", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var dbChapter = await Context.Chapters.FirstAsync(x => x.Id == chapterId);
        dbChapter.Should().NotBeNull();
        dbChapter.Name.Should().Be(chapterName);
        dbChapter.EstimatedLearningTimeMinutes.Should().Be(estimatedTime);
        dbChapter.Number.Should().Be(_mainChapter.Number);
    }
    
    [Fact]
    public async Task ShouldNotUpdateChapterBecauseChapterNotFound()
    {
        // Arrange
        var chapterId = Guid.NewGuid();
        const string chapterName = "Updated chapter name";
        const uint estimatedTime = 7;
        var request = new CourseChapterUpdateDto(
            chapterId,
            chapterName,
            estimatedTime);

        // Act
        var response = await Client.PutAsJsonAsync("course-chapters/update", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task ShouldNotUpdateChapterBecauseNameDuplicated()
    {
        // Arrange
        var chapterId = _mainChapter.Id;
        var chapterName = _secondaryChapter.Name;
        const uint estimatedTime = 7;
        var request = new CourseChapterUpdateDto(
            chapterId.Value,
            chapterName,
            estimatedTime);

        // Act
        var response = await Client.PutAsJsonAsync("course-chapters/update", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
    
    [Fact]
    public async Task ShouldDeleteChapter()
    {
        // Arrange
        var chapterId = _mainChapter.Id;

        // Act
        var response = await Client.DeleteAsync($"course-chapters/delete/{chapterId}");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var dbChapter = await Context.Chapters.FirstOrDefaultAsync(x => x.Id == chapterId);
        dbChapter.Should().BeNull();
    }
    
    [Fact]
    public async Task ShouldNotDeleteChapterBecauseChapterNotFound()
    {
        // Arrange
        var chapterId = Guid.NewGuid();

        // Act
        var response = await Client.DeleteAsync($"course-chapters/delete/{chapterId}"); 

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldUpdateChaptersOrder()
    {
        // Arrange
        var chapters = new List<CourseChapterId>
        {
            _mainChapter.Id,
            _secondaryChapter.Id
        };
        var numbers = new List<uint>
        {
            2,
            1
        };
        var request = new CourseChaptersUpdateOrderDto(
            chapters.Select(x => x.Value),
            numbers);

        // Act
        var response = await Client.PutAsJsonAsync("course-chapters/update-order", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        for (int i = 0; i < chapters.Count; i++)
        {
            var dbChapter = await Context.Chapters.FirstAsync(x => x.Id == chapters[i]);
            dbChapter.Number.Should().Be(numbers[i]);
        }
    }
    
    [Fact]
    public async Task ShouldNotUpdateChaptersOrderBecauseChapterNotFound()
    {
        // Arrange
        var chapters = new List<Guid>
        {
            Guid.NewGuid(),
            Guid.NewGuid()
        };
        var numbers = new List<uint>
        {
            2,
            1
        };
        var request = new CourseChaptersUpdateOrderDto(
            chapters,
            numbers);

        // Act
        var response = await Client.PutAsJsonAsync("course-chapters/update-order", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    public async Task InitializeAsync()
    {
        await UserManager.CreateAsync(_mainUser);
        await Context.Courses.AddAsync(_mainCourse);
        await Context.Chapters.AddRangeAsync(_mainChapter, _secondaryChapter);
        await SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        Context.Chapters.RemoveRange(Context.Chapters);
        Context.Courses.RemoveRange(Context.Courses);
        Context.Users.RemoveRange(Context.Users);
        await SaveChangesAsync();
    }
}