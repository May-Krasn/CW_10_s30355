using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Trips_Database.DTOs;
using Trips_Database.Services;

namespace Trips_Database.Controllers;

[ApiController]
[Route("trips")]
public class TripsController(IDbService dbService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetTripsAsync()
    {
        return Ok(await dbService.GetTripsAsync());
    }
}