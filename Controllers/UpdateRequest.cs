using System.ComponentModel.DataAnnotations;

namespace Updater.Controllers
{
    public class UpdateRequest
    {
        [Required]
        public string ImageName { get; set; }

        [Required]
        public string Tag { get; set; }

        public string GetFullImageUri() => $"{ImageName}{(Tag == null ? "" : ":" + Tag)}";
    }
}