using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GPhotoSync
{
    public interface IViewModelLocator
    {
        TViewModel Locate<TViewModel>() where TViewModel : IViewModel;
    }
}
