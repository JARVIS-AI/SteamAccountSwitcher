﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Gameloop.Vdf;

namespace SteamAccountSwitcher
{
    public class SteamAccountCollection : ObservableCollection<SteamAccount>
    {
        private FileSystemWatcher _watcher;
        private string _installDirectory;
        private string _configDirectory;

        public void SetDirectory(string installDirectory)
        {
            if (string.IsNullOrEmpty(installDirectory) || !Directory.Exists(installDirectory))
                throw new ArgumentException("The directory doesn't exist.", nameof(installDirectory));

            _installDirectory = installDirectory;
            _configDirectory = Path.Combine(installDirectory, "config");

            if (_watcher != null)
                _watcher.Dispose();

            _watcher = new(_configDirectory, "loginusers.vdf")
            {
                EnableRaisingEvents = true
            };
            _watcher.Changed += OnFileChanged;

            Reload();
        }

        public void Reload()
        {
            if (string.IsNullOrEmpty(_installDirectory) || !Directory.Exists(_installDirectory))
                throw new InvalidOperationException("Can't reload; the directory doesn't exist!");

            Clear();
            foreach (var account in GetAccounts(_configDirectory))
                Add(account);
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            App.Current.Dispatcher.Invoke(() => Reload());
        }

        private IEnumerable<SteamAccount> GetAccounts(string configDirectory)
        {
            var loginUsersVdfPath = Path.Combine(configDirectory, "loginusers.vdf");
            dynamic loginUsers = VdfConvert.Deserialize(File.ReadAllText(loginUsersVdfPath));

            foreach (var loginUser in loginUsers.Value)
            {
                yield return new()
                {
                    //ID = loginUser.Key,
                    Name = loginUser.Value.AccountName.Value,
                    Alias = loginUser.Value.PersonaName.Value,
                };
            }
        }
    }
}