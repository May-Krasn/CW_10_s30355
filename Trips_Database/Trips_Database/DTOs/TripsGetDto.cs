using Trips_Database.Models;

namespace Trips_Database.DTOs;

public class TripsGetDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public int MaxPeople { get; set; }
    
    public ICollection<CountryGetDto>? Countries { get; set; } = new List<CountryGetDto>();
    public ICollection<ClientGetDto>? Clients { get; set; } = new List<ClientGetDto>();
    
}

public class CountryGetDto
{
    public string Name { get; set; }
}

public class ClientGetDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}