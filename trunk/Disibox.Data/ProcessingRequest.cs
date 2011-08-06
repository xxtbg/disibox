namespace Disibox.Data
{
    public class ProcessingRequest
    {
        public ProcessingRequest(string fileUri, string fileContentType, string processingToolName)
        {
            FileUri = fileUri;
            FileContentType = fileContentType;
            ToolName = processingToolName;
        }

        public string FileUri { get; private set; }

        public string FileContentType { get; private set; }

        public string ToolName { get; private set; }

        public static ProcessingRequest FromString(string req)
        {
            var reqParts = req.Split(new[] { ',' });

            var fileUri = reqParts[0];
            var fileContentType = reqParts[1];
            var toolName = reqParts[2];

            return new ProcessingRequest(fileUri, fileContentType, toolName);
        }

        public override string ToString()
        {
            return string.Format("{0},{1},{2}", FileUri, FileContentType, ToolName);
        }
    }
}
