using System.Net;
using System.Net.Http.Json;
using Api.Dtos;
using Domain.Categories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tests.Common;
using Tests.Data;


namespace Api.Tests.Integration.Categories;

public class CategoriesControllerTests : BaseIntegrationTest, IAsyncLifetime
{
    private readonly Category _mainCategory = CategoriesData.MainCategory;
    private readonly Category _secondaryCategory = CategoriesData.SecondaryCategory;

    public CategoriesControllerTests(IntegrationTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task ShouldCreateCategory()
    {
        // Arrange
        const string categoryName = "Test category name";
        var request = new CategoryCreateDto(categoryName);

        // Act
        var response = await Client.PostAsJsonAsync("categories/add", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var responseCategory = await response.ToResponseModel<CategoryDto>();
        var categoryId = new CategoryId(responseCategory.Id);

        var dbCategory = await Context.Categories.FirstAsync(x => x.Id == categoryId);
        dbCategory.Should().NotBeNull();
        dbCategory.Name.Should().Be(categoryName);
    }

    [Fact]
    public async Task ShouldNotCreateCategoryBecauseNameDuplicated()
    {
        // Arrange
        var categoryName = _mainCategory.Name;
        var request = new CategoryCreateDto(categoryName);

        // Act
        var response = await Client.PostAsJsonAsync("categories/add", request);

        // Assert 
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task ShouldUpdateCategory()
    {
        // Arrange
        const string categoryName = "Updated test category name";
        var request = new CategoryDto(_mainCategory.Id.Value, categoryName);

        // Act
        var response = await Client.PutAsJsonAsync("categories/update", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var responseCategory = await response.ToResponseModel<CategoryDto>();
        var categoryId = new CategoryId(responseCategory.Id);

        var dbCategory = await Context.Categories.FirstAsync(x => x.Id == categoryId);
        dbCategory.Should().NotBeNull();
        dbCategory.Name.Should().Be(categoryName);
    }

    [Fact]
    public async Task ShouldNotUpdateCategoryBecauseNameDuplicated()
    {
        // Arrange
        var categoryName = _secondaryCategory.Name;
        var request = new CategoryDto(_mainCategory.Id.Value, categoryName);

        // Act
        var response = await Client.PutAsJsonAsync("categories/update", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task ShouldNotUpdateCategoryBecauseCategoryNotFound()
    {
        // Arrange
        const string categoryName = "Updated category name";
        var request = new CategoryDto(Guid.NewGuid(), categoryName);

        // Act
        var response = await Client.PutAsJsonAsync("categories/update", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldDeleteCategory()
    {
        // Arrange
        var categoryId = _secondaryCategory.Id;

        // Act
        var response = await Client.DeleteAsync($"categories/delete/{categoryId}");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var responseCategory = await response.ToResponseModel<CategoryDto>();
        responseCategory.Id.Should().Be(categoryId.Value);

        var dbCategory = await Context.Categories.FirstOrDefaultAsync(x => x.Id == categoryId);
        dbCategory.Should().BeNull();
    }

    [Fact]
    public async Task ShouldNotDeleteCategoryBecauseCategoryNotFound()
    {
        // Arrange
        var categoryId = Guid.NewGuid();

        // Act
        var response = await Client.DeleteAsync($"categories/delete/{categoryId}");

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    //ToDo: Test category delete if has courses

    public async Task InitializeAsync()
    {
        await Context.Categories.AddRangeAsync(_mainCategory, _secondaryCategory);
        await SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        Context.Categories.RemoveRange(Context.Categories);
        await SaveChangesAsync();
    }
}