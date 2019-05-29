using System;

namespace Updater.Domain
{
    public class ImageEvent
    {
        public string Image { get; set; }
        public string Tag { get; set; }
        public string NameSpace { get; internal set; }
        public string Deployment { get; internal set; }
        public long TimeStamp { get; set; }
        public string Message { get; set; }
    }
}