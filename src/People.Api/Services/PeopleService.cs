using Microsoft.EntityFrameworkCore;
using People.Api.ApiModels;
using People.Api.Mappers;
using People.Data.Context;

namespace People.Api.Services;

public class PeopleService(Context context, ILogger<PeopleService> logger) : IPeopleService
{
    private readonly Context _context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<Result<List<PersonApi>>> GetPeopleAsync()
    {
        try
        { 
            var apiPeople = await _context.MyEntities
                .Select(PeopleMappings.AsApiModel)
                .ToListAsync();
            
            return apiPeople;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving people from the database.");
            return Result<List<PersonApi>>.InternalServerError(ex.Message);
        }
    }

    public async Task<Result<PersonApi>> GetPersonAsync(int id)
    {
        try
        {
            var person = await _context.MyEntities.FindAsync(id);

            return person?.ToApiModel() 
                   ?? Result<PersonApi>.NotFound($"Person with ID {id} not found.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving Person with id {personId} from the database.", id);
            return Result<PersonApi>.InternalServerError(ex.Message);
        }
    }

    public async Task<Result<PersonApi>> CreatePersonAsync(PersonApi personApi)
    {
        try
        {
            var toAdd = personApi.ToEntity();
            _context.MyEntities.Add(toAdd);
            await _context.SaveChangesAsync();
            return toAdd.ToApiModel();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while adding a new Person to the database.");
            return Result<PersonApi>.InternalServerError(ex.Message);
        }
    }

    public async Task<Result<PersonApi>> UpdatePersonAsync(PersonApi personApi)
    {
        try
        {
            var existingPerson = await _context.MyEntities.FindAsync(personApi.Id);

            if (existingPerson == null)
            {
                return Result<PersonApi>.NotFound($"Person with ID {personApi.Id} not found.");
            }
            
            existingPerson.Name = personApi.Name;
            existingPerson.DateOfBirth = personApi.DateOfBirth;
            
            _context.MyEntities.Update(existingPerson);
            
            await _context.SaveChangesAsync();
            
            return existingPerson.ToApiModel();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating Person with id {personId} in the database.", personApi.Id);
            return Result<PersonApi>.InternalServerError(ex.Message);
        }
    }

    public async Task<Result<PersonApi>> DeletePersonAsync(int id)
    {
        try
        {
            var toDelete = await _context.MyEntities.FindAsync(id);

            if (toDelete == null)
            {
                return Result<PersonApi>.NotFound($"Person with ID {id} not found.");
            }
            
            _context.MyEntities.Remove(toDelete);
            await _context.SaveChangesAsync();
            return toDelete.ToApiModel();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting Person with id {personId} from the database.", id);
            return Result<PersonApi>.InternalServerError(ex.Message);
        }
    }
}