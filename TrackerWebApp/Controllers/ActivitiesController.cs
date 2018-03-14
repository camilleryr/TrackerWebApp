using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrackerWebApp.Data;
using TrackerWebApp.Models;

namespace TrackerWebApp.Controllers
{
    public class ActivitiesController : Controller
    {
        private readonly ApplicationDbContext _context;
        static HttpClient client = new HttpClient();

        public ActivitiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Activities
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Activity.Include(a => a.ActivityType);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Activities/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var activities = _context.Activity;
            Activity activity;

            if (id == null)
            {
                activity = await activities
                    .Include(a => a.ActivityType)
                    .Include(a => a.Notes)
                    .OrderByDescending(m => m.Id)
                    .FirstAsync();
            }

            else
            {
                activity = await activities
                    .Include(a => a.ActivityType)
                    .Include(a => a.Notes)
                    .SingleOrDefaultAsync(m => m.Id == id);

                if (activity == null)
                {
                    return NotFound();
                }
            }

            var PreviousActivity = activities.ToList().Select(x => x.Id).TakeWhile(x => x != activity.Id).LastOrDefault();
            var NextActivity = activities.ToList().Select(x => x.Id).SkipWhile(x => x != activity.Id).Skip(1).FirstOrDefault();


            HttpResponseMessage response = await client.GetAsync($"https://trackmyrun-41804.firebaseio.com/{activity.FirebaseLocation}.json");

            GeoLocation[] geoLocationArray;

                byte[] responseByteArray = await response.Content.ReadAsByteArrayAsync();
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(GeoLocation[]));
                MemoryStream stream = new MemoryStream(responseByteArray);
                stream.Position = 0;
                geoLocationArray = (GeoLocation[])ser.ReadObject(stream);


            StringBuilder polyLine = new StringBuilder();

            polyLine.Append("http://maps.googleapis.com/maps/api/staticmap?size=800x400&path=");

            foreach (GeoLocation g in geoLocationArray)
            {
                int arrayIndex = geoLocationArray.ToList().IndexOf(g);

                if (arrayIndex % 5 == 0)
                {
                    polyLine.Append($"{g.coords.latitude.ToString()},{g.coords.longitude.ToString()}");
                    if(arrayIndex < geoLocationArray.Length - 5)
                    {
                        polyLine.Append("|");
                    }
                }
            }

            ViewData["Map"] = polyLine.ToString();
            ViewData["PreviousActivityId"] = (int?) PreviousActivity;
            ViewData["NextActivityId"] = (int?)NextActivity;
            ViewData["ActivityTypeId"] = new SelectList(_context.ActivityType, "ActivityTypeId", "Description", activity.ActivityTypeId);

            return View(activity);
        }

        // GET: Activities/Create
        public IActionResult Create()
        {
            ViewData["ActivityTypeId"] = new SelectList(_context.ActivityType, "ActivityTypeId", "ActivityTypeId");
            return View();
        }

        // POST: Activities/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Distance,Pace,StartTime,FirebaseLocation,ActivityTypeId")] Activity activity)
        {
            if (ModelState.IsValid)
            {
                _context.Add(activity);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ActivityTypeId"] = new SelectList(_context.ActivityType, "ActivityTypeId", "ActivityTypeId", activity.ActivityTypeId);
            return View(activity);
        }

        // GET: Activities/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var activity = await _context.Activity.SingleOrDefaultAsync(m => m.Id == id);
            if (activity == null)
            {
                return NotFound();
            }
            ViewData["ActivityTypeId"] = new SelectList(_context.ActivityType, "ActivityTypeId", "Description", activity.ActivityTypeId);

            return View(activity);
        }

        // POST: Activities/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Distance,Pace,StartTime,FirebaseLocation,ActivityTypeId")] Activity activity)
        {
            if (id != activity.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(activity);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ActivityExists(activity.Id))
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
            return RedirectToAction("Details", new { Id = id });
        }

        // GET: Activities/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var activity = await _context.Activity
                .Include(a => a.ActivityType)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (activity == null)
            {
                return NotFound();
            }

            return View(activity);
        }

        // POST: Activities/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var activity = await _context.Activity.SingleOrDefaultAsync(m => m.Id == id);
            _context.Activity.Remove(activity);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ActivityExists(int id)
        {
            return _context.Activity.Any(e => e.Id == id);
        }
    }
}
