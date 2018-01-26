namespace Updater.Controllers
{
    public class UpdateRequest
    {
        public string ImageName { get; set; }
        public string Tag { get; set; }

        public string GetFullImageUri() => $"{ImageName}{(Tag == null ? "" : ":" + Tag)}";
    }
}