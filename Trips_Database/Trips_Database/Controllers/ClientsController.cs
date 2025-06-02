using Microsoft.AspNetCore.Mvc;
using Trips_Database.Exceptions;
using Trips_Database.Services;

namespace Trips_Database.Controllers;

[ApiController]
[Route("api/clients")]
public class ClientsController(IDbService dbService) : ControllerBase
{
    [HttpDelete("{idClient}")]
    public async Task<IActionResult> RemoveClientById([FromRoute] int idClient)
    {
        try
        {
            await dbService.RemoveClientByIdAsync(idClient);
            return NoContent();
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (TripsExistException e)
        {
            return BadRequest(e.Message);
        }
    }
}