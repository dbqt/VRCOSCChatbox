
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

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
        }

        public void SendOSCMessage()
        {
            // /chatbox/input s b
            var message = new SharpOSC.OscMessage("/chatbox/input", CurrentText, true);
            var sender = new SharpOSC.UDPSender(vrcAddress, vrcPort);
            sender.Send(message);
        }

        private void ClearMessage()
        {
            ChatBox.Text = string.Empty;
        }

        private void UpdateCharacterCount()
        {
            NumberLetter.Text = $"({ChatBox.Text.Length}/144)";
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
    }
}
