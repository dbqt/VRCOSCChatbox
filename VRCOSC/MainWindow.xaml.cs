using System.Collections.Concurrent;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using VRCOSC.Properties;

namespace VRCOSC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string vrcAddress = "127.0.0.1";
        private int vrcPort = 9000;

        private string currentText = string.Empty;

        private SharpOSC.UDPSender? sender = null;

        private ConcurrentStack<string> messageBuffer = new ConcurrentStack<string>();

        private Task? currentMessageTask = null;

        private SharpOSC.UDPSender Sender 
        { 
            get
            {
                if (sender == null)
                {
                    sender = new SharpOSC.UDPSender(vrcAddress, vrcPort);
                }
                return sender;
            }
        }

        public string CurrentText {
            get 
            { 
                return currentText;
            } 
            set
            { 
                currentText = value;
                OnPropertyChanged(nameof(CurrentText));
            } 
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            UpdateCharacterCount();
            SendWhileTypingCheckBox.IsChecked = Settings.Default.SendWhileTyping;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            SaveSettings();
            base.OnClosing(e);
        }

        public void SendOSCMessage()
        {
            QueueMessage();
        }

        public void SendOSCTypingSignal(bool typing)
        {
            if (SendWhileTypingCheckBox.IsChecked.GetValueOrDefault())
            {
                QueueMessage();
            }

            // /chatbox/typing b
            var message = new SharpOSC.OscMessage("/chatbox/typing", typing);
            Sender.Send(message);
        }

        private void ClearMessage()
        {
            ChatBox.Text = string.Empty;
        }

        private void UpdateCharacterCount()
        {
            NumberLetter.Text = $"({ChatBox.Text.Length}/144)";

            SendOSCTypingSignal(ChatBox.Text.Length > 0);
        }

        private void QueueMessage()
        {
            messageBuffer.Push(CurrentText);

            if (currentMessageTask == null)
            {
                currentMessageTask = Task.Run(async () =>
                {
                    // Keep sending while we have messages to process
                    while (messageBuffer.Count > 0)
                    {
                        // Only process if we have messages
                        if (messageBuffer.TryPop(out var result) && !string.IsNullOrWhiteSpace(result))
                        {
                            // Only clear if we have a non-empty message to avoid clearing prematurely
                            messageBuffer.Clear();
                            var message = new SharpOSC.OscMessage("/chatbox/input", result, true);
                            Sender.Send(message);

                            // Delay suggested by vrc
                            await Task.Delay(1500);
                        }
                    }
                    currentMessageTask = null;
                });
            }
        }

        private void SaveSettings()
        {
            Settings.Default.SendWhileTyping = SendWhileTypingCheckBox.IsChecked.GetValueOrDefault();
            Settings.Default.Save();
        }

        private void SendClick(object sender, RoutedEventArgs e)
        {
            SendOSCMessage();
            ClearMessage();
        }

        private void TextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                SendOSCMessage();
                ClearMessage();
            }
        }

        private void TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            UpdateCharacterCount();
        }

        private void SendWhileTypingCheckBoxChanged(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }
    }
}
