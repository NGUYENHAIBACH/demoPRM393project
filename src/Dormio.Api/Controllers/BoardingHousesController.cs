using System.Security.Claims;
using Dormio.Api.Data;
using Dormio.Api.Dtos;
using Dormio.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dormio.Api.Controllers;

/// <summary>CRUD for boarding houses. Landlords only see and change their own; Admin sees all.</summary>
[ApiController]
[Route("api/boarding-houses")]
[Authorize(Roles = $"{Roles.Landlord},{Roles.Admin}")]
public class BoardingHousesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<BoardingHousesController> _logger;

    public BoardingHousesController(AppDbContext db, ILogger<BoardingHousesController> logger)
    {
        _db = db;
        _logger = logger;
    }

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private IQueryable<BoardingHouse> Scoped() =>
        User.IsInRole(Roles.Admin) ? _db.BoardingHouses : _db.BoardingHouses.Where(h => h.OwnerId == UserId);

    private static BoardingHouseDto ToDto(BoardingHouse h) => new()
    {
        Id = h.Id, OwnerId = h.OwnerId, Name = h.Name, Address = h.Address, Description = h.Description,
        IsActive = h.IsActive, RoomCount = h.Rooms.Count, CreatedAt = h.CreatedAt
    };

    [HttpGet]
    public async Task<ActionResult<List<BoardingHouseDto>>> GetAll()
    {
        _logger.LogInformation("List boarding houses by {UserId}", UserId);
        return await Scoped().OrderBy(h => h.Name)
            .Select(h => new BoardingHouseDto
            {
                Id = h.Id, OwnerId = h.OwnerId, Name = h.Name, Address = h.Address, Description = h.Description,
                IsActive = h.IsActive, RoomCount = h.Rooms.Count, CreatedAt = h.CreatedAt
            }).ToListAsync();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BoardingHouseDto>> Get(Guid id)
    {
        var h = await Scoped().Include(x => x.Rooms).FirstOrDefaultAsync(x => x.Id == id);
        return h is null ? NotFound() : ToDto(h);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Landlord)]
    public async Task<ActionResult<BoardingHouseDto>> Create(BoardingHouseSaveRequest req)
    {
        _logger.LogInformation("Create boarding house by {UserId}", UserId);
        var h = new BoardingHouse { OwnerId = UserId, Name = req.Name, Address = req.Address, Description = req.Description, IsActive = req.IsActive };
        _db.BoardingHouses.Add(h);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = h.Id }, ToDto(h));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, BoardingHouseSaveRequest req)
    {
        _logger.LogInformation("Update boarding house {Id}", id);
        var h = await Scoped().FirstOrDefaultAsync(x => x.Id == id);
        if (h is null) return NotFound();
        h.Name = req.Name; h.Address = req.Address; h.Description = req.Description; h.IsActive = req.IsActive;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Soft delete: deactivates the boarding house (rooms and contracts are kept).</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        _logger.LogInformation("Deactivate boarding house {Id}", id);
        var h = await Scoped().FirstOrDefaultAsync(x => x.Id == id);
        if (h is null) return NotFound();
        h.IsActive = false;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
