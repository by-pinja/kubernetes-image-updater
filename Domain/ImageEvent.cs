using System;

namespace Updater.Domain
{
    public class ImageEvent
    {
        public Guid Id { get; set; }
        public DateTime Stamp { get; set; }
        public string Image { get; set;}
        public string Tag { get; set;}
        public string Deployment { get; internal set; }
        public string NameSpace { get; internal set; }
    }
}