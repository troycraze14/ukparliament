using People.Api.ApiModels;

namespace People.Api.Services;

public interface IPeopleService
{
    Task<Result<List<PersonApi>>> GetPeopleAsync();
    Task<Result<PersonApi>> GetPersonAsync(int id);
    Task<Result<PersonApi>> CreatePersonAsync(PersonApi personApi);
    Task<Result<PersonApi>> UpdatePersonAsync(PersonApi personApi);
    Task<Result<PersonApi>> DeletePersonAsync(int id);

}