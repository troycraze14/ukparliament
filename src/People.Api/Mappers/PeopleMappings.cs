using System.Linq.Expressions;
using People.Api.ApiModels;
using People.Data.Entities;

namespace People.Api.Mappers;

public static class PeopleMappings
{
    public static PersonApi ToApiModel(this Person person) => new(person.Id, person.Name, person.DateOfBirth);

    public static Expression<Func<Person, PersonApi>> AsApiModel
    {
        get { return person => new PersonApi(person.Id, person.Name, person.DateOfBirth); }
    }

    public static Person ToEntity(this PersonApi personApi)
        => new()
        {
            Id = personApi.Id,
            Name = personApi.Name,
            DateOfBirth = personApi.DateOfBirth
        };

}