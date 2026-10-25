using System.Security.Claims;
using Dormio.Api.Data;
using Dormio.Api.Dtos;
using Dormio.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Dormio.Api.Controllers;

/// <summary>
/// OData read endpoint: GET /odata/Rooms?$filter=Status eq 'Available' and BasePrice le 3000000&amp;$orderby=BasePrice&amp;$top=10
/// </summary>
[Authorize(Roles = $"{Roles.Landlord},{Roles.Admin}")]
public class RoomsController : ODataController
{
    private readonly AppDbContext _db;
    private readonly ILogger<RoomsController> _logger;

    public RoomsController(AppDbContext db, ILogger<RoomsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [EnableQuery(PageSize = 100, MaxTop = 100)]
    public IQueryable<RoomDto> Get()
    {
        _logger.LogInformation("OData Rooms query");
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var rooms = User.IsInRole(Roles.Admin)
            ? _db.Rooms
            : _db.Rooms.Where(r => r.BoardingHouse.OwnerId == userId);
        return rooms.Select(r => new RoomDto
        {
            Id = r.Id, BoardingHouseId = r.BoardingHouseId, RoomNumber = r.RoomNumber, Floor = r.Floor,
            Area = r.Area, MaxOccupants = r.MaxOccupants, BasePrice = r.BasePrice, Status = r.Status
        });
    }
}
