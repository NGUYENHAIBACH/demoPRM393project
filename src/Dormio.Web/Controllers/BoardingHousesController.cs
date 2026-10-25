using Dormio.Web.Models;
using Dormio.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dormio.Web.Controllers;

[Authorize(Roles = "Landlord,Admin")]
public class BoardingHousesController : Controller
{
    private readonly ApiClient _api;
    public BoardingHousesController(ApiClient api) => _api = api;

    public async Task<IActionResult> Index() =>
        View(await _api.GetAsync<List<BoardingHouseVm>>("api/boarding-houses") ?? new List<BoardingHouseVm>());

    [Authorize(Roles = "Landlord")]
    public IActionResult Create() => View(new BoardingHouseVm());

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Landlord")]
    public async Task<IActionResult> Create(BoardingHouseVm vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var res = await _api.PostAsync("api/boarding-houses", vm);
        if (res.IsSuccessStatusCode) { TempData["Message"] = "Boarding house created."; return RedirectToAction(nameof(Index)); }
        ModelState.AddModelError(string.Empty, "Could not create the boarding house.");
        return View(vm);
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var vm = await _api.GetAsync<BoardingHouseVm>($"api/boarding-houses/{id}");
        return vm is null ? NotFound() : View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, BoardingHouseVm vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var res = await _api.PutAsync($"api/boarding-houses/{id}", vm);
        if (res.IsSuccessStatusCode) { TempData["Message"] = "Boarding house updated."; return RedirectToAction(nameof(Index)); }
        ModelState.AddModelError(string.Empty, "Could not update the boarding house.");
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        await _api.DeleteAsync($"api/boarding-houses/{id}");
        TempData["Message"] = "Boarding house deactivated.";
        return RedirectToAction(nameof(Index));
    }
}
