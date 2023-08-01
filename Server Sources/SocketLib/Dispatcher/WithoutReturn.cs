using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace SocketLib.Dispatcher.WithoutReturn
{
    public interface FunctionBase<Param1, param2, PacketIndex>
    {
        public void ExecuteFunction(Param1 _param1, param2 _param2, PacketBase<PacketIndex> _packetbase);
    }

    public class Dispatcher<Param1, Param2, PacketIndex> where PacketIndex : notnull
    {
        private delegate void FuncTemplate(Param1 _param1, Param2 _param2, PacketBase<PacketIndex> _packetbase);

        private class Info
        {
            public Info()
            {
                Func = null;
                PacketType = null;
            }

            public FunctionBase<Param1, Param2, PacketIndex>? Func;
            public Type? PacketType;
        }

        private class FunctionPointer<PacketObject> : FunctionBase<Param1, Param2, PacketIndex> where PacketObject : PacketBase<PacketIndex>
        {
            public FunctionPointer(Action<Param1, Param2, PacketObject> _func)
                : base()
            {
                m_func = _func;
            }

            public void ExecuteFunction(Param1 _param1, Param2 _param2, PacketBase<PacketIndex> _packet_base)
            {
                PacketObject obj;

                try
                {
                    obj = (PacketObject)_packet_base;
                }
                catch
                {
                    return;
                }

                m_func(_param1, _param2, obj);
            }

            public Action<Param1, Param2, PacketObject> m_func;
        }

        public Dispatcher()
        {
            m_func = new Dictionary<PacketIndex, Info>();
            m_lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        }

        public void RegistFunction<PacketObject>(PacketIndex _packet_index, Action<Param1, Param2, PacketObject> _func) where PacketObject : PacketBase<PacketIndex>
        {
            m_lock.EnterWriteLock();

            if (false == m_func.ContainsKey(_packet_index))
            {
                m_func.Add(_packet_index, new Info() { Func = new FunctionPointer<PacketObject>(_func), PacketType = typeof(PacketObject) });
            }
            else
            {
                m_func[_packet_index].Func = new FunctionPointer<PacketObject>(_func);
                m_func[_packet_index].PacketType = typeof(PacketObject);
            }

            m_lock.ExitWriteLock();
        }

        public void RegistClass(object logic)
        {
            RegistFunction(logic.GetType(), logic, false);
        }

        public void RegistStaticFunction(Type? classtype)
        {
            RegistFunction(classtype, null, true);
        }

        public void Clear()
        {
            m_lock.EnterWriteLock();
            m_func.Clear();
            m_lock.ExitWriteLock();
        }

        public bool GetFunction(PacketIndex _packet_index, out FunctionBase<Param1, Param2, PacketIndex>? _func, out Type? _packet_type)
        {
            _func = null;
            _packet_type = null;

            bool result = false;

            m_lock.EnterReadLock();

            if (true == m_func.ContainsKey(_packet_index))
            {
                _func = m_func[_packet_index].Func;
                _packet_type = m_func[_packet_index].PacketType;
                result = true;
            }

            m_lock.ExitReadLock();

            return result;
        }

        private void RegistFunction(Type? _classtype, object? _object, bool _isstatic)
        {
            if (null == _classtype ||
                false == _classtype.IsClass ||
               0 >= _classtype.GetMethods().Length)
            {
                return;
            }

            Type functemplatetype = typeof(FuncTemplate);
            MethodInfo? functemplatemethodinfo = functemplatetype.GetMethod("Invoke");
            if (null == functemplatemethodinfo)
            {
                return;
            }

            ParameterInfo[] functemplateparams = functemplatemethodinfo.GetParameters();

            foreach (MethodInfo methodinfo in _classtype!.GetMethods())
            {
                if (_isstatic ^ methodinfo.IsStatic)
                {
                    continue;
                }

                if (false == functemplatemethodinfo.ReturnParameter.GetType().Equals(methodinfo.ReturnParameter.GetType()))
                {
                    continue;
                }

                ParameterInfo[] parameters = methodinfo.GetParameters();
                if (functemplateparams.Length != parameters.Length)
                {
                    continue;
                }

                int matchcount = 0;
                for (int i = 0; i < functemplateparams.Length - 1; ++i)
                {
                    if (true == functemplateparams[i].ParameterType.Equals(parameters[i].ParameterType))
                    {
                        ++matchcount;
                    }
                }

                if ((functemplateparams.Length - 1) != matchcount)
                {
                    continue;
                }

                if (false == parameters[parameters.Length - 1].ParameterType.IsSubclassOf(functemplateparams[functemplateparams.Length - 1].ParameterType))
                {
                    continue;
                }

                object? packet = Activator.CreateInstance(parameters[parameters.Length - 1].ParameterType);
                if (null == packet)
                {
                    continue;
                }

                Func<Type[], Type> funcType;
                List<Type> paramTypes = new List<Type>();
                foreach (ParameterInfo paraminfo in methodinfo.GetParameters())
                {
                    paramTypes.Add(paraminfo.ParameterType);
                }

                if (true == methodinfo.ReturnType.Equals(typeof(void)))
                {
                    funcType = Expression.GetActionType;
                }
                else
                {
                    funcType = Expression.GetFuncType;
                    paramTypes.Add(methodinfo.ReturnType);
                }

                PacketBase<PacketIndex> packetBase = (PacketBase<PacketIndex>)packet;
                Delegate funcHandle = methodinfo.CreateDelegate(funcType(paramTypes.ToArray()), _object);
                Type thisType = GetType();
                MethodInfo? registfunc = thisType.GetMethod("RegistFunction");
                MethodInfo? genericfunc = registfunc?.MakeGenericMethod(parameters[parameters.Length - 1].ParameterType);
                genericfunc?.Invoke(this, new object[] { packetBase.PacketIndex, funcHandle });
            }
        }

        private Dictionary<PacketIndex, Info> m_func;
        private ReaderWriterLockSlim m_lock;
    }
}
