
namespace SocketLib
{
    public class PacketBase<PacketIndexType>
    {
        public PacketBase()
        {
            PacketIndex = default(PacketIndexType)!;
        }

        public PacketBase(PacketIndexType index)
        {
            PacketIndex = index;
        }

        public PacketIndexType PacketIndex;
    }
}
