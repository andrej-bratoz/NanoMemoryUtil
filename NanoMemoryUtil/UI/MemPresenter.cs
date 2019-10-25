using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using WarInTheNorthTrainer.Code;
using WarInTheNorthTrainer.Code.MVVM;

namespace WarInTheNorthTrainer.UI
{
    public class AddressComparer : IEqualityComparer<MemoryDTO>
    {
        public bool Equals(MemoryDTO x, MemoryDTO y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;
            return x.Address == y.Address;
        }

        public int GetHashCode(MemoryDTO obj)
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

        public enum ValueType
        {
            Byte,
            Short,
            Int,
            Long
        }

        const int PROCESS_QUERY_INFORMATION = 0x0400;
        const int MEM_COMMIT = 0x00001000;
        const int PAGE_READWRITE = 0x04;
        const int PROCESS_WM_READ = 0x0010;

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
                Action = ScanForNextValues
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
                "Int",
                "Long"
            };
            ViewModel.AvailableProcessesIndexSelected = -1;
            ViewModel.ScanTypeIndexSelected = 0;
            ViewModel.ValueTypeIndexSelected = 2;
            ViewModel.AvailableProcesses = new ObservableCollection<string>(Process.GetProcesses().Select(x => x.ProcessName).ToList());
        }

        private void ApplyValue()
        {
            var process = Process.GetProcessesByName(ViewModel.SelectedProcess).FirstOrDefault();
            if (process == null) return;

            var selectedIndex = ViewModel.SelectedIndexFoundValue;
            if (selectedIndex < 0) return;
            if (!int.TryParse(ViewModel.NewValue, out var data)) return;
            var actualDto = ViewModel.FoundValues[selectedIndex];

            var newVal = BitConverter.GetBytes(data);
            WinAPI.WriteProcessMemory(process.SafeHandle.DangerousGetHandle(),
                (IntPtr)actualDto.Address, newVal, newVal.Length, out var dataRead);

            ViewModel.Status = (int)dataRead > 0 ? Brushes.Green : Brushes.Red;
        }

        private async void ScanForNextValues()
        {
            y++;
            if (!GetValue(out var value)) return;
            var process = Process.GetProcessesByName(ViewModel.SelectedProcess).FirstOrDefault();
            if (process == null) return;
            var oldValues = ViewModel.FoundValues.Select(x => new MemoryDTO()
            {
                Address = x.Address,
                Value = x.Value,
                PrevValue = x.PrevValue
            }).ToList();
            await ScanForValues();

            ViewModel.FoundValues =
                ViewModel.FoundValues.Where(x => oldValues.Any(z => z.Address == x.Address)).ToList();

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

        private async Task<List<MemoryDTO>> GetValuesThatMatch(int valToMatch)
        {
            var list = new List<MemoryDTO>();

            await Task.Run(() =>
            {
                var process = Process.GetProcessesByName(ViewModel.SelectedProcess).FirstOrDefault();
                if (process == null) return;
                WinAPI.OpenProcess(ProcessAccessFlags.VMWrite, false, process.Id);
                ulong currentAddress = 0;

                while (currentAddress <= 0x7FFFFFFF)
                {
                    var address = currentAddress;
                    View.ExecuteOnUIThread(() =>
                    {
                        ViewModel.CompletedPercent = (uint)address;
                        ViewModel.CurrentAddressInfo = $"{address}/{0x7FFFFFFF}";
                    });
                    WinAPI.VirtualQueryEx(process.Handle, (uint) currentAddress, out var information, 28);
                    var regionSize = (ulong) information.RegionSize;
                    var lastAddress = currentAddress;
                    currentAddress += regionSize;
                    if (lastAddress > currentAddress) break;
                    if (information.Protect == PAGE_READWRITE && information.State == MEM_COMMIT)
                    {
                        var numberOfChunks = regionSize / ChunkSize;
                        var chunkSize = ChunkSize;
                        var baseAddress = (ulong)information.BaseAddress;
                        var needsRemainder = true;

                        if (regionSize < chunkSize)
                        {
                            numberOfChunks = 1;
                            chunkSize = regionSize;
                            needsRemainder = false;
                        }

                        for (ulong chunk = 0; chunk < numberOfChunks; chunk++)
                        {
                            var data = new byte[chunkSize];
                            WinAPI.ReadProcessMemory(process.SafeHandle.DangerousGetHandle(),
                                (IntPtr) baseAddress, data, data.Length, out var bytesRead);
                            if ((int) bytesRead > 0)
                            {
                                var matchedValue = AsFilteredInArray(data, valToMatch, (int) information.BaseAddress);
                                matchedValue.ForEach(tuple => list.Add(new MemoryDTO()
                                {
                                    Address = tuple.Address,
                                    Value = tuple.Value
                                }));
                            }

                            baseAddress += chunkSize;
                        }

                        //DON'T DUPLICATE CODE!!

                        if (needsRemainder && regionSize % ChunkSize != 0)
                        {
                            chunkSize = regionSize % ChunkSize;
                            var data = new byte[chunkSize];
                            WinAPI.ReadProcessMemory(process.SafeHandle.DangerousGetHandle(), (IntPtr) baseAddress,
                                data, data.Length, out var bytesRead);
                            if ((int) bytesRead > 0)
                            {
                                var matchedValue = AsFilteredInArray(data, valToMatch, (int) information.BaseAddress);
                                matchedValue.ForEach(tuple => list.Add(new MemoryDTO()
                                {
                                    Address = tuple.Address,
                                    Value = tuple.Value
                                }));
                            }
                        }
                    }
                    GC.Collect();
                }

                int i = 0;
            });


            return list;
        }

        private List<(int Address, int Value)> AsFilteredInArray(byte[] data, int match, int baseAddress)
        {
            var result = new List<(int, int)>();
            var address = baseAddress;
            for (var i = 0; i < data.Length; i+= sizeof(int))
            {
                var byteSubArray = new byte[] {data[i], data[i + 1], data[i + 2], data[i + 3]};
                var newInt = BitConverter.ToInt32(byteSubArray,0);
                if (newInt == match)
                {
                    result.Add((address,newInt));
                }
                address += sizeof(int);
            }
            return result;
        }

        public static IEnumerable<List<T>> SplitList<T>(List<T> locations, int nSize = 30)
        {
            for (int i = 0; i < locations.Count; i += nSize)
            {
                yield return locations.GetRange(i, Math.Min(nSize, locations.Count - i));
            }
        }
    }

}
