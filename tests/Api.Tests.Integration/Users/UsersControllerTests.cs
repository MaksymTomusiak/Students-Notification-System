using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using Api.Dtos;
using Domain.Categories;
using Domain.CourseCategories;
using Domain.Courses;
using Domain.Registers;
using Domain.Roles;
using Domain.Users;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Tests.Common;
using Tests.Data;

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
    private readonly Course _secondaryCourse;
    private readonly CourseCategory _mainCourseCategory;
    private readonly Register _mainRegister;
    private readonly Register _secondaryRegister;
    private const string TestPassword = "TestPass123!";
    
    public UsersControllerTests(IntegrationTestWebFactory factory) : base(factory)
    {
        _mainUser = UsersData.MainUser();
        _secondaryUser = UsersData.SecondaryUser();
        _testAdminUser = UsersData.AdminUser();
        _mainCourse = CoursesData.MainCourse(_mainUser.Id);
        _secondaryCourse = CoursesData.SecondaryCourse(_mainUser.Id);
        _mainCourseCategory = CourseCategoriesData.New(_mainCourse.Id, _mainCategory.Id);
        _mainRegister = RegistersData.New(_testAdminUser.Id, _mainCourse.Id);
        _secondaryRegister = RegistersData.New(_secondaryUser.Id, _mainCourse.Id);
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
        // Arrange
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
    
    [Fact]
    public async Task ShouldEnrollUserOnCourse()
    {
        // Arrange
        var courseId = _secondaryCourse.Id;
        SetCustomAuthorizationHeader(JwtProvider.Generate(_secondaryUser, _userRole));

        // Act
        var response = await Client.PostAsync($"users/enroll-on-course/{courseId}", null);
        
        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var responseRegister = await response.ToResponseModel<RegisterDto>();
        var registerId = new RegisterId(responseRegister.Id);
        
        // Retrieve the register from the database
        var dbRegister = await Context.Registers.FirstOrDefaultAsync(x => x.Id == registerId);
        dbRegister.Should().NotBeNull();
        dbRegister!.UserId.Should().Be(_secondaryUser.Id);
        dbRegister.CourseId.Should().Be(courseId);
    }
    
    [Fact]
    public async Task ShouldNotEnrollUserBecauseAlreadyEnrolledOnCourse()
    {
        // Arrange
        var courseId = _mainCourse.Id;
        SetCustomAuthorizationHeader(JwtProvider.Generate(_testAdminUser, _userRole));

        // Act
        var response = await Client.PostAsync($"users/enroll-on-course/{courseId}", null);
        
        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
    
    [Fact]
    public async Task ShouldNotEnrollUserBecauseCourseNotFound()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        SetCustomAuthorizationHeader(JwtProvider.Generate(_secondaryUser, _userRole));

        // Act
        var response = await Client.PostAsync($"users/enroll-on-course/{courseId}", null);
        
        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task ShouldUnregisterUserFromCourse()
    {
        // Arrange
        var courseId = _mainCourse.Id;
        SetCustomAuthorizationHeader(JwtProvider.Generate(_testAdminUser, _userRole));

        // Act
        var response = await Client.DeleteAsync($"users/unregister-from-course/{courseId}");
        
        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var responseRegister = await response.ToResponseModel<RegisterDto>();
        var registerId = new RegisterId(responseRegister.Id);
        
        // Retrieve the register from the database
        var dbRegister = await Context.Registers.FirstOrDefaultAsync(x => x.Id == registerId);
        dbRegister.Should().BeNull();
    }
    
    [Fact]
    public async Task ShouldNotUnregisterUserFromCourseBecauseUserNotRegistered()
    {
        // Arrange
        var courseId = _mainCourse.Id;
        SetCustomAuthorizationHeader(JwtProvider.Generate(_mainUser, _userRole));

        // Act
        var response = await Client.DeleteAsync($"users/unregister-from-course/{courseId}");
        
        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
    
    [Fact]
    public async Task ShouldNotUnregisterUserFromCourseBecauseCourseNotFound()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        SetCustomAuthorizationHeader(JwtProvider.Generate(_testAdminUser, _userRole));

        // Act
        var response = await Client.DeleteAsync($"users/unregister-from-course/{courseId}");
        
        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task ShouldUpdateUser()
    {
        // Arrange
        const string phoneNumber = "+38 012-345-6789";
        var oldPassword = TestPassword;
        var newPassword = "Updated" + TestPassword;
        var request = new UserUpdateDto(
            phoneNumber,
            oldPassword,
            newPassword);
        SetCustomAuthorizationHeader(JwtProvider.Generate(_mainUser, _userRole));

        // Act
        var response = await Client.PutAsJsonAsync("users/update", request);
        
        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var responseUser = await response.ToResponseModel<UserDto>();
        
        // Retrieve the user from the database
        var dbUser = await Context.Users.FirstOrDefaultAsync(x => x.Id == responseUser.Id);
        dbUser.Should().NotBeNull();
        dbUser!.PhoneNumber.Should().Be(phoneNumber);
        UserManager.PasswordHasher.VerifyHashedPassword(dbUser, dbUser.PasswordHash!, newPassword)
            .Should().Be(PasswordVerificationResult.Success);
    }
    
    [Fact]
    public async Task ShouldNotUpdateUserBecausePasswordIsWrong()
    {
        // Arrange
        const string phoneNumber = "+38 012-345-6789";
        var oldPassword = "Wrong" +TestPassword;
        var newPassword = "Updated" + TestPassword;
        var request = new UserUpdateDto(
            phoneNumber,
            oldPassword,
            newPassword);
        
        // Act
        var response = await Client.PutAsJsonAsync("users/update", request);
        
        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
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
        await Context.Courses.AddRangeAsync(_mainCourse, _secondaryCourse);
        await Context.CourseCategories.AddAsync(_mainCourseCategory);
        await Context.Registers.AddRangeAsync(_mainRegister, _secondaryRegister);
        await SaveChangesAsync();
    }
    
    public async Task DisposeAsync()
    {
        Context.Registers.RemoveRange(Context.Registers);
        Context.CourseCategories.RemoveRange(Context.CourseCategories);
        Context.Courses.RemoveRange(Context.Courses);
        Context.Categories.RemoveRange(Context.Categories);
        Context.Users.RemoveRange(Context.Users);
        Context.Roles.RemoveRange(Context.Roles);
        await SaveChangesAsync();
    }
}