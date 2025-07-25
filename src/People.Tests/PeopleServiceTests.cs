using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using People.Api.ApiModels;
using People.Api.Services;
using People.Data.Context;
using People.Data.Entities;

namespace People.Tests;

public class PeopleServiceTests
{
    private readonly Mock<Context> _mockContext;
    private readonly PeopleService _peopleService;

    public PeopleServiceTests()
    {
        _mockContext = new Mock<Context>(new DbContextOptions<Context>());
        var mockLogger = new Mock<ILogger<PeopleService>>();
        _peopleService = new PeopleService(_mockContext.Object, mockLogger.Object);
    }

    private static List<Person> SamplePeopleDataSet()
    {
        return
        [
            new Person { Id = 1, Name = "Alice", DateOfBirth = new DateOnly(2001, 5, 5) },
            new Person { Id = 2, Name = "Bob", DateOfBirth = new DateOnly(2002, 5, 5) },
        ];
    }

    [Fact]
    public async Task GetPeopleAsyncShouldReturnAllPeopleInDataset()
    {
        // Arrange
        var dataset = SamplePeopleDataSet().AsQueryable();
        var mockSet = dataset.BuildMockDbSet();
        _mockContext.Setup(c => c.MyEntities).Returns(mockSet.Object);

        // Act
        var result = await _peopleService.GetPeopleAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeNull();
        result.Value.Should().BeEquivalentTo(dataset);
    }

    [Fact]
    public async Task GetPeopleAsyncShouldReturnEmptyListWhenDatasetEmpty()
    {
        // Arrange
        var emptyDataset = new List<Person>().AsQueryable();
        var mockSet = emptyDataset.BuildMockDbSet();
        _mockContext.Setup(c => c.MyEntities).Returns(mockSet.Object);

        // Act
        var result = await _peopleService.GetPeopleAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeNull();
        result.Value.Should().BeEquivalentTo(emptyDataset);
    }

    [Fact]
    public async Task GetPeopleAsyncShouldReturnErrorResultOnFailure()
    {
        // Arrange
        _mockContext.Setup(c => c.MyEntities).Throws(new Exception("Any db access error"));

        // Act
        var result = await _peopleService.GetPeopleAsync();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.ErrorType.Should().Be(ErrorType.InternalServerError);
    }

    [Fact]
    public async Task GetPersonAsyncShouldReturnPersonWhenFound()
    {
        // Arrange
        var dataset = SamplePeopleDataSet().AsQueryable();
        var found = dataset.First();
        var mockSet = dataset.BuildMockDbSet();
        _mockContext.Setup(c => c.MyEntities).Returns(mockSet.Object);
        mockSet.Setup(x => x.FindAsync(It.IsAny<int>()))
            .ReturnsAsync(found);

        // Act
        var result = await _peopleService.GetPersonAsync(found.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEquivalentTo(found);
    }

    [Fact]
    public async Task GetPersonAsyncShouldReturnNotFoundWhenMissing()
    {
        // Arrange
        var dataset = SamplePeopleDataSet().AsQueryable();
        var mockSet = dataset.BuildMockDbSet();
        _mockContext.Setup(c => c.MyEntities).Returns(mockSet.Object);
        mockSet.Setup(x => x.FindAsync(It.IsAny<int>()))
            .ReturnsAsync((Person?)null);

        // Act
        var result = await _peopleService.GetPersonAsync(1);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.ErrorType.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task GetPersonAsyncShouldReturnErrorResultOnFailure()
    {
        // Arrange
        _mockContext.Setup(c => c.MyEntities).Throws(new Exception("Any db access error"));

        // Act
        var result = await _peopleService.GetPersonAsync(1);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.ErrorType.Should().Be(ErrorType.InternalServerError);
    }

    [Fact]
    public async Task CreatePersonAsyncShouldReturnApiModelOnSuccess()
    {
        // Arrange
        var personToAdd = new PersonApi(0, "Charlie", new DateOnly(1990, 1, 1));
        var dataset = new List<Person>().AsQueryable();
        
        var mockSet = dataset.BuildMockDbSet();
        mockSet.Setup(m => m.AddAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Person p, CancellationToken ct) =>
            {
                p.Id = 1;
                return null!; // value is unused
            });
        _mockContext.Setup(c => c.MyEntities).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _peopleService.CreatePersonAsync(personToAdd);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("Charlie");
        result.Value.DateOfBirth.Should().Be(new DateOnly(1990, 1, 1));
    }

    [Fact]
    public async Task CreatePersonAsyncShouldReturnErrorResultOnFailure()
    {
        // Arrange
        var personToAdd = new PersonApi(0, "Charlie", new DateOnly(1990, 1, 1));
        _mockContext.Setup(c => c.MyEntities).Throws(new Exception("Any db access error"));

        // Act
        var result = await _peopleService.CreatePersonAsync(personToAdd);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.ErrorType.Should().Be(ErrorType.InternalServerError);
    }

    [Fact]
    public async Task UpdatePersonAsyncShouldUpdatePersonWhenFound()
    {
        // Arrange
        var dataset = SamplePeopleDataSet().AsQueryable();
        var mockSet = dataset.BuildMockDbSet();
        _mockContext.Setup(c => c.MyEntities).Returns(mockSet.Object);
        var personToUpdate = dataset.First();

        mockSet.Setup(x => x.FindAsync(It.IsAny<int>()))
            .ReturnsAsync(personToUpdate);
        var apiModelWithUpdates =
            new PersonApi(personToUpdate.Id, "UpdatedName", new DateOnly(1999,12,1));

        // Act
        var result = await _peopleService.UpdatePersonAsync(apiModelWithUpdates);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEquivalentTo(apiModelWithUpdates);
    }

    [Fact]
    public async Task UpdatePersonAsyncShouldReturnNotFoundWhenMissing()
    {
        // Arrange
        var dataset = SamplePeopleDataSet().AsQueryable();
        var mockSet = dataset.BuildMockDbSet();
        _mockContext.Setup(c => c.MyEntities).Returns(mockSet.Object);
        mockSet.Setup(x => x.FindAsync(It.IsAny<int>()))
            .ReturnsAsync((Person?)null);

        var personToUpdate = dataset.First();
        // Act
        var result = await _peopleService.UpdatePersonAsync(new PersonApi(100, "NotInSet", new DateOnly(2000, 1, 1)));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.ErrorType.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task UpdatePersonAsyncShouldReturnErrorResultOnFailure()
    {
        var emptyDataset = SamplePeopleDataSet().AsQueryable();
        var mockSet = emptyDataset.BuildMockDbSet();
        _mockContext.Setup(c => c.MyEntities).Returns(mockSet.Object);
        _mockContext.Setup(c => c.MyEntities).Throws(new Exception("Any db access error"));

        // Act
        var result =
            await _peopleService.UpdatePersonAsync(new PersonApi(100, "UpdatedName", new DateOnly(2000, 1, 1)));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.ErrorType.Should().Be(ErrorType.InternalServerError);
    }

    [Fact]
    public async Task DeletePersonAsyncShouldRemovePersonWhenFound()
    {
        // Arrange
        var dataset = SamplePeopleDataSet().AsQueryable();
        var mockSet = dataset.BuildMockDbSet();
        _mockContext.Setup(c => c.MyEntities).Returns(mockSet.Object);
        var personToDelete = dataset.First();
        mockSet.Setup(x => x.FindAsync(It.IsAny<int>()))
            .ReturnsAsync(personToDelete);
        var existingIdToDelete = personToDelete.Id;

        // Act
        var result = await _peopleService.DeletePersonAsync(existingIdToDelete);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEquivalentTo(personToDelete);
    }

    [Fact]
    public async Task DeletePersonAsyncShouldReturnNotFoundWhenMissing()
    {
        // Arrange
        var dataset = SamplePeopleDataSet().AsQueryable();
        var mockSet = dataset.BuildMockDbSet();
        _mockContext.Setup(c => c.MyEntities).Returns(mockSet.Object);

        var existingIdToDelete = dataset.Max(d => d.Id) + 1;

        // Act
        var result = await _peopleService.DeletePersonAsync(existingIdToDelete);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.ErrorType.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task DeletePersonAsyncShouldReturnErrorResultOnFailure()
    {
        // Arrange
        _mockContext.Setup(c => c.MyEntities).Throws(new Exception("Any db access error"));
        
        // Act
        var result = await _peopleService.DeletePersonAsync(1);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.ErrorType.Should().Be(ErrorType.InternalServerError);
    }
}