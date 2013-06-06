using GalaSoft.MvvmLight;
using System;

namespace GPhotoSync
{
    public class AlbumMapping : ObservableObject
    {
        #region Properties
        public Guid Id { get; set; }

        public string AlbumId { get; set; }

        private string _localPath;
        public string LocalPath
        {
            get { return _localPath; }
            set
            {
                _localPath = value;
                RaisePropertyChanged(() => IsMapped);
            }
        }


        public bool IsMapped { get { return !string.IsNullOrEmpty(LocalPath); } }
        #endregion Properties
    }
}
