//
// Copyright (c) 2011, University of Genoa
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the University of Genoa nor the
//       names of its contributors may be used to endorse or promote products
//       derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL UNIVERSITY OF GENOA BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//

using System.Collections.Generic;
using System.Web.UI.WebControls;
using Disibox.Data.Client;

namespace Disibox.WebUI.Controls
{
    public sealed class FilesTable : Table
    {
        private readonly FilesTableHeader _header = new FilesTableHeader();

        private IList<FileMetadata> _currentMetadata;

        public FilesTable()
        {
            Rows.Add(_header);
        }

        public IEnumerable<FileMetadata> GetSelectedFiles()
        {
            var selectedFiles = new List<FileMetadata>();

            // We start from 1 to skip the header.
            for (var i = 1; i < Rows.Count; ++i)
            {
                var checkCell = (CheckCell) Rows[i].Cells[0];
                if (!checkCell.Checked) continue;
                selectedFiles.Add(_currentMetadata[i-1]);
            }

            return selectedFiles;
        }

        public void Refresh(IList<FileMetadata> fileMetadata)
        {
            Rows.Clear();
            Rows.Add(_header);
            foreach (var metadata in fileMetadata)
                Rows.Add(new FilesTableRow(metadata));
            _currentMetadata = fileMetadata;
        }

        private sealed class FilesTableHeader : TableRow
        {
            private readonly string[] _columnHeaders = { "", "Name", "ContentType", "Owner", "Size" };

            public FilesTableHeader()
            {
                foreach (var columnHeader in _columnHeaders)
                    Cells.Add(new LabelCell(columnHeader));
            }
        }

        private sealed class FilesTableRow : TableRow
        {
            public FilesTableRow(FileMetadata metadata)
            {
                var cells = new TableCell[]
                            {
                                new CheckCell(),
                                new LabelCell(metadata.Name),
                                new LabelCell(metadata.ContentType),
                                new LabelCell(metadata.Owner),
                                new LabelCell(metadata.Size.ToString())
                            };
                Cells.AddRange(cells);
            }
        }
    }
}