using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;

using SocketLib;
using SocketLib.Tcp;
using SocketLib.Dispatcher.WithoutReturn;

using UtilityLib;

using Packet;

using SpaceCrewServer.Manager;
using SpaceCrewServer.Game;
using SpaceCrewServer.Logic;
using SpaceCrewServer.DB;

namespace SpaceCrewServer.Server
{
    public class ServerBase : TcpServerSocketEvent
    {
        private static ServerBase? m_instance = null;
        public static ServerBase Instance
        {
            get
            {
                m_instance = m_instance ?? new ServerBase();
                return m_instance;
            }
            private set { }
        }

        private ServerBase()
        {
            m_currentPath = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName) ?? "";

            Serializer = new JsonSerializer();
            LobbyDispatcher = new Dispatcher<User, Lobby, PacketDefine.PacketIndex>();
            RoomDispatcher = new Dispatcher<User, Room, PacketDefine.PacketIndex>();
            ConfigFile = "";
            Config = new Define.ConfigData();

            m_logger = new Logger($"{m_currentPath}\\Log", "");
            m_lobbyWorker = new Worker();
            m_dicRoomWorker = new ConcurrentDictionary<uint, Worker>();
            m_acceptsocket = new AcceptSocket(this);
            m_listSingletonType = new List<Type>();
        }

        public bool Initialize()
        {
            if(false == LoadConfig($"{m_currentPath}\\{ConfigFile}"))
            {
                ServerBase.Instance.WriteLog($"Server Initialize LoadConfig Fail. ConfigPath:{m_currentPath}\\{ConfigFile}");
                return false;
            }

            m_lobbyWorker.RunStart();
            for (uint i = 0; i < Environment.ProcessorCount; ++i)
            {
                Worker worker = new Worker();
                m_dicRoomWorker.TryAdd(i, worker);
                worker.RunStart();
            }

            LobbyDispatcher.Clear();
            LobbyDispatcher.RegistStaticFunction(typeof(PacketLobby));

            RoomDispatcher.Clear();
            RoomDispatcher.RegistStaticFunction(typeof(PacketRoom));
            RoomDispatcher.RegistStaticFunction(typeof(PacketMissionGetCard));

            m_lobbyWorker.AddRoom(RoomManager.Instance.Lobby);

            if (false == AuthencationDB.Instance.Initialize())
            {
                ServerBase.Instance.WriteLog($"Server Initialize AuthencationDB Fail.");
                return false;
            }

            if (false == MissionTableManager.Instance.Initialize())
            {
                ServerBase.Instance.WriteLog($"Server Initialize MissionTableManager Fail.");
                return false;
            }

            if (false == RoomManager.Instance.Initialize())
            {
                ServerBase.Instance.WriteLog($"Server Initialize RoomManager Fail.");
                return false;
            }

            return true;
        }

        public void Release()
        {
            AuthencationDB.Instance.Release();
            MissionTableManager.Instance.Release();
            RoomManager.Instance.Release();

            m_listSingletonType.Clear();
            m_acceptsocket.Disconnect();
            foreach(KeyValuePair<uint, Worker> roomWorker in m_dicRoomWorker)
            {
                roomWorker.Value.RunEnd();
            }
            m_dicRoomWorker.Clear();
            m_lobbyWorker.RunEnd();

            LobbyDispatcher.Clear();
            RoomDispatcher.Clear();
        }

        public void RegistSingletonType(Type _type)
        {
            m_listSingletonType.Add(_type);
        }

        public bool Start()
        {
            if (false == m_acceptsocket.Start(Config.Port))
            {
                return false;
            }

            return true;
        }

        public void WriteLog(string _log)
        {
            m_logger.WriteFile(_log);
        }

        public Worker GetWorkerFromRoomIndex(uint _roomindex)
        {
            uint workerindex = _roomindex % (uint)m_dicRoomWorker.Count;
            return m_dicRoomWorker.GetOrAdd(workerindex, (index) => { return new Worker(); });
        }

        public void OnError(SocketDefine.SocketErrorType _error_type, System.Exception _exception, SessionSocket? _sessionsocket)
        {
            Instance.WriteLog($"Socket OnError ErrorType:{_error_type}, Message:{_exception.Message}");
        }

        public void OnAccept(SessionSocket _sessionsocket)
        {
            User user = new User(_sessionsocket, Serializer);
            _sessionsocket.StateObject = user;

            RoomManager.Instance.Lobby.AddUser(user);

            ServerBase.Instance.WriteLog($"OnAccept");
        }

        public void OnDisconnect(SessionSocket _sessionsocket)
        {
            User? user = _sessionsocket.StateObject as User;
            if(null == user)
            {
                return;
            }

            ServerBase.Instance.WriteLog($"OnDisconnect UserIndex:{user.UserIndex}");

            if (null == user.Room)
            {
                return;
            }
            
            user.Room.DisconnectQueue.Enqueue(user);
        }

        public void OnSend(SessionSocket _sessionsocket) { }

        public void OnReceive(SessionSocket _sessionsocket, byte[] _data)
        {
            User? user = _sessionsocket.StateObject as User;
            if(null == user)
            {
                return;
            }

            user.Room?.PacketQueue.Enqueue(new Define.PacketData(user, _data));
            user.SetKeepAliveTime(DateTime.Now);
        }

        private bool LoadConfig(string _pathConfigJson)
        {
            FileInfo finfo = new FileInfo(_pathConfigJson);
            if(false == finfo.Exists)
            {
                ServerBase.Instance.WriteLog($"Server Initialize LoadConfig Fail. ConfigPath:{finfo.FullName}");
                return false;
            }

            string strJson = "";
            FileStream? fs = null;
            StreamReader? sr = null;
            try
            {
                fs = finfo.OpenRead();
                sr = new StreamReader(fs);
                strJson = sr.ReadToEnd();
                sr.Close();
                fs.Close();
            }
            catch
            {
                sr?.Close();
                fs?.Close();

                return false;
            }

            if(string.IsNullOrEmpty(strJson))
            {
                ServerBase.Instance.WriteLog($"Server Initialize LoadConfig Data is Null. ConfigPath:{finfo.FullName}");
                return false;
            }

            Define.ConfigData? config = Newtonsoft.Json.JsonConvert.DeserializeObject<Define.ConfigData>(strJson);
            if(null == config)
            {
                ServerBase.Instance.WriteLog($"Server Initialize LoadConfig Deserialize Fail. ConfigPath:{finfo.FullName}");
                return false;
            }

            config.MissionCSVFilePath = $"{ServerBase.Instance.CurrentPath}\\{config.MissionCSVFilePath}";
            config.FileDBConfig.FilePath = $"{ServerBase.Instance.CurrentPath}\\{config.FileDBConfig.FilePath}";

            Config = config;

            return true;
        }

        public ISerializer Serializer { get; private set; }
        public Dispatcher<User, Lobby, PacketDefine.PacketIndex> LobbyDispatcher { get; private set; }
        public Dispatcher<User, Room, PacketDefine.PacketIndex> RoomDispatcher { get; private set; }
        public string CurrentPath => m_currentPath;
        public string ConfigFile { get; set; }
        public Define.ConfigData Config { get; private set; }

        private Logger m_logger;
        private Worker m_lobbyWorker;
        private ConcurrentDictionary<uint, Worker> m_dicRoomWorker;
        private AcceptSocket m_acceptsocket;
        private List<Type> m_listSingletonType;
        private volatile string m_currentPath;
    }
}
