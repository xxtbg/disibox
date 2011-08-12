using System;
using System.Collections.Generic;
using System.IO;

namespace Disibox.Processing
{
    public abstract class BaseTool : IEquatable<BaseTool>
    {
        protected BaseTool(string name, string briefDescription, string longDescription)
        {
            Name = name;
            BriefDescription = briefDescription;
            LongDescription = longDescription;
            ProcessableTypes = new List<string>();
        }

        public string Name { get; private set; }

        public string BriefDescription { get; private set; }

        public string LongDescription { get; private set; }

        public IList<string> ProcessableTypes { get; private set; }

        public abstract ProcessingOutput ProcessFile(Stream file, string fileContentType);

        public bool Equals(BaseTool other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Name, Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (BaseTool)) return false;
            return Equals((BaseTool) obj);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public static bool operator ==(BaseTool left, BaseTool right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(BaseTool left, BaseTool right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
//            const string format = "\"{0}\",\"{1}\",\"{2}\""; bro... if it is not good for you tell me!
            const string format = "{0}, {1}, {2}";
            return string.Format(format, Name, BriefDescription, LongDescription);
        }
    }
}
