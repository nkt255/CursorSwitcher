using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;

namespace CursorSwitcher
{
    public partial class MainWindow : Window
    {
        private readonly CursorManager _cursorManager;
        private readonly AppSettings _settings;
        private readonly NotifyIcon _notifyIcon;
        private readonly GlobalInputMonitor _globalInputMonitor;
        private bool _isBindingKey;

        public MainWindow()
        {
            InitializeComponent();

            var appDir = AppContext.BaseDirectory;
            var settingsPath = Path.Combine(appDir, "settings.json");

            _settings = AppSettings.Load(settingsPath);
            _cursorManager = new CursorManager();
            _globalInputMonitor = new GlobalInputMonitor(this);

            LoadUiFromSettings();

            _notifyIcon = new NotifyIcon
            {
                Icon = SystemIcons.Information,
                Visible = true,
                Text = "CursorSwitcher"
            };

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Show", null, (_, _) => ShowWindow());
            contextMenu.Items.Add("Apply Style 1", null, (_, _) => ApplyStyle1());
            contextMenu.Items.Add("Apply Windows Default", null, (_, _) => ApplyDefaultStyle());
            contextMenu.Items.Add("Exit", null, (_, _) => ExitApplication());
            _notifyIcon.ContextMenuStrip = contextMenu;

            _notifyIcon.DoubleClick += (_, _) => ShowWindow();

            Closed += (_, _) =>
            {
                _notifyIcon.Dispose();
                _globalInputMonitor.Dispose();
            };

            StateChanged += (_, _) =>
            {
                if (WindowState == WindowState.Minimized)
                {
                    Hide();
                }
            };
        }

        private void LoadUiFromSettings()
        {
            Style1PathBox.Text = _settings.Style1Path ?? string.Empty;
            Style2PathBox.Text = _settings.Style2Path ?? "Windows 11 default";
            HotkeyBox.Text = string.IsNullOrWhiteSpace(_settings.Hotkey) ? "F9" : _settings.Hotkey;

            if (!string.IsNullOrWhiteSpace(_settings.Style1Path))
            {
                ApplyStyle1();
            }
            else
            {
                ApplyDefaultStyle();
            }
        }

        public bool IsBindingKey => _isBindingKey;

        public void StartBindingMode()
        {
            _isBindingKey = true;
            HotkeyBox.Text = "Press any key...";
        }

        public void CaptureKey(string keyName)
        {
            if (!_isBindingKey)
            {
                return;
            }

            _isBindingKey = false;
            _settings.Hotkey = keyName;
            HotkeyBox.Text = keyName;
            SaveSettings();
        }

        private void BrowseStyle1_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Animated cursor files (*.ani)|*.ani",
                Title = "Select cursor file",
                CheckFileExists = true,
                Multiselect = false
            };

            if (dialog.ShowDialog() == true)
            {
                var selectedFile = dialog.FileName;
                if (!File.Exists(selectedFile) || !selectedFile.EndsWith(".ani", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Only .ani files are supported.", "Invalid file", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _settings.Style1Path = selectedFile;
                Style1PathBox.Text = selectedFile;
                SaveSettings();
                ApplyStyle1();
            }
        }

        private void UseDefaultCursor_Click(object sender, RoutedEventArgs e)
        {
            _settings.Style1Path = null;
            Style1PathBox.Text = string.Empty;
            Style2PathBox.Text = "Windows 11 default";
            SaveSettings();
            ApplyDefaultStyle();
        }

        private void BindHotkey_Click(object sender, RoutedEventArgs e)
        {
            StartBindingMode();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
            MessageBox.Show("Settings saved.", "CursorSwitcher", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ApplyStyle1_Click(object sender, RoutedEventArgs e)
        {
            ApplyStyle1();
        }

        private void ApplyStyle1()
        {
            if (string.IsNullOrWhiteSpace(_settings.Style1Path) || !File.Exists(_settings.Style1Path))
            {
                MessageBox.Show("Style 1 is not configured. Please select a valid .ani file.", "Invalid cursor", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = _cursorManager.ApplyCustomCursor(_settings.Style1Path);
            if (!result)
            {
                MessageBox.Show("The selected file is not a valid .ani cursor. Please use a valid animated cursor file.", "Invalid .ani", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _settings.CurrentStyle = "Style1";
            Style2PathBox.Text = "Windows 11 default";
            SaveSettings();
        }

        private void ApplyDefaultStyle()
        {
            _cursorManager.ApplyWindowsDefault();
            _settings.CurrentStyle = "Default";
            SaveSettings();
        }

        private void SaveSettings()
        {
            _settings.Save();
        }

        private void ShowWindow()
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
        }

        private void ExitApplication()
        {
            _notifyIcon.Visible = false;
            Close();
            Environment.Exit(0);
        }
    }
}
