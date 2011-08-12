namespace Disibox.Data 
{
    public class FileMetadata 
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="mime"></param>
        /// <param name="uri"></param>
        /// <param name="size">the size of the <paramref name="filename"/> in kilobytes</param>
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

        /// <summary>
        /// returns the size of the filename in kilobytes
        /// </summary>
        public double Size { get; private set; }
    }
}
