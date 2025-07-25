using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using People.Api.ApiEndpoints;
using People.Api.ApiModels;
using People.Api.Services;
using System.Net;
using System.Net.Http.Json;
using System.Reflection;

namespace People.Tests;

public class PeopleEndpointsTests
{
    private async Task<(HttpClient client, WebApplication app)> CreateClientAsync(Mock<IPeopleService> mockService)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddSingleton(mockService.Object);

        builder.Services.AddValidatorsFromAssembly(Assembly.Load("People.Api"));

        var app = builder.Build();
        app.MapPeopleEndpoints();

        var port = 5000 + Random.Shared.Next(1000, 9999);
        var url = $"http://localhost:{port}";
        app.Urls.Add(url);
        await app.StartAsync();

        var client = new HttpClient { BaseAddress = new Uri(url) };
        return (client, app);
    }

    private async Task CleanUp(HttpClient client, WebApplication app)
    {
        client.Dispose();
        await app.StopAsync();
    }

    private static List<PersonApi> SamplePeopleApiSet()
    {
        return
        [
            new PersonApi(1, "Alice", new DateOnly(2001, 5, 5)),
            new PersonApi(2, "Bob", new DateOnly(2002, 5, 5))
        ];
    }

    [Fact]
    public async Task GetPeopleShouldReturnOkWithPeopleList()
    {
        // Arrange
        var mockPeopleService = new Mock<IPeopleService>();
        var personApiList = SamplePeopleApiSet();
        
        mockPeopleService.Setup(s => s.GetPeopleAsync())
            .ReturnsAsync(Result<List<PersonApi>>.Ok(personApiList));

        var(client, app) = await CreateClientAsync(mockPeopleService);

        // Act
        var response = await client.GetAsync("/people/");
        var result = await response.Content.ReadFromJsonAsync<List<PersonApi>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeEquivalentTo(personApiList);

        await CleanUp(client, app);
    }

    [Fact]
    public async Task GetPeopleShouldReturnOkWithEmptyPeopleList()
    {
        // Arrange
        var mockService = new Mock<IPeopleService>();
        var emptyPersonApiList = new List<PersonApi>();
        mockService.Setup(s => s.GetPeopleAsync())
            .ReturnsAsync(Result<List<PersonApi>>.Ok(emptyPersonApiList));

        // Act
        var (client, app) = await CreateClientAsync(mockService);

        var response = await client.GetAsync("/people/");
        var result = await response.Content.ReadFromJsonAsync<List<PersonApi>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeEquivalentTo(emptyPersonApiList);

        await CleanUp(client, app);
    }

    [Fact]
    public async Task GetPeopleShouldReturnInternalServerErrorOnFailure()
    {
        // Arrange
        var mockService = new Mock<IPeopleService>();
        mockService.Setup(s => s.GetPeopleAsync())
            .ReturnsAsync(Result<List<PersonApi>>.InternalServerError("User facing"));

        var (client, app) = await CreateClientAsync(mockService);

        // Act
        var response = await client.GetAsync("/people/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        //Cleanup
        await CleanUp(client, app);
    }

    [Fact]
    public async Task GetPersonShouldReturnOkWhenFound()
    {
        // Arrange
        var mockService = new Mock<IPeopleService>();
        var person = new PersonApi(1, "Alice", new DateOnly(2001, 5, 5));
        mockService.Setup(s => s.GetPersonAsync(1))
            .ReturnsAsync(Result<PersonApi>.Ok(person));

        var (client, app) = await CreateClientAsync(mockService);

        // Act
        var response = await client.GetAsync("/people/1");
        var result = await response.Content.ReadFromJsonAsync<PersonApi>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeEquivalentTo(person);

        await CleanUp(client, app);
    }

    [Fact]
    public async Task GetPersonShouldReturnNotFoundWhenMissing()
    {
        // Arrange
        var mockService = new Mock<IPeopleService>();
        mockService.Setup(s => s.GetPersonAsync(99))
            .ReturnsAsync(Result<PersonApi>.NotFound("Not found"));

        var (client, app) = await CreateClientAsync(mockService);

        // Act
        var response = await client.GetAsync("/people/99");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        await CleanUp(client, app);
    }

    [Fact]
    public async Task PostPersonShouldReturnCreatedWhenValid()
    {
        // Arrange
        var mockService = new Mock<IPeopleService>();
        var newPerson = new PersonApi(3, "Charlie", new DateOnly(2003, 3, 3));
        mockService.Setup(s => s.CreatePersonAsync(It.IsAny<PersonApi>()))
            .ReturnsAsync(Result<PersonApi>.Ok(newPerson));

        var (client, app) = await CreateClientAsync(mockService);

        // Act
        var response = await client.PostAsJsonAsync("/people/", newPerson);
        var result = await response.Content.ReadFromJsonAsync<PersonApi>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Should().BeEquivalentTo(newPerson);

        await CleanUp(client, app);
    }

    [Fact]
    public async Task PostPersonShouldReturnBadRequestWhenInvalid()
    {
        // Arrange
        var mockService = new Mock<IPeopleService>();
        mockService.Setup(s => s.CreatePersonAsync(It.IsAny<PersonApi>()))
            .ReturnsAsync(Result<PersonApi>.Invalid("Invalid data"));

        var (client, app) = await CreateClientAsync(mockService);

        // Act
        var response = await client.PostAsJsonAsync("/people/", new PersonApi(0, "", new DateOnly(2000, 1, 1)));

        // Assert   
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        await CleanUp(client, app);
    }

    [Fact]
    public async Task PutPersonShouldReturnOkWhenValid()
    {
        // Arrange
        var mockService = new Mock<IPeopleService>();
        var updatedPerson = new PersonApi(1, "Updated", new DateOnly(2001, 5, 5));
        mockService.Setup(s => s.UpdatePersonAsync(It.IsAny<PersonApi>()))
            .ReturnsAsync(Result<PersonApi>.Ok(updatedPerson));

        var (client, app) = await CreateClientAsync(mockService);

        //
        var response = await client.PutAsJsonAsync("/people/", updatedPerson);
        var result = await response.Content.ReadFromJsonAsync<PersonApi>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeEquivalentTo(updatedPerson);

        await CleanUp(client, app);
    }

    [Fact]
    public async Task PutPersonShouldReturnNotFoundWhenMissing()
    {
        // Arrange
        var mockService = new Mock<IPeopleService>();
        mockService.Setup(s => s.UpdatePersonAsync(It.IsAny<PersonApi>()))
            .ReturnsAsync(Result<PersonApi>.NotFound("Not found"));

        var (client, app) = await CreateClientAsync(mockService);

        // Act
        var response = await client.PutAsJsonAsync("/people/", new PersonApi(99, "Missing", new DateOnly(2000, 1, 1)));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Cleanup
        await CleanUp(client, app);
    }

    [Fact]
    public async Task DeletePersonShouldReturnNoContentWhenFound()
    {
        // Arrange
        var mockService = new Mock<IPeopleService>();
        var person = new PersonApi(1, "ToDelete", new DateOnly(2001, 5, 5));
        mockService.Setup(s => s.DeletePersonAsync(1))
            .ReturnsAsync(Result<PersonApi>.Ok(person));

        var (client, app) = await CreateClientAsync(mockService);

        // Act
        var response = await client.DeleteAsync("/people/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        await CleanUp(client, app);
    }

    [Fact]
    public async Task DeletePersonShouldReturnNotFoundWhenMissing()
    {
        // Arrange
        var mockService = new Mock<IPeopleService>();
        mockService.Setup(s => s.DeletePersonAsync(99))
            .ReturnsAsync(Result<PersonApi>.NotFound("Not found"));

        var (client, app) = await CreateClientAsync(mockService);

        // Act
        var response = await client.DeleteAsync("/people/99");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        await CleanUp(client, app);
    }
}