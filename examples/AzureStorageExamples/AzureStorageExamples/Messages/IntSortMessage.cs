using System.Collections.Generic;
using System.Linq;
using System.Text;
using AzureStorageExamples.Data;

namespace AzureStorageExamples.Messages
{
    public class IntSortMessage : IMessage
    {
        public IList<int> Integers { get; private set; }

        // Required by the queue.
        public IntSortMessage()
        {
            // Empty
        }

        private IntSortMessage(IList<int> integers)
        {
            Integers = integers;
        }

        public static IntSortMessage FromList(IList<int> integers)
        {
            return new IntSortMessage(integers);
        }

        public void FromString(string msg)
        {
            Integers = msg.Split(',').Select(s => int.Parse(s)).ToList();
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(Integers[0]);
            for (var i = 1; i < Integers.Count; ++i)
                builder.Append("," + Integers[i]);
            return builder.ToString();
        }
    }
}
