using System.Net;
using System.Net.Http.Json;
using Api.Dtos;
using Domain.CourseChapters;
using Domain.CourseSubChapters;
using Domain.Courses;
using Domain.Users;
using FluentAssertions;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using Tests.Common;
using Tests.Data;

namespace Api.Tests.Integration.CourseSubChapters;

public class CourseSubChaptersControllerTests : BaseIntegrationTest, IAsyncLifetime
{
    private readonly User _mainUser;
    private readonly Course _mainCourse;
    private const string Password = "TestPass123!";
    private const string Email = "test@te.st";
    private const string UserName = "testUserName";
    private readonly CourseChapter _mainChapter;
    private readonly CourseSubChapter _mainSubChapter;
    private readonly CourseSubChapter _secondarySubChapter;
    
    public CourseSubChaptersControllerTests(IntegrationTestWebFactory factory) : base(factory)
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(Password, BCrypt.Net.BCrypt.GenerateSalt());
        _mainUser = UsersData.NewUser(Email, UserName, passwordHash);
        _mainCourse = CoursesData.MainCourse(_mainUser.Id);
        _mainChapter = CourseChaptersData.MainChapter(_mainCourse.Id, 1);
        _mainSubChapter = CourseSubChaptersData.MainSubChapter(_mainChapter.Id, 1);
        _secondarySubChapter = CourseSubChaptersData.SecondarySubChapter(_mainChapter.Id, 2);
    }
    
    [Fact]
    public async Task ShouldCreateSubChapter()
    {
        // Arrange
        var chapterId = _mainChapter.Id;
        const string subChapterName = "Test subchapter name";
        const string subChapterContent = "Test subchapter content";
        const uint estimatedTime = 5;
        var request = new CourseSubChapterCreateDto(
            chapterId.Value,
            subChapterName,
            subChapterContent,
            estimatedTime);

        // Act
        var response = await Client.PostAsJsonAsync("course-subchapters/add", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var responseSubChapter = await response.ToResponseModel<CourseSubChapterDto>();
        var subChapterId = new CourseSubChapterId(responseSubChapter.Id);

        var dbSubChapter = await Context.SubChapters.FirstAsync(x => x.Id == subChapterId);
        dbSubChapter.Should().NotBeNull();
        dbSubChapter.Name.Should().Be(subChapterName);
        dbSubChapter.Content.Should().Be(subChapterContent);
        dbSubChapter.EstimatedLearningTimeMinutes.Should().Be(estimatedTime);
        dbSubChapter.CourseChapterId.Should().Be(chapterId);
        dbSubChapter.Number.Should().Be(3);
    }
    
    [Fact]
    public async Task ShouldNotCreateSubChapterBecauseNameDuplicated()
    {
        // Arrange
        var chapterId = _mainChapter.Id;
        var subChapterName = _mainSubChapter.Name;
        const string subChapterContent = "Test subchapter content";
        const uint estimatedTime = 5;
        var request = new CourseSubChapterCreateDto(
            chapterId.Value,
            subChapterName,
            subChapterContent,
            estimatedTime);

        // Act
        var response = await Client.PostAsJsonAsync("course-subchapters/add", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
    
    [Fact]
    public async Task ShouldNotCreateSubChapterBecauseChapterNotFound()
    {
        // Arrange
        var chapterId = Guid.NewGuid();
        var subChapterName = "Test subchapter name";
        const string subChapterContent = "Test subchapter content";
        const uint estimatedTime = 5;
        var request = new CourseSubChapterCreateDto(
            chapterId,
            subChapterName,
            subChapterContent,
            estimatedTime);

        // Act
        var response = await Client.PostAsJsonAsync("course-subchapters/add", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task ShouldUpdateSubChapter()
    {
        // Arrange
        var subChapterId = _mainSubChapter.Id;
        const string subChapterName = "Updated test subchapter name";
        const string subChapterContent = "Updated test subchapter content";
        const uint estimatedTime = 9;
        var request = new CourseSubChapterUpdateDto(
            subChapterId.Value,
            subChapterName,
            subChapterContent,
            estimatedTime);

        // Act
        var response = await Client.PutAsJsonAsync("course-subchapters/update", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var dbSubChapter = await Context.SubChapters.FirstAsync(x => x.Id == subChapterId);
        dbSubChapter.Should().NotBeNull();
        dbSubChapter.Name.Should().Be(subChapterName);
        dbSubChapter.Content.Should().Be(subChapterContent);
        dbSubChapter.EstimatedLearningTimeMinutes.Should().Be(estimatedTime);
        dbSubChapter.CourseChapterId.Should().Be(_mainChapter.Id);
        dbSubChapter.Number.Should().Be(_mainSubChapter.Number);
    }
    
    [Fact]
    public async Task ShouldNotUpdateSubChapterBecauseNameDuplicated()
    {
        // Arrange
        var subChapterId = _mainSubChapter.Id;
        var subChapterName = _secondarySubChapter.Name;
        const string subChapterContent = "Updated test subchapter content";
        const uint estimatedTime = 9;
        var request = new CourseSubChapterUpdateDto(
            subChapterId.Value,
            subChapterName,
            subChapterContent,
            estimatedTime);

        // Act
        var response = await Client.PutAsJsonAsync("course-subchapters/update", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
    
    [Fact]
    public async Task ShouldNotUpdateSubChapterBecauseSubChapterNotFound()
    {
        // Arrange
        var subChapterId = Guid.NewGuid();
        var subChapterName = "Updated test subchapter name";
        const string subChapterContent = "Updated test subchapter content";
        const uint estimatedTime = 9;
        var request = new CourseSubChapterUpdateDto(
            subChapterId,
            subChapterName,
            subChapterContent,
            estimatedTime);

        // Act
        var response = await Client.PutAsJsonAsync("course-subchapters/update", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task ShouldDeleteSubChapter()
    {
        // Arrange
        var subChapterId = _mainSubChapter.Id;

        // Act
        var response = await Client.DeleteAsync($"course-subchapters/delete/{subChapterId}");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var dbSubChapter = await Context.SubChapters.FirstOrDefaultAsync(x => x.Id == subChapterId);
        dbSubChapter.Should().BeNull();
    }
    
    [Fact]
    public async Task ShouldNotDeleteSubChapterBecauseSubChapterNotFound()
    {
        // Arrange
        var subChapterId = Guid.NewGuid();

        // Act
        var response = await Client.DeleteAsync($"course-subchapters/delete/{subChapterId}");

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task ShouldUpdateSubChaptersOrder()
    {
        // Arrange
        var subChapters = new List<CourseSubChapterId>
        {
            _mainSubChapter.Id,
            _secondarySubChapter.Id
        };
        var numbers = new List<uint>
        {
            2,
            1
        };
        var request = new CourseSubChaptersUpdateOrderDto(
            subChapters.Select(x => x.Value),
            numbers);

        // Act
        var response = await Client.PutAsJsonAsync("course-subchapters/update-order", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        for (int i = 0; i < subChapters.Count; i++)
        {
            var dbSubChapter = await Context.SubChapters.FirstAsync(x => x.Id == subChapters[i]);
            dbSubChapter.Number.Should().Be(numbers[i]);
        }
    }
    
    [Fact]
    public async Task ShouldNotUpdateSubChaptersOrderBecauseSubChapterNotFound()
    {
        // Arrange
        var subChapters = new List<CourseSubChapterId>
        {
            CourseSubChapterId.New(),
            CourseSubChapterId.New()
        };
        var numbers = new List<uint>
        {
            2,
            1
        };
        var request = new CourseSubChaptersUpdateOrderDto(
            subChapters.Select(x => x.Value),
            numbers);

        // Act
        var response = await Client.PutAsJsonAsync("course-subchapters/update-order", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    public async Task InitializeAsync()
    {
        await Context.Users.AddAsync(_mainUser);
        await Context.Courses.AddAsync(_mainCourse);
        await Context.Chapters.AddAsync(_mainChapter);
        await Context.SubChapters.AddRangeAsync(_mainSubChapter, _secondarySubChapter);
        await SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        Context.SubChapters.RemoveRange(Context.SubChapters);
        Context.Chapters.RemoveRange(Context.Chapters);
        Context.Courses.RemoveRange(Context.Courses);
        Context.Users.RemoveRange(Context.Users);
        await SaveChangesAsync();
    }
}
