using System.Diagnostics;

namespace WpfKeyListener
{
    public class ObservableProcess : ObservableBase
    {
        private string _mainWindowTitle;
        public string MainWindowTitle { get => _mainWindowTitle; set => SetProperty(ref _mainWindowTitle, value); }

        private int _id;
        public int Id { get => _id; set => SetProperty(ref _id, value); }

        public Process Process { get; set; }

        public ObservableProcess(string title, int id, Process process)
        {
            MainWindowTitle = title;
            Id = id;
            Process = process;
        }

        public ObservableProcess(Process p)
            : this(p.MainWindowTitle, p.Id, p) { }
    }
}
