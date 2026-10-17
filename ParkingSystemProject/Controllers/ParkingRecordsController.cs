using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using ParkingSystemProject.Data;
using ParkingSystemProject.Models;

namespace ParkingSystemProject.Controllers
{
    public class ParkingRecordsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ParkingRecordsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ParkingRecords
        public async Task<IActionResult> Index()
        {
            var parkingRecords = await _context.ParkingRecords
                .Include(p => p.Vehicle)
                .Include(p => p.ParkingSpace)
                .ToListAsync();
            return View(parkingRecords);
        }

        // GET: ParkingRecords/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var parkingRecord = await _context.ParkingRecords
                .Include(p => p.Vehicle)
                .Include(p => p.ParkingSpace)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (parkingRecord == null)
            {
                return NotFound();
            }

            return View(parkingRecord);
        }

        // GET: ParkingRecords/Create
        public IActionResult Create()
        {
            ViewData["VehicleId"] = new SelectList(_context.Vehicles, "Id", "RegistrationNumber");
            ViewData["ParkingSpaceId"] = new SelectList(_context.ParkingSpaces, "Id", "SpaceNumber");
            return View();
        }

        // POST: ParkingRecords/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,VehicleId,ParkingSpaceId,EntryTime,ExitTime,DurationMinutes,Price,UsedVipPoint,IsPaid")] ParkingRecord parkingRecord)
        {
            if (ModelState.IsValid)
            {
                _context.Add(parkingRecord);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["VehicleId"] = new SelectList(_context.Vehicles, "Id", "RegistrationNumber", parkingRecord.VehicleId);
            ViewData["ParkingSpaceId"] = new SelectList(_context.ParkingSpaces, "Id", "SpaceNumber", parkingRecord.ParkingSpaceId);
            return View(parkingRecord);
        }

        // GET: ParkingRecords/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var parkingRecord = await _context.ParkingRecords.FindAsync(id);
            if (parkingRecord == null)
            {
                return NotFound();
            }
            ViewData["VehicleId"] = new SelectList(_context.Vehicles, "Id", "RegistrationNumber", parkingRecord.VehicleId);
            ViewData["ParkingSpaceId"] = new SelectList(_context.ParkingSpaces, "Id", "SpaceNumber", parkingRecord.ParkingSpaceId);
            return View(parkingRecord);
        }

        // POST: ParkingRecords/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,VehicleId,ParkingSpaceId,EntryTime,ExitTime,DurationMinutes,Price,UsedVipPoint,IsPaid")] ParkingRecord parkingRecord)
        {
            if (id != parkingRecord.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(parkingRecord);
                    await _context.SaveChangesAsync();
                }
                catch (Exception)
                {
                    if (!ParkingRecordExists(parkingRecord.Id))
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
            ViewData["VehicleId"] = new SelectList(_context.Vehicles, "Id", "RegistrationNumber", parkingRecord.VehicleId);
            ViewData["ParkingSpaceId"] = new SelectList(_context.ParkingSpaces, "Id", "SpaceNumber", parkingRecord.ParkingSpaceId);
            return View(parkingRecord);
        }

        // GET: ParkingRecords/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var parkingRecord = await _context.ParkingRecords
                .Include(p => p.Vehicle)
                .Include(p => p.ParkingSpace)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (parkingRecord == null)
            {
                return NotFound();
            }

            return View(parkingRecord);
        }

        // POST: ParkingRecords/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var parkingRecord = await _context.ParkingRecords.FindAsync(id);
            if (parkingRecord != null)
            {
                _context.ParkingRecords.Remove(parkingRecord);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ParkingRecordExists(int id)
        {
            return _context.ParkingRecords.Any(e => e.Id == id);
        }
    }
}