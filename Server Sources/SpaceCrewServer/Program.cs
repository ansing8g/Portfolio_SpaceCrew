using System;
using System.Threading;

using SpaceCrewServer.Server;

namespace SpaceCrewServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                if (1 == args.Length &&
                    false == string.IsNullOrEmpty(args[0]))
                {
                    ServerBase.Instance.ConfigFile = args[0];
                }
                else
                {
                    string? strInput = null;
                    while(string.IsNullOrEmpty(strInput))
                    {
                        Console.Write("Input Server ConfigFile Path:");
                        strInput = Console.ReadLine();
                        Console.Clear();
                    }

                    ServerBase.Instance.ConfigFile = strInput;
                }
            }
            catch(Exception e)
            {
                ServerBase.Instance.WriteLog($"Config Input Error. File:{ServerBase.Instance.ConfigFile}, argsCount:{args.Length}, args[0]:{(0 < args.Length ? args[0] : "")}, Error:{e.Message}");
                return;
            }

            if (false == ServerBase.Instance.Initialize())
            {
                Console.WriteLine($"Server Initialize Fail. ConfigFile:{ServerBase.Instance.ConfigFile}");
                return;
            }

            if (false == ServerBase.Instance.Start())
            {
                Console.WriteLine($"Server Start Fail. ConfigPath:{ServerBase.Instance.ConfigFile} Port:{ServerBase.Instance.Config.Port}");
                return;
            }

            Console.WriteLine($"Server Start!!!\r\nPort:{ServerBase.Instance.Config.Port}");
            while (true)
            {
                Thread.Sleep(1000);
            }

            ServerBase.Instance.Release();
        }
    }
}
