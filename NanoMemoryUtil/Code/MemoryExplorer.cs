using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using NanoMemUtil.UI;

namespace NanoMemUtil.Code
{
    public class MemoryExplorer
    {
        public const int CHUNK_SIZE = 16000000;

        const int PROCESS_QUERY_INFORMATION = 0x0400;
        const int MEM_COMMIT = 0x00001000;
        const int PAGE_READWRITE = 0x04;
        const int PROCESS_WM_READ = 0x0010;

        private int _conversionIndex;
        private int _valueLength;
        private ulong _maxAddressValue;
        private Process _process;

        public MemoryExplorer(MemValueType valueType, ulong maxAddress, Process process)
        {
            _conversionIndex = GetConversionIndex(valueType);
            _valueLength = (int) valueType;
            _maxAddressValue = maxAddress;
            _process = process;
            WinAPI.OpenProcess(ProcessAccessFlags.All, false, _process.Id);
        }

        public async Task<List<MemoryDto>> ReQueryData(List<MemoryDto> data, dynamic valueToMatch, Action<ulong> callback)
        {
            var newData = new List<MemoryDto>();
            await Task.Run(() =>
            {
                var index = (ulong)0;
                data.ForEach(dto =>
                {
                    var buffer = new byte[_valueLength];
                    WinAPI.ReadProcessMemory(_process.SafeHandle.DangerousGetHandle(),
                        (IntPtr)dto.Address, buffer, buffer.Length, out var bytesRead);
                    if((int)bytesRead == _valueLength)
                    {
                        var newVal = ConvertToRightValue(buffer, _conversionIndex);
                        if (newVal == valueToMatch)
                        {
                            newData.Add(new MemoryDto()
                            {
                                Address = dto.Address,
                                PrevValue = dto.Value,
                                Value = newVal
                            });
                        }
                    }
                    callback?.Invoke(index);
                    index++;
                });
            });
            return newData;
        }

        public async  Task<List<MemoryDto>> GetMemoryPositionThatMatch(dynamic valToMatch, 
            Action<ulong> statusAction)
        {
            var list = new List<MemoryDto>();

            await Task.Run(() =>
            {
                ulong currentAddress = 0;

                while (currentAddress <= _maxAddressValue)
                {
                    var address = currentAddress;
                    statusAction?.Invoke(address);
                    WinAPI.VirtualQueryEx(_process.Handle, (uint)currentAddress, out var information, 28);
                    var regionSize = (ulong)information.RegionSize;
                    var lastAddress = currentAddress;
                    currentAddress += regionSize;
                    if (lastAddress > currentAddress) break;
                    if (information.Protect == PAGE_READWRITE && information.State == MEM_COMMIT)
                    {
                        var numberOfChunks = regionSize / CHUNK_SIZE;
                        ulong chunkSize = CHUNK_SIZE;
                        var baseAddress = (ulong) information.BaseAddress;
                        var needsRemainder = true;

                        if(regionSize < CHUNK_SIZE)
                        {
                            numberOfChunks = 1;
                            chunkSize = regionSize;
                            needsRemainder = false;
                        }

                        for (ulong chunk = 0; chunk < numberOfChunks; chunk++)
                        {
                            PerformRead(valToMatch, chunkSize, information, baseAddress, list);
                            baseAddress += chunkSize;
                        }
                        //
                        if(needsRemainder && regionSize % (ulong)CHUNK_SIZE != 0)
                        {
                            chunkSize = regionSize & CHUNK_SIZE;
                            PerformRead(valToMatch, chunkSize, information, baseAddress, list);
                        }
                    }
                }
            });

          


            return list;
        }

        private void PerformRead(dynamic valToMatch, ulong chunkSize, MEMORY_BASIC_INFORMATION information, ulong baseAddress,
            List<MemoryDto> list)
        {
            var data = new byte[chunkSize];
            WinAPI.ReadProcessMemory(_process.SafeHandle.DangerousGetHandle(), information.BaseAddress,
                data, data.Length, out var bytesRead);
            if ((int) bytesRead > 0)
            {
                var matchedValue = (List<(ulong Address, dynamic Value)>) MapToCorrectValues(data,
                    valToMatch,
                    (ulong) baseAddress, _conversionIndex);
                matchedValue.ForEach(tuple => list.Add(new MemoryDto()
                {
                    Address = tuple.Address,
                    Value = tuple.Value
                }));
            }
        }

        private List<(ulong Address, dynamic Value)> MapToCorrectValues(byte[] data, dynamic match, ulong baseAddress, 
            int conversionIndex)
        {
            var result = new List<(ulong, dynamic)>();
            var address = baseAddress;
            for (var i = 0; i < data.Length; i += _valueLength)
            {
                byte[] byteSubArray = null;
                if (_valueLength == 1) byteSubArray = new[] { data[i] };
                if (_valueLength == 2) byteSubArray = new[] { data[i], data[i + 1] };
                if (_valueLength == 4) byteSubArray = new[] { data[i], data[i + 1], data[i + 2], data[i + 3] };
                if (_valueLength == 8) byteSubArray = new[] { data[i], data[i + 1], data[i + 2], data[i + 3], data[i + 4], data[i + 5], data[i + 6], data[i + 7] };

                var newInt = ConvertToRightValue(byteSubArray.ToArray(), conversionIndex);
                if (newInt == match)
                {
                    result.Add((address, newInt));
                }
                address += (ulong)_valueLength;
            }
            return result;
        }

        private dynamic ConvertToRightValue(byte[] data, int conversionIndex)
        {
            if (conversionIndex == 0) return data[0];
            if (conversionIndex == 1) return BitConverter.ToInt16(data, 0);
            if (conversionIndex == 2) return BitConverter.ToUInt16(data, 0);
            if (conversionIndex == 3) return BitConverter.ToInt32(data, 0);
            if (conversionIndex == 4) return BitConverter.ToUInt32(data, 0);
            if (conversionIndex == 5) return BitConverter.ToInt64(data, 0);
            if (conversionIndex == 6) return BitConverter.ToUInt64(data, 0);
            throw new ArgumentException("Unknown value type selected");
        }

      

        private int GetConversionIndex(MemValueType index)
        {
            if (index == MemValueType.Byte) return 0;
            if (index == MemValueType.Short) return 1;
            if (index == MemValueType.UShort) return 2;
            if (index == MemValueType.Int) return 3;
            if (index == MemValueType.UInt) return 4;
            if (index == MemValueType.Long) return 5;
            if (index == MemValueType.ULong) return 6;
            throw new ArgumentException();
        }
    }
}
