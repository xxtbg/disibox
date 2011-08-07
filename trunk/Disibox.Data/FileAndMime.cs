namespace Disibox.Data 
{
    public class FileAndMime 
    {
        public FileAndMime(string filename, string mime) 
        {
            Filename = filename;
            Mime = mime;
        }

        public string Filename { get; private set; }

        public string Mime { get; private set; }
    }
}
