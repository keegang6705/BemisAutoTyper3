using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System;

namespace BemisAutoTyper3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// dotnet publish -c Release --self-contained=true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true


    public partial class MainWindow : System.Windows.Window
    {
        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
        private bool IS_RUNNING = false;
        private const int KEYEVENTF_KEYDOWN = 0x0000;
        private const int KEYEVENTF_KEYUP = 0x0002;
        public MainWindow()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        private void TextBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalChange != 0)
            {
                lineNumbersScrollViewer.ScrollToVerticalOffset(e.VerticalOffset);
            }
        }


        private void IntervalTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Validate the input when focus is lost
            System.Windows.Controls.TextBox textBox = (System.Windows.Controls.TextBox)sender;
            string newText = textBox.Text;
            switch (IsValidInt(newText))
            {
                case "match":
                    break;
                case "empty":
                    textBox.Text = "10";
                    break;
                case "1000+":
                    textBox.Text = "1000";
                    break;
                case "1-":
                    textBox.Text = "1";
                    break;

            }
        }

        private string IsValidInt(string text)
        {
            // Use regex to validate the input as a float number between 0 and 1000
            Regex regex = new Regex(@"^(1000|[1-9]\d{0,2}|1)$");
            if (regex.IsMatch(text))
            {
                return "match";
            }
            else if (string.IsNullOrEmpty(text))
            {
                return "empty";
            }
            else
            {
                try
                {
                    if (int.Parse(text) > 1000)
                    {
                        return "1000+";
                    }
                    else if (int.Parse(text) < 0)
                    {
                        return "1-";
                    }
                    else
                    {
                        return "empty";
                    }
                }
                catch
                {
                    return "1000+";
                }
            };
        }

        private void TurboModeCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            IntervalTextBox.IsEnabled = false;
        }
        private void TurboModeCheckBox_UnChecked(object sender, RoutedEventArgs e)
        {
            IntervalTextBox.IsEnabled = true;
        }


        private async void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F8)
            {
                // Get the text from the TextBox
                string textToType = DataTextBox.Text;
                int interval;
                if (!int.TryParse(IntervalTextBox.Text, out interval))
                {
                    // Handle invalid input from IntervalTextBox
                    MessageBox.Show("Invalid interval value.");
                    return;
                }

                // Clear progress bar before typing
                TypeProgressbar.Value = 0;

                // Start typing based on the specified settings
                await StartTypingAsync(textToType, interval, TurboModeCheckBox.IsChecked);
            }
        }
        private async Task C_Delay(int time)
        {
            await Task.Delay(time);
        }
        private async Task StartTypingAsync(string text, int interval, bool? turboMode)
        {
            if (IS_RUNNING)
            {
                return;
            }
            else
            {
                IS_RUNNING = true;
                StartButton.IsEnabled = false;
                DataTextBox.IsEnabled = false;
            }
            int totalCharacters = text.Length;

            if (turboMode ?? false)
            {
                for (int i = 0; i < text.Length; i++)
                {
                    // Update progress bar
                    Application.Current.Dispatcher.Invoke(() => TypeProgressbar.Value = ((i + 1) * 100 / totalCharacters) / 2);

                    keybd_event((byte)char.ToUpper(text[i]), 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero); // Key down
                    keybd_event((byte)char.ToUpper(text[i]), 0, KEYEVENTF_KEYUP, UIntPtr.Zero);   // Key up
                }
            }
            else
            {
                for (int i = 0; i < text.Length; i++)
                {
                    // Update progress bar
                    Application.Current.Dispatcher.Invoke(() => TypeProgressbar.Value = ((i + 1) * 100 / totalCharacters) / 2);

                    keybd_event((byte)char.ToUpper(text[i]), 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero); // Key down
                    keybd_event((byte)char.ToUpper(text[i]), 0, KEYEVENTF_KEYUP, UIntPtr.Zero);   // Key up

                    await Task.Delay(interval);
                }
            }

            // Ensure the progress bar reaches 100% after typing is completed
            Application.Current.Dispatcher.Invoke(() => TypeProgressbar.Value = 100);
            IS_RUNNING = false;
            StartButton.IsEnabled = true;
            DataTextBox.IsEnabled = true;
            await Task.Delay(250);
            Application.Current.Dispatcher.Invoke(() => TypeProgressbar.Value = 0);
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {

            StartButton.IsEnabled = false;
            DataTextBox.IsEnabled = false;
            StartButton.Content = "3\n";
            await C_Delay(1000);
            StartButton.Content = "2\n";
            await C_Delay(1000);
            StartButton.Content = "1\n";
            await C_Delay(1000);
            StartButton.Content = "กำลังพิมพ์\n";
            await C_Delay(250);
            // Get the text from the TextBox
            string textToType = DataTextBox.Text;
            textToType = Regex.Replace(textToType, "\r\n|\n", "\t\t");
            int interval;
            if (!int.TryParse(IntervalTextBox.Text, out interval))
            {
                // Handle invalid input from IntervalTextBox
                MessageBox.Show("Invalid interval value.");
                return;
            }

            // Clear progress bar before typing
            TypeProgressbar.Value = 0;

            // Start typing based on the specified settings
            await StartTypingAsync(textToType, interval, TurboModeCheckBox.IsChecked);
            StartButton.Content = "สำเร็จ\n";
            await C_Delay(500);
            StartButton.Content = "Start\n";

        }
    }
}
