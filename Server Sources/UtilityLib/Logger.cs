using System;
using System.IO;
using System.Text;

namespace UtilityLib
{
    public class Logger
    {
        public Logger() : this("", "", 0, 0)
        {

        }

        public Logger(string _path_directory, string _filename, uint _delay_second = 0, long _limit_filesize = 0)
        {
            if (true == string.IsNullOrEmpty(_path_directory))
            {
                _path_directory = "./Log";
            }

            if (true == string.IsNullOrEmpty(_filename))
            {
                _filename = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            }

            if (0 >= _delay_second)
            {
                _delay_second = 60 * 60;
            }

            DirectoryInfo dinfo = new DirectoryInfo(_path_directory);
            if (false == dinfo.Exists)
            {
                dinfo.Create();
            }

            lock (this)
            {
                m_path_directory = dinfo.FullName;
                m_filename = _filename;
                m_delay_second = (double)_delay_second;
                m_limit_filesize = _limit_filesize;

                //--> CreateFile(); 를 사용하면 생성자에서 초기화하지 않은 값이 있다고 warning 뜸
                m_createtime = DateTime.Now;
                m_filesize = 0;
                m_fileinfo = new FileInfo(m_path_directory + "\\" + m_filename + "_" + m_createtime.ToString("yyyy-MM-dd HH-mm-ss") + ".txt");
            }
        }

        public void WriteFile(string _log)
        {
            lock (this)
            {
                if (0 != m_limit_filesize &&
                    m_filesize >= m_limit_filesize)
                {
                    CreateFile();
                }
                else if (0 != m_delay_second &&
                    DateTime.Now >= m_createtime.AddSeconds(m_delay_second))
                {
                    CreateFile();
                }

                byte[] log_data = Encoding.Default.GetBytes($"[{DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")}]{_log}");

                FileStream fs = m_fileinfo.Open(FileMode.Append, FileAccess.Write, FileShare.Read);
                StreamWriter sw = new StreamWriter(fs);
                sw.WriteLine(Encoding.UTF8.GetString(log_data));
                sw.Close();
                fs.Close();

                m_filesize += log_data.Length;
            }
        }

        private void CreateFile()
        {
            m_createtime = DateTime.Now;
            m_filesize = 0;
            m_fileinfo = new FileInfo(m_path_directory + "\\" + m_filename + "_" + m_createtime.ToString("yyyy-MM-dd HH-mm-ss") + ".txt");
        }

        private FileInfo m_fileinfo;
        private DateTime m_createtime;
        private long m_filesize;
        private double m_delay_second;
        private long m_limit_filesize;
        private string m_path_directory;
        private string m_filename;
    }
}
