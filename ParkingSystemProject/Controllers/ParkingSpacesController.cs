using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ParkingSystemProject.Data;
using ParkingSystemProject.Models;

namespace ParkingSystemProject.Controllers
{
    public class ParkingSpacesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ParkingSpacesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ParkingSpaces
        public async Task<IActionResult> Index()
        {
            var parkingSpaces = await _context.ParkingSpaces.ToListAsync();
            return View(parkingSpaces);
        }

        // GET: ParkingSpaces/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var parkingSpace = await _context.ParkingSpaces
                .FirstOrDefaultAsync(m => m.Id == id);
            if (parkingSpace == null)
            {
                return NotFound();
            }

            return View(parkingSpace);
        }

        // GET: ParkingSpaces/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ParkingSpaces/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,SpaceNumber,IsOccupied")] ParkingSpace parkingSpace)
        {
            if (ModelState.IsValid)
            {
                _context.Add(parkingSpace);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(parkingSpace);
        }

        // GET: ParkingSpaces/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var parkingSpace = await _context.ParkingSpaces.FindAsync(id);
            if (parkingSpace == null)
            {
                return NotFound();
            }
            return View(parkingSpace);
        }

        // POST: ParkingSpaces/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,SpaceNumber,IsOccupied")] ParkingSpace parkingSpace)
        {
            if (id != parkingSpace.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(parkingSpace);
                    await _context.SaveChangesAsync();
                }
                catch (Exception)
                {
                    if (!ParkingSpaceExists(parkingSpace.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(parkingSpace);
        }

        // GET: ParkingSpaces/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var parkingSpace = await _context.ParkingSpaces
                .FirstOrDefaultAsync(m => m.Id == id);
            if (parkingSpace == null)
            {
                return NotFound();
            }

            return View(parkingSpace);
        }

        // POST: ParkingSpaces/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var parkingSpace = await _context.ParkingSpaces.FindAsync(id);
            if (parkingSpace != null)
            {
                _context.ParkingSpaces.Remove(parkingSpace);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ParkingSpaceExists(int id)
        {
            return _context.ParkingSpaces.Any(e => e.Id == id);
        }
    }
}