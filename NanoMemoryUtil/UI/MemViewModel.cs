using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using NanoMemUtil.Code;
using NanoMemUtil.Properties;

namespace NanoMemUtil.UI
{
    public class MemViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<string> _availableProcesses = new ObservableCollection<string>();
        private MemCommand _refreshAvailableProcesses;
        private string _value;
        private bool _isHexFormat;
        private int _scanTypeIndexSelected;
        private List<string> _scanType;
        private int _valueTypeIndexSelected;
        private List<string> _valueType;
        private MemCommand _newScanCommand;
        private MemCommand _nextScanCommand;
        private string _selectedProcess;
        private int _availableProcessesIndexselected;
        private List<MemoryDto> _foundValues;
        private long _completedPercent;
        private string _currentAddressInfo;
        private bool _isNextScanEnabled;
        private List<MemoryDto> _foundValuesSnapshot;
        private MemCommand _applyValue;
        private int _selectedIndexFoundValue;
        private string _newValue;
        private Brush _status = Brushes.Transparent;

        public ulong MaxVal => 0x7FFFFFFF;

        //
        public ObservableCollection<string> AvailableProcesses
        {
            get => _availableProcesses;
            set
            {
                _availableProcesses = value;
                OnPropertyChanged(nameof(AvailableProcesses));
            }
        }

        public int AvailableProcessesIndexSelected
        {
            get => _availableProcessesIndexselected;
            set
            {
                _availableProcessesIndexselected = value;
                OnPropertyChanged();
            }
        }

        public string SelectedProcess
        {
            get => _selectedProcess;
            set
            {
                _selectedProcess = value;
                OnPropertyChanged();
            }
        }

        //

        public MemCommand NewScanCommand
        {
            get => _newScanCommand;
            set
            {
                _newScanCommand = value;
                OnPropertyChanged();
            }
        }

        public MemCommand NextScanCommand
        {
            get => _nextScanCommand;
            set
            {
                _nextScanCommand = value;
                OnPropertyChanged();
            }
        }

        public MemCommand RefreshAvailableProcesses
        {
            get => _refreshAvailableProcesses;
            set
            {
                _refreshAvailableProcesses = value;
                OnPropertyChanged();
            }
        }

        //

        public string Value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged();
            }
        }

        public bool IsHexFormat
        {
            get => _isHexFormat;
            set
            {
                _isHexFormat = value;
                OnPropertyChanged();
            }
        }

        //

        public int ScanTypeIndexSelected
        {
            get => _scanTypeIndexSelected;
            set
            {
                _scanTypeIndexSelected = value;
                OnPropertyChanged();
            }
        }

        public List<string> ScanType
        {
            get => _scanType;
            set
            {
                _scanType = value;
                OnPropertyChanged();
            }
        }

        //

        public int ValueTypeIndexSelected
        {
            get => _valueTypeIndexSelected;
            set
            {
                _valueTypeIndexSelected = value;
                OnPropertyChanged();
            }
        }

        public List<string> ValueType
        {
            get => _valueType;
            set
            {
                _valueType = value;
                OnPropertyChanged();
            }
        }

        //

        public int SelectedIndexFoundValue
        {
            get => _selectedIndexFoundValue;
            set
            {
                _selectedIndexFoundValue = value;
                OnPropertyChanged();
            }
        }

        public List<MemoryDto> FoundValues
        {
            get => _foundValues;
            set
            {
                _foundValues = value;
                OnPropertyChanged();
            }
        }

        public List<MemoryDto> FoundValuesSnapshot
        {
            get => _foundValuesSnapshot;
            set
            {
                _foundValuesSnapshot = value;
                OnPropertyChanged();
            }
        }

        //

        public long CompletedPercent
        {
            get => _completedPercent;
            set
            {
                _completedPercent = value;
                OnPropertyChanged();
            }
        }

        public string CurrentAddressInfo
        {
            get => _currentAddressInfo;
            set
            {
                _currentAddressInfo = value;
                OnPropertyChanged();
            }
        }

        public bool IsNextScanEnabled
        {
            get => _isNextScanEnabled;
            set
            {
                _isNextScanEnabled = value;
                OnPropertyChanged();
            }
        }

        public MemCommand ApplyValue
        {
            get => _applyValue;
            set
            {
                _applyValue = value;
                OnPropertyChanged();
            }
        }

        public string NewValue
        {
            get => _newValue;
            set
            {
                _newValue = value;
                OnPropertyChanged();
            }
        }

        public Brush Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }


        //
        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class MemCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            Action?.Invoke();
        }

        public event EventHandler CanExecuteChanged;

        public Action Action { get; set; }
    }


}
