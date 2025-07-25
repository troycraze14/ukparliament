using People.Api.ApiModels;
using People.Api.Services;
using People.Api.Validators;

namespace People.Api.ApiEndpoints;

public static class PeopleEndpoints
{
    public const string Group = "/people";

    public static void MapPeopleEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup(Group)
            .WithTags("People")
            .WithOpenApi();

        group.MapGet("/", async (IPeopleService peopleService) =>
        {
            var people = await peopleService.GetPeopleAsync();
            return people.Match(
                onSuccess: Results.Ok,
                onFailure: ErrorResult);
        }).WithName("GetPeople")
            .WithSummary("Get a list of all people");

        group.MapGet("/{id:int}", async (int id, IPeopleService peopleService) =>
        {
            var person = await peopleService.GetPersonAsync(id);
            return person.Match(
                onSuccess: Results.Ok,
                onFailure: ErrorResult);
        }).WithName("GetPerson")
        .WithSummary("Get person by id");

        group.MapPost("/", async (PersonApi personApi, IPeopleService service) =>
        {
            var created = await service.CreatePersonAsync(personApi);
            return created.Match(
                onSuccess: (newPerson) => Results.CreatedAtRoute("GetPerson", new { id = newPerson.Id }, newPerson),
                onFailure: ErrorResult);
        }).WithName("Create Person")
            .WithSummary("Create a new Person record")
            .WithRequestValidation<PersonApi>();

        group.MapPut("/", async (PersonApi personApi, IPeopleService service) =>
        {
            var updated = await service.UpdatePersonAsync(personApi);
            return updated.Match(
                onSuccess: Results.Ok,
                onFailure: ErrorResult);
        }).WithName("UpdatePerson")
            .WithSummary("Updates an existing Person record")
            .WithRequestValidation<PersonApi>();

        group.MapDelete("/{id:int}", async (int id, IPeopleService peopleService) =>
        {
            var deleted = await peopleService.DeletePersonAsync(id);
            return deleted.Match(
                onSuccess: (d) => Results.NoContent(),
                onFailure: ErrorResult);
        }).WithName("DeletePerson")
            .WithSummary("Deletes an existing Person record");

        return; 
        
        static IResult ErrorResult(ErrorResponse error) => error.ErrorType switch
        {
            ErrorType.NotFound => Results.NotFound(error.Error),
            ErrorType.Invalid => Results.BadRequest(error.Error),
            ErrorType.InternalServerError => Results.StatusCode(500),
            _ => Results.Problem("An unexpected error occurred.")
        };
    }

}