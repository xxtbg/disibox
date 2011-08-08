namespace Disibox.Processing
{
    public class ProcessingOutput
    {
        public ProcessingOutput(object output, string outputContentType)
        {
            Output = output;
            OutputContentType = outputContentType;
        }

        public object Output { get; private set; }

        public string OutputContentType { get; private set; }
    }
}
