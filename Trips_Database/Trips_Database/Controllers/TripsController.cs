using Microsoft.AspNetCore.Mvc;
using Trips_Database.DTOs;
using Trips_Database.Exceptions;
using Trips_Database.Services;

namespace Trips_Database.Controllers;

[ApiController]
[Route("api/trips")]
public class TripsController(IDbService dbService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetTripsAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1 || pageSize < 1) return BadRequest("Page and pageSize must be greater than 0");

        var trips = await dbService.GetTripsAsync();
        
        var tripsPage = trips.Skip((page - 1) * pageSize).Take(pageSize);
        
        return Ok(tripsPage);
    }

    [HttpPost("{idTrip}/clients")]
    public async Task<IActionResult> AddClientToTripAsync([FromRoute] int idTrip, [FromBody] ClientTripGetDto clientTrip)
    {
        try
        {
            await dbService.AddClientToTripAsync(idTrip, clientTrip);
            return Created();
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (TripIsOnGoingException e)
        {
            return BadRequest(e.Message);
        }
        catch (ClientExists e)
        {
            return BadRequest(e.Message);
        }
    }
}