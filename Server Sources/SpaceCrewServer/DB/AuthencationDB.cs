using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

using DBLib.File;

using SpaceCrewServer.Server;

namespace SpaceCrewServer.DB
{
    public class AuthencationTable : IFileCacheDBData<string>
    {
        public AuthencationTable()
            : base(string.Empty)
        {
            Index = 0;
        }

        public uint Index;
    }

    public class AuthencationDB : Utility.Singleton<AuthencationDB>
    {
        private const uint UserIndexPoolCount = 10000;

        public AuthencationDB()
        {
            m_db = null;
            m_indexPool = new();
        }

        public override bool Initialize()
        {
            m_db = FileCacheDB<string, AuthencationTable>.Create(ServerBase.Instance.Config.FileDBConfig);
            if(null == m_db)
            {
                return false;
            }

            HashSet<uint> hsIndex = new();
            Action<AuthencationTable> FuncSelectAll = (AuthencationTable _table) =>
            {
                if(false == hsIndex.Contains(_table.Index))
                {
                    hsIndex.Add(_table.Index);
                }
            };
            m_db.SelectAll(FuncSelectAll);

            for(uint i = 1; i <= UserIndexPoolCount; ++i)
            {
                if(hsIndex.Contains(i))
                {
                    continue;
                }

                m_indexPool.Enqueue(i);
            }

            return true;
        }

        public override void Release() { }

        public bool GetIndex(string _id, out uint _index)
        {
            _index = 0;

            if (null == m_db)
            {
                return false;
            }

            AuthencationTable? table = null;
            if (true == m_db.Select(_id, out table))
            {
                if(null == table)
                {
                    return false;
                }

                _index = table.Index;
                return true;
            }

            AuthencationTable newTable = new();
            newTable.Key = _id;

            if (false == m_indexPool.TryDequeue(out newTable.Index))
            {
                return false;
            }

            if (false == m_db.Insert(newTable))
            {
                return false;
            }

            _index = newTable.Index;

            return true;
        }

        private FileCacheDB<string, AuthencationTable>? m_db;
        private ConcurrentQueue<uint> m_indexPool;
    }
}
