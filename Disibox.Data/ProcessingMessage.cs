namespace Disibox.Data
{
    public class ProcessingMessage
    {
        public ProcessingMessage(string fileUri, string fileContentType, string processingToolName)
        {
            FileUri = fileUri;
            FileContentType = fileContentType;
            ToolName = processingToolName;
        }

        public string FileUri { get; private set; }

        public string FileContentType { get; private set; }

        public string ToolName { get; private set; }

        public static ProcessingMessage FromString(string req)
        {
            var reqParts = req.Split(new[] { ',' });

            var fileUri = reqParts[0];
            var fileContentType = reqParts[1];
            var toolName = reqParts[2];

            return new ProcessingMessage(fileUri, fileContentType, toolName);
        }

        public override string ToString()
        {
            return string.Format("{0},{1},{2}", FileUri, FileContentType, ToolName);
        }
    }
}
