using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using Api.Dtos;
using Domain.Categories;
using Domain.CourseCategories;
using Domain.Courses;
using Domain.Feedbacks;
using Domain.Registers;
using Domain.Roles;
using Domain.Users;
using FluentAssertions;
using Infrastructure.Authentication;
using LanguageExt.Pipes;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Tests.Common;
using Tests.Data;

namespace Api.Tests.Integration.Feedbacks;

public class FeedbacksControllerTests: BaseIntegrationTest, IAsyncLifetime
{
    private readonly User _mainUser;
    private readonly User _secondaryUser;
    private readonly Role _userRole = RolesData.UserRole;
    private readonly Role _adminRole = RolesData.AdminRole;
    private readonly Course _mainCourse;
    private readonly Course _secondaryCourse;
    private readonly Register _secondaryRegister;
    private readonly Feedback _mainFeedback;
    private const string TestPassword = "TestPass123!";
    
    public FeedbacksControllerTests(IntegrationTestWebFactory factory) : base(factory)
    {
        _mainUser = UsersData.MainUser();
        _secondaryUser = UsersData.SecondaryUser();
        _mainCourse = CoursesData.MainCourse(_mainUser.Id);
        _secondaryCourse = CoursesData.SecondaryCourse(_mainUser.Id);
        _secondaryRegister = RegistersData.New(_secondaryUser.Id, _mainCourse.Id);
        _mainFeedback = FeedbacksData.New(_mainUser.Id, _mainCourse.Id);
    }
    
    [Fact]
    public async Task ShouldCreateFeedbackOnCourse()
    {
        // Arrange
        var courseId = _mainCourse.Id;
        var content = "Test content";
        ushort rating = 3;
        var request = new FeedbackCreateDto(courseId.Value, content, rating);
        
        SetCustomAuthorizationHeader(JwtProvider.Generate(_secondaryUser, _userRole));

        // Act
        var response = await Client.PostAsJsonAsync("feedbacks/add", request);
        
        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var responseFeedback = await response.ToResponseModel<FeedbackDto>();
        var feedbackId = new FeedbackId(responseFeedback.Id);
        
        // Retrieve the register from the database
        var dbFeedback = await Context.Feedbacks.FirstOrDefaultAsync(x => x.Id == feedbackId);
        dbFeedback.Should().NotBeNull();
        dbFeedback!.UserId.Should().Be(_secondaryUser.Id);
        dbFeedback.CourseId.Should().Be(courseId);
        dbFeedback.Content.Should().Be(content);
        dbFeedback.Rating.Should().Be(rating);
    }
    
    [Fact]
    public async Task ShouldNotCreateFeedbackOnCourseBecauseCourseNotFound()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var content = "Test content";
        ushort rating = 3;
        var request = new FeedbackCreateDto(courseId, content, rating);
        
        SetCustomAuthorizationHeader(JwtProvider.Generate(_secondaryUser, _userRole));

        // Act
        var response = await Client.PostAsJsonAsync("feedbacks/add", request);
        
        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task ShouldNotCreateFeedbackOnCourseBecauseUserAlreadyHasOne()
    {
        // Arrange
        var courseId = _mainCourse.Id.Value;
        var content = "Test content";
        ushort rating = 3;
        var request = new FeedbackCreateDto(courseId, content, rating);
        SetCustomAuthorizationHeader(JwtProvider.Generate(_mainUser, _userRole));

        // Act
        var response = await Client.PostAsJsonAsync("feedbacks/add", request);
        
        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
    
    [Fact]
    public async Task ShouldDeleteFeedbackOnCourse()
    {
        // Arrange
        var feedbackId = _mainFeedback.Id;

        // Act
        var response = await Client.DeleteAsync($"feedbacks/delete/{feedbackId}");
        
        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var dbFeedback = await Context.Feedbacks.FirstOrDefaultAsync(x => x.Id == feedbackId);
        dbFeedback.Should().BeNull();
    }
    
    [Fact]
    public async Task ShouldNotDeleteFeedbackOnCourseBecauseFeedbackNotFound()
    {
        // Arrange
        var feedbackId = Guid.NewGuid();

        // Act
        var response = await Client.DeleteAsync($"feedbacks/delete/{feedbackId}");
        
        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    public async Task InitializeAsync()
    {
        await RoleManager.CreateAsync(_userRole);
        await RoleManager.CreateAsync(_adminRole);
        await UserManager.CreateAsync(_mainUser, TestPassword);
        await UserManager.CreateAsync(_secondaryUser, TestPassword);
        await UserManager.AddToRoleAsync(_mainUser, _userRole.Name!);
        await UserManager.AddToRoleAsync(_secondaryUser, _userRole.Name!);
        await Context.Courses.AddRangeAsync(_mainCourse, _secondaryCourse);
        await Context.Registers.AddRangeAsync(_secondaryRegister);
        await Context.Feedbacks.AddAsync(_mainFeedback);
        await SaveChangesAsync();
    }
    
    public async Task DisposeAsync()
    {
        Context.Registers.RemoveRange(Context.Registers);
        Context.Feedbacks.RemoveRange(Context.Feedbacks);
        Context.Courses.RemoveRange(Context.Courses);
        Context.Users.RemoveRange(Context.Users);
        Context.Roles.RemoveRange(Context.Roles);
        await SaveChangesAsync();
    }
}