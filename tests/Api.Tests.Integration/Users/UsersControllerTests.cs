using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using Domain.Categories;
using Domain.CourseCategories;
using Domain.Courses;
using Domain.Roles;
using Domain.Users;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Tests.Common;
using Tests.Data;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Api.Tests.Integration.Users;

public class UsersControllerTests: BaseIntegrationTest, IAsyncLifetime
{
    private readonly Category _mainCategory = CategoriesData.MainCategory;
    private readonly Category _secondaryCategory = CategoriesData.SecondaryCategory;
    private readonly User _mainUser;
    private readonly User _secondaryUser;
    private readonly User _testAdminUser;
    private readonly Role _userRole = RolesData.UserRole;
    private readonly Role _adminRole = RolesData.AdminRole;
    private readonly Course _mainCourse;
    private readonly CourseCategory _mainCourseCategory;
    private const string TestPassword = "TestPass123!";
    
    public UsersControllerTests(IntegrationTestWebFactory factory) : base(factory)
    {
        _mainUser = UsersData.MainUser();
        _secondaryUser = UsersData.SecondaryUser();
        _testAdminUser = UsersData.AdminUser();
        _mainCourse = CoursesData.MainCourse(_mainUser.Id);
        _mainCourseCategory = CourseCategoriesData.New(_mainCourse.Id, _mainCategory.Id);
    }

    [Fact]
    public async Task ShouldRegisterUser()
    {
        // Arrange
        const string userName = "testUserName";
        const string userEmail = "testUser@gmail.com";
        const string password = "TestPass123!";
        var request = new
        {
            UserName = userName,
            Email = userEmail,
            Password = password
        };

        // Act
        var response = await Client.PostAsJsonAsync("users/register", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        
        // Retrieve the user from the database
        var dbUser = await Context.Users.FirstOrDefaultAsync(x => x.Email == userEmail);
        dbUser.Should().NotBeNull();
        dbUser!.UserName.Should().Be(userName);
        dbUser.Email.Should().Be(userEmail);
        var isUserInRole = await UserManager.IsInRoleAsync(dbUser, "User");
        isUserInRole.Should().BeTrue();
        UserManager.PasswordHasher.VerifyHashedPassword(dbUser, dbUser.PasswordHash!, password)
            .Should().Be(PasswordVerificationResult.Success);
        
        // Verify the token
        var handler = new JwtSecurityTokenHandler();
        var responseToken = await response.Content.ReadAsStringAsync();
        responseToken.Should().NotBeNullOrEmpty();

        var token = handler.ReadJwtToken(responseToken);
        token.Should().NotBeNull();
        token.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == dbUser.Id.ToString());
        token.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == dbUser.Email);
        token.Claims.Should().Contain(c => c.Type == "role" && c.Value == "User");
    }
    
    [Fact]
    public async Task ShouldNotRegisterBecauseEmailIsAlreadyUsed()
    {
        // Arrange
        const string userName = "testUserName";
        var userEmail = _mainUser.Email;
        const string password = "TestPass123!";
        var request = new
        {
            UserName = userName,
            Email = userEmail,
            Password = password
        };

        // Act
        var response = await Client.PostAsJsonAsync("users/register", request);

        // Assert 
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task ShouldNotRegisterBecauseUserNameIsAlreadyUsed()
    {
        // Arrange
        var userName = _mainUser.UserName;
        const string userEmail = "testUser@gmail.com";
        const string password = "TestPass123!";
        var request = new
        {
            UserName = userName,
            Email = userEmail,
            Password = password
        };

        // Act
        var response = await Client.PostAsJsonAsync("users/register", request);

        // Assert 
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task ShouldLoginUser()
    {
        // Arrange
        var userEmail = _mainUser.Email;
        var password = TestPassword;
        var request = new
        {
            Email = userEmail,
            Password = password
        };

        // Act
        var response = await Client.PostAsJsonAsync("users/login", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        // Verify the token
        var handler = new JwtSecurityTokenHandler();
        var responseToken = await response.Content.ReadAsStringAsync();
        responseToken.Should().NotBeNullOrEmpty();

        var token = handler.ReadJwtToken(responseToken);
        token.Should().NotBeNull();
        token.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == _mainUser.Id.ToString());
        token.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == userEmail);
        token.Claims.Should().Contain(c => c.Type == "role" && c.Value == "User");
    }
    
    [Fact]
    public async Task ShouldNotLoginUserBecauseWrongPassword()
    {
        // Arrange
        var userEmail = _mainUser.Email;
        const string password = "WrongPass123!";
        var request = new
        {
            Email = userEmail,
            Password = password
        };

        // Act
        var response = await Client.PostAsJsonAsync("users/login", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    
    [Fact]
    public async Task ShouldNotLoginUserBecauseWrongEmail()
    {
        // Arrange
        const string userEmail = "notexistinguser@gmail.com";
        const string password = "SomePass123!";
        var request = new
        {
            Email = userEmail,
            Password = password
        };

        // Act
        var response = await Client.PostAsJsonAsync("users/login", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    
    [Fact]
    public async Task ShouldDeleteUsualUserByAdmin()
    {
        // Arrange
        var userId = _secondaryUser.Id;

        // Act
        var response = await Client.DeleteAsync($"users/delete/{userId}");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        
        // Retrieve the user from the database
        var dbUser = await Context.Users.FirstOrDefaultAsync(x => x.Id == userId);
        dbUser.Should().BeNull();
    }

    [Fact]
    public async Task ShouldDeleteUsualUserByThemselves()
    {
        // Arrange
        var userId = _secondaryUser.Id;
        SetCustomAuthorizationHeader(JwtProvider.Generate(_secondaryUser, _userRole));

        // Act
        var response = await Client.DeleteAsync($"users/delete/{userId}");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        
        // Retrieve the user from the database
        var dbUser = await Context.Users.FirstOrDefaultAsync(x => x.Id == userId);
        dbUser.Should().BeNull();
    }
    
    [Fact]
    public async Task ShouldDeleteAdminUserByThemselves()
    {
        // Arrange
        var userId = _testAdminUser.Id;
        SetCustomAuthorizationHeader(JwtProvider.Generate(_testAdminUser, _adminRole));

        // Act
        var response = await Client.DeleteAsync($"users/delete/{userId}");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        
        // Retrieve the user from the database
        var dbUser = await Context.Users.FirstOrDefaultAsync(x => x.Id == userId);
        dbUser.Should().BeNull();
    }
    
    [Fact]
    public async Task ShouldNotDeleteUsualUserByAnotherUsualUser()
    {
        // Arrange
        var userId = _mainUser.Id;
        SetCustomAuthorizationHeader(JwtProvider.Generate(_secondaryUser, _userRole));

        // Act
        var response = await Client.DeleteAsync($"users/delete/{userId}");

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        
        // Retrieve the user from the database
        var dbUser = await Context.Users.FirstOrDefaultAsync(x => x.Id == userId);
        dbUser.Should().NotBeNull();
    }
    
    [Fact]
    public async Task ShouldNotDeleteAdminByUsualUser()
    {
        // Arrange
        var userId = _testAdminUser.Id;
        SetCustomAuthorizationHeader(JwtProvider.Generate(_secondaryUser, _userRole));

        // Act
        var response = await Client.DeleteAsync($"users/delete/{userId}");

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        
        // Retrieve the user from the database
        var dbUser = await Context.Users.FirstOrDefaultAsync(x => x.Id == userId);
        dbUser.Should().NotBeNull();
    }
    
    [Fact]
    public async Task ShouldNotDeleteAdminByAnotherAdminUser()
    {
        // Arrange4
        var userId = _testAdminUser.Id;

        // Act
        var response = await Client.DeleteAsync($"users/delete/{userId}");

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        
        // Retrieve the user from the database
        var dbUser = await Context.Users.FirstOrDefaultAsync(x => x.Id == userId);
        dbUser.Should().NotBeNull();
    }
    
    public async Task InitializeAsync()
    {
        await RoleManager.CreateAsync(_userRole);
        await RoleManager.CreateAsync(_adminRole);
        await UserManager.CreateAsync(_mainUser, TestPassword);
        await UserManager.CreateAsync(_secondaryUser, TestPassword);
        await UserManager.CreateAsync(_testAdminUser, TestPassword);
        await UserManager.AddToRoleAsync(_mainUser, _userRole.Name!);
        await UserManager.AddToRoleAsync(_secondaryUser, _userRole.Name!);
        await UserManager.AddToRoleAsync(_testAdminUser, _adminRole.Name!);
        await Context.Categories.AddRangeAsync(_mainCategory, _secondaryCategory);
        await Context.Courses.AddAsync(_mainCourse);
        await Context.CourseCategories.AddAsync(_mainCourseCategory);
        await SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        Context.CourseCategories.RemoveRange(Context.CourseCategories);
        Context.Courses.RemoveRange(Context.Courses);
        Context.Categories.RemoveRange(Context.Categories);
        Context.Users.RemoveRange(Context.Users);
        Context.Roles.RemoveRange(Context.Roles);
        await SaveChangesAsync();
    }
}