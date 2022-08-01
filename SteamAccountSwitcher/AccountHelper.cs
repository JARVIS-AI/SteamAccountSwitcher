﻿using System;
using System.ComponentModel;
using System.Windows;

namespace SteamAccountSwitcher
{
    public static class AccountHelper
    {
        public static void New()
        {
            var dialog = new AccountProperties();

            dialog.ShowDialog();

            if (dialog.DialogResult != true || dialog.NewAccount == null)
                return;

            if (string.IsNullOrWhiteSpace(dialog.NewAccount.Username) &&
                string.IsNullOrWhiteSpace(dialog.NewAccount.Password) &&
                string.IsNullOrWhiteSpace(dialog.NewAccount.DisplayName))
            {
                return;
            }

            Add(dialog.NewAccount);
        }

        private static void Add(Account account)
        {
            account.AddDate = DateTime.Now;
            App.Accounts.Add(account);
        }

        public static void Remove(this Account account, bool msg = false)
        {
            if (msg &&
                Alert.Show(
                    $"Are you sure you want to remove \"{account.GetDisplayName()}\"?",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning,
                    MessageBoxResult.Yes) == MessageBoxResult.No)
            {
                return;
            }

            App.Accounts.Remove(account);
        }

        public static void Edit(this Account account)
        {
            var dialog = new AccountProperties(account);
            dialog.ShowDialog();
            if (dialog.DialogResult != true)
                return;

            var newAccount = dialog.NewAccount;
            if (newAccount == null)
                return;

            newAccount.LastModifiedDate = DateTime.Now;
            var accountIndex = App.Accounts.IndexOf(account);
            if (accountIndex == -1)
                return;

            App.Accounts[accountIndex] = newAccount;
        }

        public static Account MoveUp(this Account account) =>
            App.Accounts.MoveUp(account);

        public static Account MoveDown(this Account account) =>
            App.Accounts.MoveDown(account);

        public static void SwitchTo(this Account account, bool onStart = false)
        {
            var worker = new BackgroundWorker();
            worker.DoWork += delegate
            {
                SteamClient.LogoutWithTimeout();
                SteamClient.Login(account, onStart);
            };

            worker.RunWorkerAsync();
        }
    }
}