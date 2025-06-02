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
    public async Task<IActionResult> GetTripsAsync()
    {
        return Ok(await dbService.GetTripsAsync());
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