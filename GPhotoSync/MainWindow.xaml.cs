using System.Windows;
using System.Windows.Controls.Ribbon;

namespace GPhotoSync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ((MainViewModel)DataContext).TryLoadAlbums();
        }
    }
}
