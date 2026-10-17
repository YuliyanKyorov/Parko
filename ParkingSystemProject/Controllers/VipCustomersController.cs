using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ParkingSystemProject.Data;
using ParkingSystemProject.Models;

namespace ParkingSystemProject.Controllers
{
    public class VipCustomersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VipCustomersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: VipCustomers
        public async Task<IActionResult> Index()
        {
            var vipCustomers = await _context.VipCustomers.ToListAsync();
            return View(vipCustomers);
        }

        // GET: VipCustomers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vipCustomer = await _context.VipCustomers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (vipCustomer == null)
            {
                return NotFound();
            }

            return View(vipCustomer);
        }

        // GET: VipCustomers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: VipCustomers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,Address,City,Country,Phone,Email,Points,PointsResetDate")] VipCustomer vipCustomer)
        {
            if (ModelState.IsValid)
            {
                _context.Add(vipCustomer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(vipCustomer);
        }

        // GET: VipCustomers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vipCustomer = await _context.VipCustomers.FindAsync(id);
            if (vipCustomer == null)
            {
                return NotFound();
            }
            return View(vipCustomer);
        }

        // POST: VipCustomers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,Address,City,Country,Phone,Email,Points,PointsResetDate")] VipCustomer vipCustomer)
        {
            if (id != vipCustomer.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vipCustomer);
                    await _context.SaveChangesAsync();
                }
                catch (Exception)
                {
                    if (!VipCustomerExists(vipCustomer.Id))
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
            return View(vipCustomer);
        }

        // GET: VipCustomers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vipCustomer = await _context.VipCustomers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (vipCustomer == null)
            {
                return NotFound();
            }

            return View(vipCustomer);
        }

        // POST: VipCustomers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vipCustomer = await _context.VipCustomers.FindAsync(id);
            if (vipCustomer != null)
            {
                _context.VipCustomers.Remove(vipCustomer);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VipCustomerExists(int id)
        {
            return _context.VipCustomers.Any(e => e.Id == id);
        }
    }
}