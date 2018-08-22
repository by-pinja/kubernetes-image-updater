using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Updater.Domain;

namespace Updater.Controllers
{
    public class ImageController: Controller
    {
        private readonly UpdaterDbContext _context;
        private readonly ImageUpdater _updater;

        public ImageController(UpdaterDbContext context, ImageUpdater updater)
        {
            _context = context;
            _updater = updater;
        }

        [HttpPost("/api/update")]
        [Authorize(AuthenticationSchemes = "ApiKey")]
        public ActionResult<IEnumerable<ImageEvent>> UpdateImages([Required][FromBody] UpdateRequest request)
        {
            return Ok(_updater.UpdateEventHandler(request.GetFullImageUri()));
        }

        [HttpGet("/api/history/{imageName}")]
        public IActionResult GetUpdatesForImage(string imageName)
        {
            return Ok(_context.EventHistory
                .Where(x => x.Image == imageName)
                .OrderByDescending(x => x.Stamp)
                .ToList());
        }

        [HttpGet("/api/history/{imageName}/current")]
        public IActionResult GetLatestUpdate(string imageName)
        {
            return Ok(_context.EventHistory
                .Where(x => x.Image == imageName)
                .OrderByDescending(x => x.Stamp)
                .Take(1)
                .ToList());
        }
    }
}