using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static string CacheFolderPath => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\HomeAssistant";

        private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        private const string RegistryValueName = "AppsUseLightTheme";

        public MainWindow()
        {
            InitializeComponent();          
            SourceInitialized += OnSourceInitialized;
            Closing += OnClosing;
        }

        private void OnClosing(object? sender, CancelEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                HomeAssistant.Properties.Settings.Default.Top = RestoreBounds.Top;
                HomeAssistant.Properties.Settings.Default.Left = RestoreBounds.Left;
                HomeAssistant.Properties.Settings.Default.Height = RestoreBounds.Height;
                HomeAssistant.Properties.Settings.Default.Width = RestoreBounds.Width;
                HomeAssistant.Properties.Settings.Default.Maximized = true;
            }
            else
            {
                HomeAssistant.Properties.Settings.Default.Top = this.Top;
                HomeAssistant.Properties.Settings.Default.Left = this.Left;
                HomeAssistant.Properties.Settings.Default.Height = this.Height;
                HomeAssistant.Properties.Settings.Default.Width = this.Width;
                HomeAssistant.Properties.Settings.Default.Maximized = false;
            }

            HomeAssistant.Properties.Settings.Default.Save();

            SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;
        }

        protected override async void OnContentRendered(EventArgs e)
        {
            var environment = await CoreWebView2Environment.CreateAsync(null, CacheFolderPath);
            await WebView.EnsureCoreWebView2Async(environment);

            WebView.Source = new UriBuilder(HomeAssistant.Properties.Settings.Default.URL).Uri;
        }

        private void OnSourceInitialized(object? sender, EventArgs e)
        {
            var source = (HwndSource)PresentationSource.FromVisual(this);
            source.AddHook(WndProc);

            this.Top = HomeAssistant.Properties.Settings.Default.Top;
            this.Left = HomeAssistant.Properties.Settings.Default.Left;
            this.Height = HomeAssistant.Properties.Settings.Default.Height;
            this.Width = HomeAssistant.Properties.Settings.Default.Width;

            if (HomeAssistant.Properties.Settings.Default.Maximized)
            {
                WindowState = WindowState.Maximized;
            }

            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;

            this.HandleWindowsTHeme();
        }

        private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            this.HandleWindowsTHeme();
        }

        private void HandleWindowsTHeme()
        {
            if (LightTheme())
            {
                this.titlebar.Background = new SolidColorBrush(Colors.White);
                this.Background = new SolidColorBrush(Colors.White);
                this.titlebarText.Foreground = new SolidColorBrush(Colors.Black);
                this.closeButton.Foreground = new SolidColorBrush(Colors.Black);
                this.maximizeRestoreButton.Foreground = new SolidColorBrush(Colors.Black);
                this.maximizeRestoreButton.Background = new SolidColorBrush(Colors.White);
                this.minimizeButton.Foreground = new SolidColorBrush(Colors.Black);
                this.minimizeButton.Background = new SolidColorBrush(Colors.White);
                this.optionsButton.Foreground = new SolidColorBrush(Colors.Black);
                this.optionsButton.Background = new SolidColorBrush(Colors.White);
            }
            else
            {
                this.titlebar.Background = new SolidColorBrush(Color.FromRgb(28,28,28));
                this.Background = new SolidColorBrush(Color.FromRgb(28, 28, 28));
                this.titlebarText.Foreground = new SolidColorBrush(Colors.White);
                this.closeButton.Foreground = new SolidColorBrush(Colors.White);
                this.maximizeRestoreButton.Foreground = new SolidColorBrush(Colors.White);
                this.maximizeRestoreButton.Background = new SolidColorBrush(Color.FromRgb(28, 28, 28));
                this.minimizeButton.Foreground = new SolidColorBrush(Colors.White);
                this.minimizeButton.Background = new SolidColorBrush(Color.FromRgb(28, 28, 28));
                this.optionsButton.Foreground = new SolidColorBrush(Colors.White);
                this.optionsButton.Background = new SolidColorBrush(Color.FromRgb(28, 28, 28));
            }

            this.minimizeButton.MouseEnter += Button_MouseEnter;
            this.minimizeButton.MouseLeave += Button_MouseLeave;
            this.maximizeRestoreButton.MouseEnter += Button_MouseEnter;
            this.maximizeRestoreButton.MouseLeave += Button_MouseLeave;
            this.closeButton.MouseEnter += CloseButton_MouseEnter;
            this.closeButton.MouseLeave += CloseButton_MouseLeave;
            this.optionsButton.MouseEnter += Button_MouseEnter;
            this.optionsButton.MouseLeave += Button_MouseLeave;
        }

        private void CloseButton_MouseEnter(object sender, MouseEventArgs e)
        {
            if (LightTheme()) { ((Button)sender).Foreground = new SolidColorBrush(Colors.White); }
        }

        private void CloseButton_MouseLeave(object sender, MouseEventArgs e)
        {
            if (LightTheme()) { ((Button)sender).Foreground = new SolidColorBrush(Colors.Black); }
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            if (LightTheme())
            {
                ((Button) sender).Background = new SolidColorBrush(Colors.White);
            }
            else
            {
                ((Button)sender).Background = new SolidColorBrush(Color.FromRgb(28,28,28));
            }
        }

        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            if (LightTheme())
            {
                ((Button)sender).Background = new SolidColorBrush(Colors.LightGray);
            }
            else
            {
                ((Button)sender).Background = new SolidColorBrush(Color.FromRgb(61,61,61));
            }
        }

        private static bool LightTheme()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath))
            {
                object registryValueObject = key?.GetValue(RegistryValueName);
                if (registryValueObject == null)
                {
                    return true;
                }

                int registryValue = (int)registryValueObject;

                return registryValue > 0;
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case NativeHelpers.WM_NCHITTEST:
                    if (NativeHelpers.IsSnapLayoutEnabled())
                    {
                        // Return HTMAXBUTTON when the mouse is over the maximize/restore button
                        var point = PointFromScreen(new Point(lParam.ToInt32() & 0xFFFF, lParam.ToInt32() >> 16));
                        if (WpfHelpers.GetElementBoundsRelativeToWindow(maximizeRestoreButton, this).Contains(point))
                        {
                            handled = true;
                            // Apply hover button style
                            maximizeRestoreButton.Background = (Brush)App.Current.Resources["TitleBarButtonHoverBackground"];
                            maximizeRestoreButton.Foreground = (Brush)App.Current.Resources["TitleBarButtonHoverForeground"];
                            return new IntPtr(NativeHelpers.HTMAXBUTTON);
                        } else
                        {
                            // Apply default button style (cursor is not on the button)
                            maximizeRestoreButton.Background = (Brush)App.Current.Resources["TitleBarButtonBackground"];
                            maximizeRestoreButton.Foreground = (Brush)App.Current.Resources["TitleBarButtonForeground"];
                        }
                    }
                    break;
                case NativeHelpers.WM_NCLBUTTONDOWN:
                    if (NativeHelpers.IsSnapLayoutEnabled())
                    {
                        if (wParam.ToInt32() == NativeHelpers.HTMAXBUTTON)
                        {
                            handled = true;
                            // Apply pressed button style
                            maximizeRestoreButton.Background = (Brush)App.Current.Resources["TitleBarButtonPressedBackground"];
                            maximizeRestoreButton.Foreground = (Brush)App.Current.Resources["TitleBarButtonPressedForeground"];
                        }
                    }
                    break;
                case NativeHelpers.WM_NCLBUTTONUP:
                    if (NativeHelpers.IsSnapLayoutEnabled())
                    {
                        if (wParam.ToInt32() == NativeHelpers.HTMAXBUTTON)
                        { 
                            // Apply default button style
                            maximizeRestoreButton.Background = (Brush)App.Current.Resources["TitleBarButtonBackground"];
                            maximizeRestoreButton.Foreground = (Brush)App.Current.Resources["TitleBarButtonForeground"];
                            // Maximize or restore the window
                            ToggleWindowState();
                        }
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        private void OnOptionsButtonClick(object sender, RoutedEventArgs e)
        {
            var dialog = new HomeAssistant.OptionsWindow();
            dialog.Show();
            dialog.Closing += (sender, e) =>
            {
                var d = sender as HomeAssistant.OptionsWindow;
                if (!d.Canceled)
                {
                    var NewUrl = d.InputText;
                    HomeAssistant.Properties.Settings.Default.URL = NewUrl;
                    WebView.Source = new UriBuilder(HomeAssistant.Properties.Settings.Default.URL).Uri;
                }
            };
        }

        private void OnMinimizeButtonClick(object sender, RoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        private void OnMaximizeRestoreButtonClick(object sender, RoutedEventArgs e)
        {
            ToggleWindowState();
        }        

        private void maximizeRestoreButton_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            maximizeRestoreButton.ToolTip = WindowState == WindowState.Normal ? "Maximize" : "Restore";
        }

        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        private void NewWindowMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var w = new MainWindow();
            w.WindowState = WindowState.Normal;
            w.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            w.Show();
        }

        private void QuitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Icon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && e.ChangedButton == MouseButton.Left)
            {
                Close();
            }
            else if (e.ChangedButton == MouseButton.Left || e.ChangedButton == MouseButton.Right)
            {
                ShowSystemMenu(e.GetPosition(this));
            }
        }

        public void ToggleWindowState()
        {
            if (WindowState == WindowState.Maximized)
            {
                SystemCommands.RestoreWindow(this);
            }
            else
            {
                SystemCommands.MaximizeWindow(this);
            }
        }

        public void ShowSystemMenu(Point point)
        {
            // Increment coordinates to allow double-click
            ++point.X;
            ++point.Y;
            if (WindowState == WindowState.Normal)
            {
                point.X += Left;
                point.Y += Top;
            }
            SystemCommands.ShowSystemMenu(this, point);
        }
    }
}
