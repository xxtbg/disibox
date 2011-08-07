namespace Disibox.Data 
{
    public class FileAndMime 
    {
        public FileAndMime(string filename, string mime, string uri) 
        {
            Filename = filename;
            Mime = mime;
            Uri = uri;
        }

        public string Filename { get; private set; }

        public string Mime { get; private set; }

        public string Uri{ get; private set; }
    }
}
