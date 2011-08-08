using System.IO;

namespace Disibox.Processing
{
    public class ProcessingOutput
    {
        public ProcessingOutput(Stream outputContent, string outputContentType)
        {
            Content = outputContent;
            ContentType = outputContentType;
        }

        public Stream Content { get; private set; }

        public string ContentType { get; private set; }
    }
}
