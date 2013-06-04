using System.Windows.Controls;

namespace GPhotoSync
{
    /// <summary>
    /// Interaction logic for GPhotoView.xaml
    /// </summary>
    public partial class GPhotoView : UserControl
    {
        public GPhotoView()
        {
            InitializeComponent();
        }

        private void OnMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var list = sender as ListBox;
            if (list == null) return;

            ((GPhotoViewModel)DataContext).SelectedAlbum = list.SelectedItem as AlbumViewModel;
        }
    }
}
