using System.Windows;

namespace WpfKeyListener
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainWindowViewModel VM;

        public MainWindow()
        {
            VM = new MainWindowViewModel(); 
            DataContext = VM;

            InitializeComponent();
        }
    }
}
