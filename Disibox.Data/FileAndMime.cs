using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Disibox.Data {
    public class FileAndMime {
        private string _filename;
        private string _mime;

        public FileAndMime(string filename, string mime) {
            _filename = filename;
            _mime = mime;
        }

        public string Filename {
            get { return _filename; }
        }

        public string Mime {
            get { return _mime; }
        }
    }
}
