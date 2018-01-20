using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Updater.Domain;

namespace Updater.Controllers
{
    public class ImageController: Controller
    {
        private readonly UpdaterDbContext _context;
        private readonly ImageUpdater _updater;

        protected ImageController(UpdaterDbContext context, ImageUpdater updater)
        {
            _context = context;
            _updater = updater;
        }

        [HttpPost("/api/update")]
        public IActionResult UpdateImages([FromBody] UpdateRequest request)
        {
            _updater.UpdateEventHandler(request.GetFullImageUri());
            return Ok();
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
        public IActionResult UpdateImages(string imageName)
        {
            return Ok(_context.EventHistory
                .Where(x => x.Image == imageName)
                .Take(1)
                .OrderByDescending(x => x.Stamp)
                .ToList());
        }
    }
}