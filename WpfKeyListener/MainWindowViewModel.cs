using Gma.System.MouseKeyHook;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace WpfKeyListener
{
    public class MainWindowViewModel : ObservableBase
    {
        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        private IKeyboardMouseEvents m_GlobalHook;

        private ObservableCollection<ObservableProcess> _processes;
        private ObservableCollection<ObservableProcess> _processesSwitchList;
        private ObservableProcess _selectedProcessMainList;
        private ObservableProcess _selectedProcessSwitchList;
        //private ObservableCollection<string> _keyList;

        private int _processIndex = 0;
        private ObservableProcess _currentSwitchingProcess = null;
        private bool _processSwitchingIsEnabled = false;
        private bool _isBindingKey = false;
        private string _keyBindingsString = "";

        public ICommand CmdGetProcesses { get; set; }
        public ICommand CmdBringToFrontFromButton { get; set; }
        public ICommand CmdMoveToSwitchList { get; set; }
        public ICommand CmdRemoveFromSwitchList { get; set; }
        public ICommand CmdStartKeyBinding { get; set; }
        public ICommand CmdCancelKeyBinding { get; set; }
        public ICommand CmdResetKeyBindings { get; set; }

        public ObservableCollection<ObservableProcess> Processes { get => _processes; set => SetProperty(ref _processes, value); }
        public ObservableCollection<ObservableProcess> ProcessesSwitchList { get => _processesSwitchList; set => SetProperty(ref _processesSwitchList, value); }
        //public ObservableCollection<string> KeyList { get => _keyList; set => SetProperty(ref _keyList, value); }

        public ObservableProcess SelectedProcessMainList { get => _selectedProcessMainList; set => SetProperty(ref _selectedProcessMainList, value); }
        public ObservableProcess SelectedProcessSwitchList { get => _selectedProcessSwitchList; set => SetProperty(ref _selectedProcessSwitchList, value); }

        public ObservableProcess CurrentSwitchingProcess { get => _currentSwitchingProcess; set => SetProperty(ref _currentSwitchingProcess, value); }
        
        public bool ProcessSwitchingIsEnabled { get => _processSwitchingIsEnabled; set => SetProperty(ref _processSwitchingIsEnabled, value); }

        public bool IsBindingKey { get => _isBindingKey; set => SetProperty(ref _isBindingKey, value); }

        public string KeyBindingsString { get => _keyBindingsString; set => SetProperty(ref _keyBindingsString, value); }

        public List<char> KeyList { get; set; } = new List<char>();

        public MainWindowViewModel()
        {
            CmdGetProcesses = new RelayCommand(GetProcesses);
            CmdBringToFrontFromButton = new RelayCommand(BringToFrontFromButton);
            CmdMoveToSwitchList = new RelayCommand(MoveToSwitchList);
            CmdRemoveFromSwitchList = new RelayCommand(RemoveFromSwitchList);
            CmdStartKeyBinding = new RelayCommand(StartKeyBinding);
            CmdCancelKeyBinding = new RelayCommand(CancelKeyBinding);
            CmdResetKeyBindings = new RelayCommand(ResetKeyBindings);

            Processes = new ObservableCollection<ObservableProcess>();
            ProcessesSwitchList = new ObservableCollection<ObservableProcess>();

            // Subscribe to input events
            Subscribe();

            // Get all processes with a window
            GetProcesses();
        }

        public void CycleProcesses()
        {
            // If list is empty, bail
            if (ProcessesSwitchList.Count == 0)
            {
                return;
            }

            // Increment index, wrapping back to 0 on overflow
            _processIndex = (_processIndex >= ProcessesSwitchList.Count - 1) ? 0 : _processIndex + 1;

            // Focus the next window
            CurrentSwitchingProcess = ProcessesSwitchList.ElementAt(_processIndex);
            SetForegroundWindow(CurrentSwitchingProcess.Process.MainWindowHandle);
        }

        #region Command handlers

        public void MoveToSwitchList(object args)
        {
            if (SelectedProcessMainList != null)
            {
                // Since this will update the combobox thru databinding, SelectedProcessMainList will become 
                // null shortly after the "Remove" call below. Save the selected process before that.
                ObservableProcess proc = SelectedProcessMainList;

                Processes.Remove(proc); 
                ProcessesSwitchList.Add(proc);
            }
        }

        public void RemoveFromSwitchList(object args)
        {
            if (SelectedProcessSwitchList != null)
            {
                // Since this will update the combobox thru databinding, SelectedProcessSwitchList will become 
                // null shortly after the "Remove" call below. Save the selected process before that.
                ObservableProcess proc = SelectedProcessSwitchList;

                Processes.Add(proc);
                ProcessesSwitchList.Remove(proc);
            }
        }

        public void StartKeyBinding(object args)
        {
            IsBindingKey = true;
        }

        public void CancelKeyBinding(object args)
        {
            IsBindingKey = false;
        }

        public void ResetKeyBindings(object args)
        {
            KeyList.Clear();
            KeyBindingsString = String.Empty;
        }

        public void BringToFrontFromButton(object args)
        {
            SetForegroundWindow(SelectedProcessMainList.Process.MainWindowHandle);
        }

        /// <summary>
        /// Gets a list of all processes that have a MainWindowTitle (i.e. have a window).
        /// </summary>
        public void GetProcesses(object args = null)
        {
            ProcessSwitchingIsEnabled = false;
            ProcessesSwitchList.Clear();
            Processes.Clear();
            var procs = Process.GetProcesses();

            foreach (Process p in procs)
            {
                if (!string.IsNullOrEmpty(p.MainWindowTitle))
                {
                    Processes.Add(new ObservableProcess(p));
                }
            }
        }

        #endregion

        #region Mouse/Keyboard hooking

        public void Subscribe()
        {
            // Note: for the application hook, use the Hook.AppEvents() instead
            m_GlobalHook = Hook.GlobalEvents();

            m_GlobalHook.MouseDownExt += GlobalHookMouseDownExt;
            m_GlobalHook.KeyPress += GlobalHookKeyPress;
        }
        public void Unsubscribe()
        {
            m_GlobalHook.MouseDownExt -= GlobalHookMouseDownExt;
            m_GlobalHook.KeyPress -= GlobalHookKeyPress;

            //It is recommened to dispose it
            m_GlobalHook.Dispose();
        }

        private void GlobalHookKeyPress(object sender, KeyPressEventArgs e)
        {
            Console.WriteLine("KeyPress: \t{0}", e.KeyChar);

            if (IsBindingKey)
            {
                // Add key to list of bindings when in keybinding mode
                if (!KeyList.Contains(e.KeyChar))
                {
                    KeyList.Add(e.KeyChar);
                    KeyBindingsString += $" {e.KeyChar}";
                }

                // Exit keybinding mode
                IsBindingKey = false;
            }
            else if (ProcessSwitchingIsEnabled)
            {
                // Check if pressed key is in list, if so then cycle processes
                if (KeyList.Contains(e.KeyChar))
                {
                    CycleProcesses();
                }
            }
        }

        private void GlobalHookMouseDownExt(object sender, MouseEventExtArgs e)
        {
            Console.WriteLine("MouseDown: \t{0}; \t System Timestamp: \t{1}", e.Button, e.Timestamp);

            // uncommenting the following line will suppress the middle mouse button click
            // if (e.Buttons == MouseButtons.Middle) { e.Handled = true; }
        }

        #endregion
    }
}
