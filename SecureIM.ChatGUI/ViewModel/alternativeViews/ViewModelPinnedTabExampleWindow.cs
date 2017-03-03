﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using GalaSoft.MvvmLight.Command;
using PostSharp.Patterns.Diagnostics;
using SecureIM.ChatBackend;
using SecureIM.ChatBackend.model;
using SecureIM.ChatGUI.ViewModel.interfaces;
using SecureIM.ChatGUI.ViewModel.TabClasses;

namespace SecureIM.ChatGUI.ViewModel.alternativeViews
{
    public sealed class ViewModelPinnedTabExampleWindow : ViewModelExampleBase, IViewModelPinnedTabExampleWindow
    {
        #region Public Properties

        public RelayCommand<User> AddChatTabCommand { get; set; }

        public RelayCommand<TabBase> PinTabCommand { get; set; }

        #endregion Public Properties

        #region Public Constructors

        [Log("MyProf")]
        public ViewModelPinnedTabExampleWindow()
        {
            TabBase vm1 = CreateTabChatMain();
            vm1.IsPinned = true;
            ItemCollection.Add(vm1);
            ItemCollection.Add(CreateTabChatWindow(new User("")));
            //            ItemCollection.Add(CreateTab3());
            //            ItemCollection.Add(CreateTabLoremIpsum());
            SelectedTab = ItemCollection.FirstOrDefault();
            ICollectionView view = CollectionViewSource.GetDefaultView(ItemCollection);
            //This sort description is what keeps the source collection sorted, based on tab number.
            //You can also use the sort description to manually sort the tabs, based on your own criterias,
            //as show below by sorting both by tab number and Pinned status.
            view.SortDescriptions.Add(new SortDescription("IsPinned", ListSortDirection.Descending));
            view.SortDescriptions.Add(new SortDescription("TabNumber", ListSortDirection.Ascending));

            PinTabCommand = new RelayCommand<TabBase>(PinTabCommandAction);
            AddChatTabCommand = new RelayCommand<User>(AddTabCommandAction);
            ChatBackend.ChatBackend.Instance.ProcessMessageDelegate = ProcessMessage;
        }

        #endregion Public Constructors

        #region Public Methods

        //Adds a random tab
        [Log("MyProf")]
        public void AddTabCommandAction(User user)
        {
            TabChatWindow newTab = CreateTabChatWindow(user);

            var bind = new Binding
            {
                Source = newTab,
                Path = new PropertyPath("TargetUser")
            };
            newTab.TargetUser = user;
            bind.Mode = BindingMode.TwoWay;
            bind.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

            ItemCollection.Add(newTab);
        }

        #endregion Public Methods

        #region Private Methods

        [Log("MyProf")]
        private static void AddMessageToChat(MessageComposite messageComposite, TabChatWindow chatTab)
        {
            string username = messageComposite.Sender.Name ?? "";
            string message = messageComposite.Message.Text ?? "";

            string newChatLog = $"{chatTab?.ChatHistory}{username}: {message}{Environment.NewLine}";
            chatTab.ChatHistory = newChatLog;
        }

        [Log("MyProf")]
        private static void AddMessageToChat(MessageComposite messageComposite, TabChatMain chatMain)
        {
            string username = messageComposite.Sender.Name ?? "";
            string message = messageComposite.Message.Text ?? "";

            string newChatLog = $"{chatMain?.ChatHistory}{username}: {message}{Environment.NewLine}";
            chatMain.ChatHistory = newChatLog;
        }

        [Log("MyProf")]
        private static bool MatchTabToTargetUser(MessageComposite messageComposite, TabBase tab)
        {
            if (tab.IsPinned)
            {
                return messageComposite.Sender.PublicKey.Equals("event") || messageComposite.Sender.PublicKey.Equals("info");
            }

            var chatTab = tab as TabChatWindow;
            return chatTab != null && chatTab.TargetUser.PublicKey.Equals(messageComposite.Sender.PublicKey);
        }

        [Log("MyProf")]
        private void PinTabCommandAction(TabBase tab)
        {
            tab.IsPinned = !tab.IsPinned;
            ICollectionView view = CollectionViewSource.GetDefaultView(ItemCollection);
            view.Refresh();
        }

        [Log("MyProf")]
        private void ProcessMessage(MessageComposite messageComposite, DisplayMessageDelegate dmd)
        {
            //TODO: do stuff

            IEnumerable<TabBase> matchedTab = ItemCollection.Where(tab => MatchTabToTargetUser(messageComposite, tab));

            IEnumerable<TabBase> tabBases = matchedTab as TabBase[] ?? matchedTab.ToArray();
            if (!tabBases.Any())
            {
                //TODO: create new tab

                TabChatWindow newChatTab = CreateTabChatWindow(messageComposite.Sender);
                ItemCollection.Add(newChatTab);
                AddMessageToChat(messageComposite, newChatTab);
            }
            else
            {
                //TODO: add to chat history of tab
                if (tabBases.First().IsPinned)
                {
                    var matchedChatMain = tabBases.First() as TabChatMain;
                    if (matchedChatMain != null) AddMessageToChat(messageComposite, matchedChatMain);
                }
                else
                {
                    var matchedChatTab = tabBases.First() as TabChatWindow;
                    if (matchedChatTab != null) AddMessageToChat(messageComposite, matchedChatTab);
                }
            }
        }

        #endregion Private Methods
    }
}