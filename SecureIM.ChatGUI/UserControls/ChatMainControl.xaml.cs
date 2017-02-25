﻿using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using SecureIM.ChatBackend.model;

namespace SecureIM.ChatGUI.UserControls
{
    /// <summary>
    ///     Interaction logic for ChatWindowControl.xaml
    /// </summary>
    public partial class ChatMainControl
    {
        #region Public Properties

        public ChatBackend.ChatBackend Backend { get; }

        #endregion Public Properties

        #region Public Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ChatWindowControl" /> class.
        /// </summary>
        public ChatMainControl()
        {
            InitializeComponent();
            Backend = ChatBackend.ChatBackend.Instance;
            Backend.DisplayMessageDelegate = DisplayMessage;
            ScrollToEnd();

            if (!Backend.ServiceStarted)
                Task.Run(() => Backend.StartService());
        }

        #endregion Public Constructors

        #region Public Methods

        /// <summary>
        ///     Displays the given message in the user's gui
        /// </summary>
        /// <param name="messageComposite">
        ///     The delegate method that tells the backend how to display messages received from other
        ///     users
        /// </param>
        public void DisplayMessage(MessageComposite messageComposite)
        {
            string username = messageComposite.Sender.Name ?? "";
            string message = messageComposite.Message.Text ?? "";
            Dispatcher.InvokeAsync(() =>
            {
                TxtChatPane.AppendText(username + ": " + message + Environment.NewLine);

                BindingExpression exp = TxtChatPane.GetBindingExpression(TextBox.TextProperty);
                exp?.UpdateSource();
            });
        }

        #endregion Public Methods

        #region Private Methods

        private void BtnAddFriend_Click(object sender, RoutedEventArgs e)
        {
            string friendAlias = TxtAlias.Text;
            string friendPubKey = TxtFriendPublicKey.Text;
            SendCommand($"addfriend:{friendAlias}:{friendPubKey}");
        }

        private void BtnGenKeyPair_Click(object sender, RoutedEventArgs e)
        {
            SendCommand("genkey:");
            SendCommand("regpub:");
        }

        private void BtnGetPubKey_Click(object sender, RoutedEventArgs e) => SendCommand("getpub:");

        private void BtnSetName_Click(object sender, RoutedEventArgs e)
        {
            string newName = TxtSetName.Text;
            SendCommand($"setname:{newName}");
        }

        private void ScrollToEnd()
        {
            if (TxtChatPane == null) return;

            TxtChatPane.SelectionStart = TxtChatPane.Text.Length;
            TxtChatPane.ScrollToEnd();
            TxtEntryField.Focus();
        }

        private void SendCommand(string commandString)
        {
            //            var oldDelegate = Backend.DisplayMessageDelegate;
            //            Backend.DisplayMessageDelegate = DisplayMessage;

            Backend.SendMessage(commandString);
            TxtEntryField.Clear();
            //
            //            if (oldDelegate != null) Backend.DisplayMessageDelegate = oldDelegate;
        }

        private void TextBoxEntryField_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (!(e.Key == Key.Return || e.Key == Key.Enter)) return;

            Dispatcher.InvokeAsync(() =>
            {
                Backend.SendMessage(TxtEntryField.Text);
                TxtEntryField.Clear();
            });

            TxtEntryField.Focus();
        }

        private void TxtChatPane_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) => ScrollToEnd();

        private void TxtChatPane_TextChanged(object sender, TextChangedEventArgs e) => ScrollToEnd();

        #endregion Private Methods
    }
}