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
    public Task<ClientGetDto> AddClientToTripAsync(int tripId, ClientTripGetDto client);
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

    public async Task<ClientGetDto> AddClientToTripAsync(int tripId, ClientTripGetDto client)
    {
        if (await data.Clients.Where(ct => ct.Pesel == client.Pesel).AnyAsync())
        {
            throw new ClientExists($"Client with pesel {client.Pesel} already exists");
        }
        // punkt 2.2 "Czy klient o takim numerze PESEL jest już zapisany na
        // daną wycieczkę - jeśli tak, zwracamy błąd"
        // już jest wykluczone, jako że klient nie powinien istnieć w bazie w ogóle
        // to nie może być przypisany do wycieczki
        
        var trip = await data.Trips.Where(t => t.IdTrip == tripId).Include(ct => ct.ClientTrips).FirstOrDefaultAsync();

        if (trip == null)
        {
            throw new NotFoundException($"Trip with id {tripId} not found");
        }

        if (trip.DateFrom <= DateTime.Now)
        {
            throw new TripIsOnGoingException("Trip has already started");
        }

        var newClient = new Client
        {
            FirstName = client.FirstName,
            LastName = client.LastName,
            Email = client.Email,
            Telephone = client.Telephone,
            Pesel = client.Pesel
        };
        
        await data.Clients.AddAsync(newClient);
        var createdClient = data.Clients.Where(c => c.Pesel == newClient.Pesel).FirstOrDefaultAsync();

        var newClientTrip = new ClientTrip
        {
            IdClient = createdClient.Id,
            IdTrip = tripId,
            RegisteredAt = DateTime.Today,
            PaymentDate = client.PaymentDate ?? null
        };
        
        await data.ClientTrips.AddAsync(newClientTrip);
        await data.SaveChangesAsync();

        return new ClientGetDto
        {
            IdClient = createdClient.Id,
            FirstName = newClient.FirstName,
            LastName = newClient.LastName,
            Email = newClient.Email,
            Telephone = newClient.Telephone,
            Pesel = newClient.Pesel
        };
    }
}