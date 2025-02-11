using System.Net;
using System.Net.Http.Json;
using Api.Dtos;
using Domain.Categories;
using Domain.CourseBans;
using Domain.CourseCategories;
using Domain.Courses;
using Domain.Users;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Tests.Common;
using Tests.Data;

namespace Api.Tests.Integration.CourseBans;

public class CourseBansControllerTests: BaseIntegrationTest, IAsyncLifetime
{
    private readonly Category _mainCategory = CategoriesData.MainCategory;
    private readonly User _mainUser;
    private readonly User _secondaryUser;
    private readonly Course _mainCourse;
    private readonly CourseCategory _mainCourseCategory;
    private readonly CourseBan _mainBan;
    private const string Password = "TestPass123!";
    
    public CourseBansControllerTests(IntegrationTestWebFactory factory) : base(factory)
    {
        _mainUser = UsersData.MainUser();
        _secondaryUser = UsersData.SecondaryUser();
        _mainCourse = CoursesData.MainCourse(_mainUser.Id);
        _mainCourseCategory = CourseCategoriesData.New(_mainCourse.Id, _mainCategory.Id);
        _mainBan = CourseBansData.New(_mainCourse.Id, _secondaryUser.Id);
    }
    
    [Fact]
    public async Task ShouldBanUserFromCourse()
    {
        // Arrange
        var userId = _mainUser.Id;
        var courseId = _mainCourse.Id;
        const string reason = "Test reason";
        var request = new BanDto(
            userId,
            courseId.Value,
            reason);

        // Act
        var response = await Client.PostAsJsonAsync("bans/add", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var responseBan = await response.ToResponseModel<CourseBanDto>();
        var courseBanId = new CourseBanId(responseBan.Id);

        var dbBan = await Context.CourseBans.FirstAsync(x => x.Id == courseBanId);
        dbBan.Should().NotBeNull();
        dbBan.Reason.Should().Be(reason);
        dbBan.UserId.Should().Be(userId);
        dbBan.CourseId.Should().Be(courseId);
    }
    
    [Fact]
    public async Task ShouldNotBanUserFromCourseBecauseUserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var courseId = _mainCourse.Id;
        const string reason = "Test reason";
        var request = new BanDto(
            userId,
            courseId.Value,
            reason);

        // Act
        var response = await Client.PostAsJsonAsync("bans/add", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task ShouldNotBanUserFromCourseBecauseCourseNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var courseId = _mainCourse.Id;
        const string reason = "Test reason";
        var request = new BanDto(
            userId,
            courseId.Value,
            reason);

        // Act
        var response = await Client.PostAsJsonAsync("bans/add", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task ShouldNotBanUserFromCourseBecauseBanAlreadyExists()
    {
        // Arrange
        var userId = _secondaryUser.Id;
        var courseId = _mainCourse.Id;
        const string reason = "Test reason";
        var request = new BanDto(
            userId,
            courseId.Value,
            reason);

        // Act
        var response = await Client.PostAsJsonAsync("bans/add", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task ShouldUnbanUserFromCourse()
    {
        // Arrange
        var banId = _mainBan.Id;

        // Act
        var response = await Client.DeleteAsync($"bans/delete/{banId}");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var dbBan = await Context.CourseBans.FirstOrDefaultAsync(x => x.Id == banId);
        dbBan.Should().BeNull();
    }
    
    [Fact]
    public async Task ShouldNotUnbanUserFromCourseBecauseBanNotFound()
    {
        // Arrange
        var banId = Guid.NewGuid();

        // Act
        var response = await Client.DeleteAsync($"bans/delete/{banId}");

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    public async Task InitializeAsync()
    {
        await UserManager.CreateAsync(_mainUser, Password);
        await UserManager.CreateAsync(_secondaryUser, Password);
        await Context.Categories.AddRangeAsync(_mainCategory);
        await Context.Courses.AddAsync(_mainCourse);
        await Context.CourseCategories.AddAsync(_mainCourseCategory);
        await Context.CourseBans.AddAsync(_mainBan);
        await SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        Context.CourseBans.RemoveRange(Context.CourseBans);
        Context.CourseCategories.RemoveRange(Context.CourseCategories);
        Context.Courses.RemoveRange(Context.Courses);
        Context.Categories.RemoveRange(Context.Categories);
        Context.Users.RemoveRange(Context.Users);
        await SaveChangesAsync();
    }
}