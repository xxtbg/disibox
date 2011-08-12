namespace Disibox.Data 
{
    public class FileMetadata 
    {
        public FileMetadata(string filename, string mime, string uri, double size) 
        {
            Filename = filename;
            Mime = mime;
            Uri = uri;
            Size = size;
        }

        public string Filename { get; private set; }

        public string Mime { get; private set; }

        public string Uri{ get; private set; }

        public double Size { get; private set; }
    }
}
