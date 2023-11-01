using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Simbir.GO.BLL;
using Simbir.GO.BLL.Models;
using Simbir.GO.WebApi.Models;

namespace Simbir.GO.WebApi.Controllers;

[Route("api/Transport")]
[ApiController]
public class TransportController : ControllerBase
{
    private readonly SimbirGoDbContext _dbContext;

    public TransportController(SimbirGoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTransportById(int id)
    {
        var transport = await _dbContext.Transports.FindAsync(id);

        if (transport == null)
        {
            return NotFound();
        }

        return Ok(transport);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateTransport([FromBody] TransportRequest model)
    {
        var transport = new Transport
        {
            CanBeRented = model.CanBeRented,
            TransportType = model.TransportType,
            Model = model.Model,
            Color = model.Color,
            Identifier = model.Identifier,
            Description = model.Description,
            Latitude = model.Latitude,
            Longitude = model.Longitude,
            MinutePrice = model.MinutePrice,
            DayPrice = model.DayPrice
        };

        _dbContext.Transports.Add(transport);
        await _dbContext.SaveChangesAsync();

        return Created("", transport);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateTransport(int id, [FromBody] TransportRequest model)
    {
        var transport = await _dbContext.Transports.FindAsync(id);

        if (transport == null)
        {
            return NotFound();
        }

        transport.CanBeRented = model.CanBeRented;

        transport.Model = model.Model?? transport.Model;
        transport.Color = model.Color?? transport.Color;
        transport.Identifier = model.Identifier?? transport.Identifier;
        transport.Description = model.Description?? transport.Description;
        transport.Latitude = model.Latitude;
        transport.Longitude = model.Longitude;
        transport.MinutePrice = model.MinutePrice;
        transport.DayPrice = model.DayPrice;

        _dbContext.Transports.Update(transport);
        await _dbContext.SaveChangesAsync();

        return Ok(transport);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteTransport(int id)
    {
        var transport = await _dbContext.Transports.FindAsync(id);

        if (transport == null)
        {
            return NotFound();
        }

        _dbContext.Transports.Remove(transport);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }
}
