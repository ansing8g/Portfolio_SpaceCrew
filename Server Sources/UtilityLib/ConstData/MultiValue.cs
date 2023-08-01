using System.Collections.Generic;
using System.IO;

namespace UtilityLib.ConstData.MultiValue
{
    public class ConstDataManager<Key, DataType>
        where Key : struct
        where DataType : class
    {
        public delegate bool DataParser(string _strData, out Key _key, out DataType? _dataType);

        public ConstDataManager()
        {
            m_dicData = new Dictionary<Key, List<DataType?>>();
        }

        public bool Load_File(string _pathCSVFile, DataParser _funcParser, bool _parsingHeaderPass = true)
        {
            try
            {
                List<string> listData = new List<string>();
                FileInfo finfo = new FileInfo(_pathCSVFile);
                if (false == finfo.Exists)
                {
                    return false;
                }

                FileStream? fs = null;
                StreamReader? sr = null;
                try
                {
                    fs = finfo.OpenRead();
                    sr = new StreamReader(fs);

                    bool isPass = _parsingHeaderPass;
                    while (!sr.EndOfStream)
                    {
                        string? strData = sr.ReadLine()?.Trim(new char[] { '\r', '\n' });
                        if (string.IsNullOrEmpty(strData))
                        {
                            continue;
                        }

                        if (true == isPass)
                        {
                            isPass = false;
                            continue;
                        }

                        listData.Add(strData);
                    }

                    sr.Close();
                    fs.Close();
                }
                catch
                {
                    sr?.Close();
                    fs?.Close();

                    return false;
                }

                foreach (string strData in listData)
                {
                    if (false == _funcParser(strData, out Key key, out DataType? dataType))
                    {
                        return false;
                    }

                    if (false == m_dicData.ContainsKey(key))
                    {
                        m_dicData.Add(key, new List<DataType?>());
                    }

                    m_dicData[key].Add(dataType);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        public bool GetValue(Key _key, out List<DataType?>? _value)
        {
            _value = null;

            if (false == m_dicData.ContainsKey(_key))
            {
                return false;
            }

            _value = m_dicData[_key];
            return true;
        }

        private Dictionary<Key, List<DataType?>> m_dicData;
    }
}
