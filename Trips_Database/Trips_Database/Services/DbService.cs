using Microsoft.EntityFrameworkCore;
using Trips_Database.Data;
using Trips_Database.DTOs;
using Trips_Database.Exceptions;
using Trips_Database.Models;

namespace Trips_Database.Services;

public interface IDbService
{
    public Task<ICollection<TripsGetDto>> GetTripsAsync();
    public Task RemoveClientByIdAsync(int clientId);
}

public class DbService(MasterContext data) : IDbService
{
    public async Task<ICollection<TripsGetDto>> GetTripsAsync()
    {
        var countries = await data.Countries.ToListAsync();
        var trips = await data.Trips.ToListAsync();
        var clients = await data.Clients.Include(client => client.ClientTrips).ToListAsync();
        
        var result = trips.Select(tr => new TripsGetDto
        {
            Name = tr.Name,
            Description = tr.Description,
            DateFrom = tr.DateFrom,
            DateTo = tr.DateTo,
            MaxPeople = tr.MaxPeople,
            Countries = countries.Where(ct => ct.IdTrips.Any(t => t.IdTrip == tr.IdTrip))
                .Select(ct => new CountryGetDto
            {
                Name = ct.Name
            }).ToList(),
            Clients = clients.Where(cl => cl.ClientTrips.Any(ct => ct.IdTrip == tr.IdTrip))
                .Select(cl => new ClientGetDto
            {
                FirstName = cl.FirstName,
                LastName = cl.LastName
            }).ToList(),
        }).ToList();
        
        return result;
    }

    public async Task RemoveClientByIdAsync(int clientId)
    {
        if (await data.ClientTrips.AnyAsync(ct => ct.IdClient == clientId))
        {
            throw new TripsExistException("Client can't be removed, trips exist");
        }

        var affectedRows = await data.Clients.Where(ct => ct.IdClient == clientId).ExecuteDeleteAsync();
        if (affectedRows == 0)
        {
            throw new NotFoundException($"Client {clientId} not found");
        }

    }
}