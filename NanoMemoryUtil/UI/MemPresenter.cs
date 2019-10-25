using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using NanoMemUtil.Code;
using NanoMemUtil.Code.MVVM;

namespace NanoMemUtil.UI
{
    public enum MemValueType
    {
        Byte = 1,
        Short = 2,
        UShort = 2,
        Int = 4,
        UInt = 4,
        Long = 8,
        ULong = 8
    }

    public class AddressComparer : IEqualityComparer<MemoryDto>
    {
        public bool Equals(MemoryDto x, MemoryDto y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;
            return x.Address == y.Address;
        }

        public int GetHashCode(MemoryDto obj)
        {
            return obj.GetHashCode();
        }
    }

    public class MemPresenter : PresenterBase<MemView,MemViewModel>
    {

        public int y = 1111;

        private const ulong ChunkSize = 10000000;

        public enum ScanType
        {
            DecreasedValueBy,
            IncreasedValueBy
        }

      

       

        public MemPresenter(MemView view, MemViewModel vm) : 
            base(view, vm)
        {
            ViewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(MemViewModel.AvailableProcessesIndexSelected))
                {
                    if (ViewModel.AvailableProcessesIndexSelected >= 0)
                    {
                        ViewModel.SelectedProcess = ViewModel.AvailableProcesses[ViewModel.AvailableProcessesIndexSelected];
                    }
                    else
                    {
                        ViewModel.SelectedProcess = string.Empty;
                    }
                }
            };
        }

        public override void OnViewReady()
        {
            ViewModel.ApplyValue = new MemCommand()
            {
                Action = ApplyValue
            };
           
            ViewModel.NewScanCommand = new MemCommand()
            {
                Action = async () => await ScanForValues()
            };
            ViewModel.NextScanCommand = new MemCommand()
            {
                Action = async () => await ScanForNextValues()
            };
            ViewModel.RefreshAvailableProcesses = new MemCommand()
            {
                Action = () =>
                    ViewModel.AvailableProcesses =
                        new ObservableCollection<string>(Process.GetProcesses().Select(x => x.ProcessName).ToList())
            };

            ViewModel.ScanType = new List<string>()
            {
                "Decreased Value By",
                "Increase Value By"
            };
            ViewModel.ValueType = new List<string>()
            {
                "Byte", 
                "Short",
                "UShort",
                "Int",
                "UInt",
                "Long",
                "ULong"
            };
            ViewModel.AvailableProcessesIndexSelected = -1;
            ViewModel.ScanTypeIndexSelected = 0;
            ViewModel.ValueTypeIndexSelected = 2;
            ViewModel.AvailableProcesses = new ObservableCollection<string>(Process.GetProcesses().Select(x => x.ProcessName).ToList());
        }

       

        private void ApplyValue()
        {
            var process = GetSelectedProcess();

            var selectedIndex = ViewModel.SelectedIndexFoundValue;
            if (selectedIndex < 0) return;
            if (!int.TryParse(ViewModel.NewValue, out var data)) return;
            var actualDto = ViewModel.FoundValues[selectedIndex];

            var newVal = BitConverter.GetBytes(data);
            int result = WinAPI.WriteProcessMemory(process.SafeHandle.DangerousGetHandle(),
                (IntPtr)actualDto.Address, newVal, newVal.Length, out var dataRead);

            ViewModel.Status = (int)dataRead > 0 && result > 0 ? Brushes.Green : Brushes.Red;
        }

        private Process GetSelectedProcess()
        {
            var process = Process.GetProcessesByName(ViewModel.SelectedProcess).FirstOrDefault();
            return process;
        }

        private async Task ScanForNextValues()
        {
            if (!GetValue(out var value)) return;
            var process = GetSelectedProcess();
            if(process == null) return;
            await RecheckValue();
        }

        private async Task ScanForValues()
        {
            if (!GetValue(out var value)) return;
            ViewModel.CompletedPercent = 0;
            ViewModel.CurrentAddressInfo = $"{0}/{ViewModel.MaxVal}";
            ViewModel.FoundValues = await GetValuesThatMatch(value);
            ViewModel.Value = string.Empty;

        }

        private bool GetValue(out int value)
        {
            value = 0;
            if (ViewModel.IsHexFormat)
            {
                if (!int.TryParse(ViewModel.Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value))
                {
                    return false;
                }
            }
            else
            {
                if (!int.TryParse(ViewModel.Value, out value))
                {
                    return false;
                }
            }

            return true;
        }

        private ScanType GetSelectedScanType()
        {
            if (ViewModel.ScanTypeIndexSelected == 0) return ScanType.DecreasedValueBy;
            return ScanType.IncreasedValueBy;
        }

        public override void OnViewFinished()
        {
        }

        private async Task RecheckValue()
        {
            if (!GetValue(out var value)) return;
            var process = GetSelectedProcess();
            if (process == null) return;
            var explorer = CreateNewMemoryExplorer();
            var totalAddressesToRecheck = ViewModel.FoundValues.Count;
            ViewModel.FoundValues = await explorer.ReQueryData(ViewModel.FoundValues, value, obj =>
            {
                View.ExecuteOnUIThread(() =>
                {
                    ViewModel.CompletedPercent = (long) (((float) obj / (float) totalAddressesToRecheck) / 100.0f);
                    ViewModel.CurrentAddressInfo = $"{obj}/{totalAddressesToRecheck}";
                });
            });
        }

       

        private async Task<List<MemoryDto>> GetValuesThatMatch(dynamic valToMatch)
        {
            var memoryExplorer = CreateNewMemoryExplorer();
            return await memoryExplorer.GetMemoryPositionThatMatch(valToMatch, new Action<ulong>((address) =>
            {
                View.ExecuteOnUIThread(() =>
                {
                    ViewModel.CompletedPercent = (long) (((float) address / (float)ViewModel.MaxVal) * 100.0);
                    ViewModel.CurrentAddressInfo = $"{address}/{ViewModel.MaxVal}";
                });
            }));
        }

        private MemValueType MapToValueType(int index)
        {
            if (index == 0) return MemValueType.Byte;
            if (index == 1) return MemValueType.Short;
            if (index == 2) return MemValueType.UShort;
            if (index == 3) return MemValueType.Int;
            if (index == 4) return MemValueType.UInt;
            if (index == 5) return MemValueType.Long;
            if (index == 6) return MemValueType.ULong;
            throw new ArgumentException();
        }


        public static IEnumerable<List<T>> SplitList<T>(List<T> locations, int nSize = 30)
        {
            for (int i = 0; i < locations.Count; i += nSize)
            {
                yield return locations.GetRange(i, Math.Min(nSize, locations.Count - i));
            }
        }

        private MemoryExplorer CreateNewMemoryExplorer()
        {
            return new MemoryExplorer(MapToValueType(ViewModel.ValueTypeIndexSelected),
                ViewModel.MaxVal, GetSelectedProcess());
        }
    }

}
