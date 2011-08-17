using System;
using Disibox.Data.Client;

namespace Disibox.WebUI
{
    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void UploadButton_Click(object sender, EventArgs e)
        {
            var ds = new ClientDataSource();

            ds.AddFile(FileUpload.FileName, FileUpload.FileContent);
        }
    }
}
