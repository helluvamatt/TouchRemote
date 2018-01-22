using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections;
using TouchRemote.Utils;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using TouchRemote.Properties;
using System.Windows.Input;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace TouchRemote.Model
{
    internal class IPWhitelist : INotifyPropertyChanged
    {
        #region Dependency properties

        private IPWhitelistEntry _NewEntry;
        public IPWhitelistEntry NewEntry
        {
            get
            {
                return _NewEntry;
            }
            set
            {
                PropertyChanged.ChangeAndNotify(ref _NewEntry, value, () => NewEntry);
            }
        }

        private int _SelectedEntryIndex;
        public int SelectedEntryIndex
        {
            get
            {
                return _SelectedEntryIndex;
            }
            set
            {
                PropertyChanged.ChangeAndNotify(ref _SelectedEntryIndex, value, () => SelectedEntryIndex);
            }
        }

        public ObservableCollection<IPWhitelistEntry> Entries { get; private set; }

        public ICommand AddEntryCommand { get; private set; }

        public ICommand RemoveEntryCommand { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public IPWhitelist()
        {
            AddEntryCommand = new DelegateCommand(AddEntry);
            RemoveEntryCommand = new DelegateCommand(RemoveEntry);
            Entries = new ObservableCollection<IPWhitelistEntry>();
            NewEntry = new IPWhitelistEntry();
            if (Settings.Default.IPWhitelist != null)
            {
                foreach (string entry in Settings.Default.IPWhitelist)
                {
                    Entries.Add(new IPWhitelistEntry(entry));
                }
            }
            Entries.CollectionChanged += Entries_CollectionChanged;
        }

        public void AddEntry()
        {
            if (!NewEntry.HasErrors)
            {
                Entries.Add(NewEntry);
                NewEntry = new IPWhitelistEntry();
            }
        }

        public void RemoveEntry()
        {
            if (SelectedEntryIndex >= 0)
            {
                Entries.RemoveAt(SelectedEntryIndex);
            }
        }

        private void Entries_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var coll = new StringCollection();
            coll.AddRange(Entries.Select(entry => entry.Value).ToArray());
            Settings.Default.IPWhitelist = coll;
        }
    }

    internal class IPWhitelistEntry : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        #region Dependency properties

        private string _Value;
        public string Value
        {
            get
            {
                return _Value;
            }
            set
            {
                PropertyChanged.ChangeAndNotify(ref _Value, value, () => Value, OnValueChanged);
            }
        }

        private void OnValueChanged(string oldValue, string newValue)
        {
            _errors.Clear();
            OnErrorsChanged("NewEntry");

            if (string.IsNullOrEmpty(Value))
            {
                _errors.TryAdd("Value", new List<string> { "Please enter a valid IP adderss." });
                OnErrorsChanged("Value");
                return;
            }

            IPAddress addr;
            if (!IPAddress.TryParse(Value, out addr))
            {
                _errors.TryAdd("Value", new List<string> { "Please enter a valid IP address." });
                OnErrorsChanged("Value");
                return;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public IPWhitelistEntry(string entry)
        {
            _Value = entry;
        }

        public IPWhitelistEntry() { }

        #region Validation

        private ConcurrentDictionary<string, List<string>> _errors = new ConcurrentDictionary<string, List<string>>();

        public bool HasErrors => _errors.Any(kv => kv.Value != null && kv.Value.Count > 0);

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public IEnumerable GetErrors(string propertyName)
        {
            List<string> errorsForName;
            _errors.TryGetValue(propertyName, out errorsForName);
            return errorsForName;
        }

        public void OnErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        #endregion
    }
}
