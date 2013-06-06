using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPhotoSync
{
    public class CloseDialogMessage
    {
        public object Content { get; private set; }

        public CloseDialogMessage(object content)
        {
            Content = content;
        }
    }
}
