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
        private readonly ImageUpdater _updater;

        public ImageController(ImageUpdater updater)
        {
            _updater = updater;
        }

        [HttpPost("/api/update")]
        [Authorize(AuthenticationSchemes = "ApiKey")]
        public ActionResult<IEnumerable<ImageEvent>> UpdateImages([Required][FromBody] UpdateRequest request)
        {
            return Ok(_updater.UpdateEventHandler(request.GetFullImageUri()));
        }
    }
}