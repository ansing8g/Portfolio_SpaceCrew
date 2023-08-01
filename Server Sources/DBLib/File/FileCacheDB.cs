using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Reflection;

using UtilityLib;

namespace DBLib.File
{
    public abstract class IFileCacheDBData<KeyType>
        where KeyType : IConvertible
    {
        protected IFileCacheDBData(KeyType _key)
        {
            Key = _key;
        }

        public KeyType Key;
    }

    public class FileCacheDB<KeyType, DataType>
        where KeyType : IConvertible
        where DataType : IFileCacheDBData<KeyType>, new()
    {
        private const char ColumnDelimiter = '|';
        private const char PartDelimiter = ',';

        private class HeaderInfo
        {
            public HeaderInfo()
            {
                m_dicHeader = new();
            }

            public void SetHeader(ConcurrentBag<(string Name, TypeCode Type)> _bagHeaderInfo)
            {
                m_dicHeader.Clear();

                for (int i = _bagHeaderInfo.Count - 1; i >= 0; i--)
                {
                    (string Name, TypeCode Type) info = _bagHeaderInfo.ToArray()[i];
                    m_dicHeader.AddOrUpdate(info.Name, ((uint)i, info.Type), (oldName, oldData) => { return ((uint)i, info.Type); });
                }
            }

            public bool ReadFileData(string _fileData)
            {
                Func<string, int, char, (bool IsComplete, int Offset, string Value)> FuncParsing = (string _fileData, int _startOffset, char _delimiter) =>
                {
                    string data = string.Empty;
                    int offset = _startOffset;
                    while (_delimiter != _fileData[offset])
                    {
                        data += _fileData[offset];
                        ++offset;

                        if (offset >= _fileData.Length)
                        {
                            return (false, 0, string.Empty);
                        }
                    }

                    return (true, offset + 1, data);
                };

                ConcurrentDictionary<string, (uint Index, TypeCode TypeCode)> dicTempHeader = new();
                (bool IsComplete, int Offset, string Value) result = (false, 0, string.Empty);
                while (result.Offset < _fileData.Length)
                {
                    result = FuncParsing(_fileData, result.Offset, PartDelimiter);
                    if (false == result.IsComplete)
                    {
                        return false;
                    }

                    uint index = uint.Parse(result.Value);

                    result = FuncParsing(_fileData, result.Offset, PartDelimiter);
                    if (false == result.IsComplete)
                    {
                        return false;
                    }

                    if(false == Enum.TryParse(result.Value, out TypeCode typecode))
                    {
                        return false;
                    }

                    result = FuncParsing(_fileData, result.Offset, ColumnDelimiter);
                    if (false == result.IsComplete)
                    {
                        return false;
                    }

                    dicTempHeader.AddOrUpdate(result.Value, (index, typecode), (string oldKey, (uint oldIndex, TypeCode oldTypeCode) oldValue) => (index, typecode));
                }

                m_dicHeader = dicTempHeader;

                return true;
            }

            public void WriteFileData(out string _fileData)
            {
                _fileData = string.Empty;

                List<(uint Index, TypeCode TypeCode, string Name)> listData = new();
                foreach (KeyValuePair<string, (uint Index, TypeCode TypeCode)> data in m_dicHeader)
                {
                    listData.Add((data.Value.Index, data.Value.TypeCode, data.Key));
                }

                foreach ((uint Index, TypeCode TypeCode, string Value) data in listData)
                {
                    _fileData += $"{data.Index}{PartDelimiter}{data.TypeCode}{PartDelimiter}{data.Value}{ColumnDelimiter}";
                }
            }

            public bool GetHeader(string _name, out (uint Index, string Name, TypeCode Type)? _header)
            {
                _header = null;

                if (false == m_dicHeader.ContainsKey(_name))
                {
                    return false;
                }

                _header = (m_dicHeader[_name].Index, _name, m_dicHeader[_name].Type);

                return true;
            }

            public ConcurrentDictionary<string, (uint Index, TypeCode Type)> GetHeaderNameKey => m_dicHeader;

            private ConcurrentDictionary<string, (uint Index, TypeCode Type)> m_dicHeader;
        }

        private class DataInfo
        {
            public DataInfo(HeaderInfo _headerinfo)
            {
                m_dicData = new();

                IEnumerator<KeyValuePair<string, (uint Index, TypeCode TypeCode)>> iter = _headerinfo.GetHeaderNameKey.GetEnumerator();
                while (iter.MoveNext())
                {
                    if (false == TypeToObject(iter.Current.Value.TypeCode, out object obj))
                    {
                        return;
                    }

                    m_dicData.AddOrUpdate(iter.Current.Value.Index, (iter.Current.Value.TypeCode, obj), (uint key, (TypeCode oldTypeCode, object oldObj) value) => { return (iter.Current.Value.TypeCode, obj); });
                }
            }

            public bool ReadFileData(string _fileData)
            {
                Func<string, int, char, (bool IsComplete, int Offset, string Value)> FuncParsing = (string _fileData, int _startOffset, char _delimiter) =>
                {
                    string data = string.Empty;
                    int offset = _startOffset;
                    while (_delimiter != _fileData[offset])
                    {
                        data += _fileData[offset];
                        ++offset;

                        if (offset >= _fileData.Length)
                        {
                            return (false, 0, string.Empty);
                        }
                    }

                    return (true, offset + 1, data);
                };

                ConcurrentDictionary<uint, (TypeCode TypeCode, object Value)> dicTempData = new();
                (bool IsComplete, int Offset, string Value) result = (false, 0, string.Empty);
                while (result.Offset < _fileData.Length)
                {
                    result = FuncParsing(_fileData, result.Offset, PartDelimiter);
                    if (false == result.IsComplete)
                    {
                        return false;
                    }

                    uint index = uint.Parse(result.Value);

                    result = FuncParsing(_fileData, result.Offset, PartDelimiter);
                    if (false == result.IsComplete)
                    {
                        return false;
                    }

                    if (false == Enum.TryParse(result.Value, out TypeCode typecode))
                    {
                        return false;
                    }

                    if (TypeCode.String != typecode)
                    {
                        result = FuncParsing(_fileData, result.Offset, ColumnDelimiter);
                        if (false == result.IsComplete)
                        {
                            return false;
                        }

                        if (false == FileDataToObject(typecode, result.Value, out object value))
                        {
                            return false;
                        }

                        dicTempData.AddOrUpdate(index, (typecode, value), (uint oldIndex, (TypeCode oldTypeCode, object oldValue) oldValue) => (typecode, value));
                    }
                    else
                    {
                        result = FuncParsing(_fileData, (int)result.Offset, PartDelimiter);
                        if (false == result.IsComplete)
                        {
                            return false;
                        }

                        if (false == int.TryParse((string)result.Value, out int length))
                        {
                            return false;
                        }

                        result = FuncParsing(_fileData, result.Offset, ColumnDelimiter);
                        if (false == result.IsComplete)
                        {
                            return false;
                        }

                        dicTempData.AddOrUpdate(index, (typecode, result.Value), (uint oldIndex, (TypeCode oldTypeCode, object oldValue) oldValue) => (typecode, result.Value));
                    }
                }

                m_dicData = dicTempData;

                return true;
            }

            public bool WriteFileData(out string _fileData)
            {
                _fileData = string.Empty;

                List<(uint Index, TypeCode TypeCode, string Value)> listData = new();
                foreach (KeyValuePair<uint, (TypeCode TypeCode, object Value)> data in m_dicData)
                {
                    string strValue = string.Empty;
                    switch (data.Value.TypeCode)
                    {
                        case TypeCode.Boolean:
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                        case TypeCode.Char: { strValue = data.Value.Value.ToString()!; } break;
                        case TypeCode.DateTime: { strValue = ((DateTime)data.Value.Value).Ticks.ToString(); } break;
                        case TypeCode.String:
                            {
                                string str = data.Value.Value.ToString() ?? "";
                                strValue = $"{str.Length}{PartDelimiter}{str}";
                            }
                            break;
                        default: { return false; }
                    }

                    listData.Add((data.Key, data.Value.TypeCode, strValue));
                }

                foreach ((uint Index, TypeCode TypeCode, string Value) data in listData)
                {
                    _fileData += $"{data.Index}{PartDelimiter}{data.TypeCode}{PartDelimiter}{data.Value}{ColumnDelimiter}";
                }

                return true;
            }

            public bool GetValue(uint _index, out (TypeCode TypeCode, object Value)? _value)
            {
                _value = null;

                if (false == m_dicData.ContainsKey(_index))
                {
                    return false;
                }

                _value = m_dicData[_index];

                return true;
            }

            public bool SetValue(uint _index, TypeCode _typecode, object _value)
            {
                if (false == m_dicData.ContainsKey(_index))
                {
                    return false;
                }

                if (_typecode != m_dicData[_index].TypeCode)
                {
                    return false;
                }

                m_dicData[_index] = (_typecode, _value);

                return true;
            }

            private bool TypeToObject(TypeCode _typecode, out object _obj)
            {
                _obj = new();
                switch (_typecode)
                {
                    case TypeCode.Boolean: { _obj = false; } break;
                    case TypeCode.Char: { _obj = ' '; } break;
                    case TypeCode.SByte: { _obj = 0; } break;
                    case TypeCode.Byte: { _obj = 0; } break;
                    case TypeCode.Int16: { _obj = 0; } break;
                    case TypeCode.UInt16: { _obj = 0; } break;
                    case TypeCode.Int32: { _obj = 0; } break;
                    case TypeCode.UInt32: { _obj = 0; } break;
                    case TypeCode.Int64: { _obj = 0; } break;
                    case TypeCode.UInt64: { _obj = 0; } break;
                    case TypeCode.Single: { _obj = 0.0f; } break;
                    case TypeCode.Double: { _obj = 0.0; } break;
                    case TypeCode.Decimal: { _obj = 0; } break;
                    case TypeCode.DateTime: { _obj = DateTime.MinValue; } break;
                    case TypeCode.String: { _obj = string.Empty; } break;
                    default: { return false; }
                }

                return true;
            }

            private bool FileDataToObject(TypeCode _typecode, string _filedata, out object _value)
            {
                _value = new();
                switch (_typecode)
                {
                    case TypeCode.Boolean: { _value = bool.Parse(_filedata); } break;
                    case TypeCode.Char: { _value = _filedata[0]; } break;
                    case TypeCode.SByte: { _value = sbyte.Parse(_filedata); } break;
                    case TypeCode.Byte: { _value = byte.Parse(_filedata); } break;
                    case TypeCode.Int16: { _value = short.Parse(_filedata); } break;
                    case TypeCode.UInt16: { _value = ushort.Parse(_filedata); } break;
                    case TypeCode.Int32: { _value = int.Parse(_filedata); } break;
                    case TypeCode.UInt32: { _value = uint.Parse(_filedata); } break;
                    case TypeCode.Int64: { _value = long.Parse(_filedata); } break;
                    case TypeCode.UInt64: { _value = ulong.Parse(_filedata); } break;
                    case TypeCode.Single: { _value = float.Parse(_filedata); } break;
                    case TypeCode.Double: { _value = double.Parse(_filedata); } break;
                    case TypeCode.Decimal: { _value = decimal.Parse(_filedata); } break;
                    case TypeCode.DateTime: { _value = new DateTime(long.Parse(_filedata)); } break;
                    case TypeCode.String: { _value = _filedata; } break;
                    default: { return false; }
                }

                return true;
            }

            private ConcurrentDictionary<uint, (TypeCode TypeCode, object Value)> m_dicData;
        }

        public static FileCacheDB<KeyType, DataType>? Create(FileCacheDBConfig _config)
        {
            if(true == string.IsNullOrEmpty(_config.FilePath))
            {
                return null;
            }

            FileCacheDB<KeyType, DataType> fdb = new(_config);
            if (false == fdb.InitializierOpen())
            {
                if (false == fdb.InitializierCreate())
                {
                    return null;
                }
            }

            return fdb;
        }

        private FileCacheDB(FileCacheDBConfig _config)
        {
            m_file = new(_config.FilePath);
            m_header = new();
            m_dicData = new();
            m_timer = new();
            m_isChange = false;

            m_timer.Regist((double)_config.WriteDelaySecond, Worker);
        }

        private bool InitializierOpen()
        {
            if (false == m_file.Exists)
            {
                return false;
            }

            if (false == ReadFile())
            {
                return false;
            }

            return true;
        }

        private bool InitializierCreate()
        {
            if (true == m_file.Exists)
            {
                return false;
            }

            ConcurrentBag<(string Name, TypeCode Type)> bagHeader = new();
            foreach (FieldInfo field in typeof(DataType).GetFields())
            {
                bagHeader.Add((field.Name, Type.GetTypeCode(field.FieldType)));
            }

            m_header.SetHeader(bagHeader);

            return true;
        }

        private bool ReadFile()
        {
            List<string> listFileData = new();
            using (FileStream fs = m_file.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (StreamReader sr = new(fs))
                {
                    while (!sr.EndOfStream)
                    {
                        string? data = sr.ReadLine();
                        if (!string.IsNullOrEmpty(data))
                        {
                            listFileData.Add(data);
                        }
                    }
                }
            }

            if (0 == listFileData.Count)
            {
                return false;
            }

            if (false == m_header.ReadFileData(listFileData[0]))
            {
                return false;
            }

            if (false == m_header.GetHeader("Key", out (uint Index, String Name, TypeCode Type)? header) ||
                null == header)
            {
                return false;
            }

            ConcurrentDictionary<KeyType, DataInfo> dicTemp = new();
            for (int i = 1; i < listFileData.Count; ++i)
            {
                DataInfo data = new(m_header);
                if (false == data.ReadFileData(listFileData[i]))
                {
                    return false;
                }

                if (false == data.GetValue(header.Value.Index, out (TypeCode TypeCode, object Value)? value) ||
                    null == value)
                {
                    return false;
                }

                dicTemp.AddOrUpdate((KeyType)value.Value.Value, data, (KeyType oldKey, DataInfo oldData) => { return data; });
            }

            m_dicData = dicTemp;

            return true;
        }

        private void WriteFile()
        {
            string? directoryPath = Path.GetDirectoryName(m_file.FullName);
            if(false == Directory.Exists(directoryPath) &&
                null != directoryPath)
            {
                DirectoryInfo directoryInfo = Directory.CreateDirectory(directoryPath);
                if(false == directoryInfo.Exists)
                {
                    return;
                }
            }

            string newFilePath = $"{directoryPath}\\{Path.GetFileNameWithoutExtension(m_file.Name)}_Temp{m_file.Extension}";

            ConcurrentDictionary<KeyType, DataInfo> dicTemp = new();
            IEnumerator<KeyValuePair<KeyType, DataInfo>> iter = m_dicData.GetEnumerator();
            while (iter.MoveNext())
            {
                dicTemp.AddOrUpdate(iter.Current.Key, iter.Current.Value, (KeyType oldKey, DataInfo oldValue) => iter.Current.Value);
            }

            FileInfo finfo = new(newFilePath);
            using (FileStream fs = finfo.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
            {
                using (StreamWriter sw = new(fs))
                {
                    m_header.WriteFileData(out string fileData);
                    sw.WriteLine(fileData);

                    iter = dicTemp.GetEnumerator();
                    while (iter.MoveNext())
                    {
                        if (false == iter.Current.Value.WriteFileData(out string data))
                        {
                            finfo.Delete();
                            return;
                        }

                        sw.WriteLine(data);
                    }
                }
            }

            finfo.MoveTo(m_file.FullName, true);
            m_file = finfo;

            dicTemp.Clear();
        }

        private void Worker()
        {
            if (false == m_isChange)
            {
                return;
            }

            m_isChange = false;

            WriteFile();
        }

        private void GetValue(DataInfo _datainfo, out DataType _data)
        {
            _data = new();
            foreach (FieldInfo field in _data.GetType().GetFields())
            {
                if (false == m_header.GetHeader(field.Name, out (uint Index, String Name, TypeCode Type)? header) ||
                    null == header)
                {
                    continue;
                }

                if (Type.GetTypeCode(field.FieldType) != header.Value.Type)
                {
                    continue;
                }

                if (false == _datainfo.GetValue(header.Value.Index, out (TypeCode TypeCode, object Value)? value) ||
                    null == value)
                {
                    continue;
                }

                field.SetValue(_data, value.Value.Value);
            }
        }

        private void SetValue(DataType _data, DataInfo _datainfo)
        {
            foreach (FieldInfo field in _data.GetType().GetFields())
            {
                object? value = field.GetValue(_data);
                if (null == value)
                {
                    continue;
                }

                if (false == m_header.GetHeader(field.Name, out (uint Index, String Name, TypeCode Type)? header) ||
                    null == header)
                {
                    continue;
                }

                if (false == _datainfo.SetValue(header.Value.Index, Type.GetTypeCode(field.FieldType), value))
                {
                    continue;
                }
            }
        }

        public bool Select(KeyType _key, out DataType? _data)
        {
            _data = null;

            if (false == m_dicData.ContainsKey(_key))
            {
                return false;
            }

            GetValue(m_dicData[_key], out _data);

            return true;
        }

        public void SelectAll(Action<DataType> _func)
        {
            IEnumerator<KeyValuePair<KeyType, DataInfo>> iter = m_dicData.GetEnumerator();
            while(iter.MoveNext())
            {
                GetValue(iter.Current.Value, out DataType data);
                _func(data);
            }
        }

        public bool Insert(DataType _data)
        {
            if (true == m_dicData.ContainsKey(_data.Key))
            {
                return false;
            }

            DataInfo datainfo = new(m_header);
            SetValue(_data, datainfo);

            if (false == m_dicData.TryAdd(_data.Key, datainfo))
            {
                return false;
            }

            m_isChange = true;

            return true;
        }

        public bool Update(DataType _data)
        {
            if (false == m_dicData.ContainsKey(_data.Key))
            {
                return false;
            }

            DataInfo datainfo = m_dicData[_data.Key];
            SetValue(_data, datainfo);

            m_isChange = true;

            return true;
        }

        public bool Delete(KeyType _key)
        {
            if (false == m_dicData.Remove(_key, out DataInfo? data))
            {
                return false;
            }

            m_isChange = true;

            return true;
        }

        private FileInfo m_file;
        private HeaderInfo m_header;
        private ConcurrentDictionary<KeyType, DataInfo> m_dicData;
        private TimerManager m_timer;
        private bool m_isChange;
    }
}
