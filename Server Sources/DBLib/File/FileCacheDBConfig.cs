
namespace DBLib.File
{
    public class FileCacheDBConfig
    {
        public FileCacheDBConfig(uint _writeDelaySecond = 10)
        {
            FilePath = string.Empty;
            WriteDelaySecond = _writeDelaySecond;
        }

        public FileCacheDBConfig(string _filepath, uint _writeDelaySecond = 10)
        {
            FilePath = _filepath;
            WriteDelaySecond = _writeDelaySecond;
        }

        public string FilePath;
        public uint WriteDelaySecond;
    }
}
