using System;
using System.Collections.Generic;
using System.Linq;

using Packet;

using SpaceCrewServer.Server;

namespace SpaceCrewServer.Game
{
    public class Mission_GetCard : IMission
    {
        public class MissionCardInfo
        {
            public MissionCardInfo(PacketDefine.Card _card, PacketDefine.MissionCardOrderType _order)
            {
                Card = _card;
                Order = _order;
                GetSlot = uint.MaxValue;
                Clear = false;
            }

            public PacketDefine.Card Card;
            public PacketDefine.MissionCardOrderType Order;
            public uint GetSlot;
            public bool Clear;
        }

        public Mission_GetCard(Room _room, uint _cardCount, List<PacketDefine.Card> _listDeck, uint _orderCount)
        {
            m_room = _room;
            m_listGetCardData = new List<MissionCardInfo>();
            m_listOrderNumber = new List<PacketDefine.MissionCardOrderType>();
            m_listOrderOffset = new List<PacketDefine.MissionCardOrderType>();
            m_check = Define.MissionCheckType.None;

            GenerateCardOrder(_orderCount, out List<PacketDefine.MissionCardOrderType> listOrder);
            Initialize(_cardCount, _listDeck, listOrder);
        }

        public Mission_GetCard(Room _room, uint _cardCount, List<PacketDefine.Card> _listDeck, List<PacketDefine.MissionCardOrderType> _listOrder)
        {
            m_room = _room;
            m_listGetCardData = new List<MissionCardInfo>();
            m_listOrderNumber = new List<PacketDefine.MissionCardOrderType>();
            m_listOrderOffset = new List<PacketDefine.MissionCardOrderType>();
            m_check = Define.MissionCheckType.None;

            Initialize(_cardCount, _listDeck, _listOrder);
        }

        private void Initialize(uint _cardCount, List<PacketDefine.Card> _listDeck, List<PacketDefine.MissionCardOrderType> _listOrder)
        {
            m_listOrderNumber.Add(PacketDefine.MissionCardOrderType._1);
            m_listOrderNumber.Add(PacketDefine.MissionCardOrderType._2);
            m_listOrderNumber.Add(PacketDefine.MissionCardOrderType._3);
            m_listOrderNumber.Add(PacketDefine.MissionCardOrderType._4);
            m_listOrderNumber.Add(PacketDefine.MissionCardOrderType._5);
            m_listOrderOffset.Add(PacketDefine.MissionCardOrderType.Ⅰ);
            m_listOrderOffset.Add(PacketDefine.MissionCardOrderType.Ⅱ);
            m_listOrderOffset.Add(PacketDefine.MissionCardOrderType.Ⅲ);
            m_listOrderOffset.Add(PacketDefine.MissionCardOrderType.Ⅳ);

            m_turnslot = m_room.TurnSlot;

            GenerateGetCard(_cardCount, _listDeck, _listOrder, out m_listGetCardData);
        }

        public bool UseMissionPacket()
        {
            if (0 == m_listGetCardData.Where(i => i.GetSlot == uint.MaxValue).Count())
            {
                return false;
            }

            Player? trunplayer = m_room.GetPlayerList[m_turnslot];
            if (null == trunplayer)
            {
                return false;
            }

            Packet.StoC.Mission_GetCardStart_Noti packet = new Packet.StoC.Mission_GetCardStart_Noti(PacketDefine.PacketResult.Success);
            packet.SelectUserIndex = trunplayer.UserIndex;
            foreach (Mission_GetCard.MissionCardInfo card in m_listGetCardData.Where(i => i.GetSlot == uint.MaxValue))
            {
                packet.listMissionGetCard.Add(new PacketDefine.MissionGetCardData()
                {
                    Card = card.Card,
                    Order = card.Order
                });
            }

            foreach (Player? player in m_room.GetPlayerList)
            {
                player?.User?.Send(packet);
            }

            return true;
        }

        public bool SelectCard(uint _userindex, MissionCardInfo _cardinfo)
        {
            if (_userindex != (m_room.GetPlayerList[m_turnslot]?.UserIndex ?? 0))
            {
                return false;
            }

            int index = m_listGetCardData.FindIndex(i => i.Card == _cardinfo.Card && i.Order == _cardinfo.Order);
            if (0 > index)
            {
                return false;
            }

            m_listGetCardData[index].GetSlot = m_turnslot;

            if (0 < m_listGetCardData.Where(i => i.GetSlot == uint.MaxValue).Count())
            {
                uint slot = 0 == m_turnslot ? (uint)m_room.GetPlayerList.Length - 1 : m_turnslot - 1;
                for (int i = 0; i < m_room.GetPlayerList.Length; ++i)
                {
                    if (null != m_room.GetPlayerList[slot])
                    {
                        m_turnslot = slot;
                        return true;
                    }

                    slot = 0 == slot ? (uint)m_room.GetPlayerList.Length - 1 : slot - 1;
                }
            }

            return true;
        }

        public void Trick(uint _slot, PacketDefine.Card _card) { }

        public void GetTrick(uint _slot, List<PacketDefine.Card> _listTrickCard)
        {
            if (Define.MissionCheckType.Success == m_check ||
                Define.MissionCheckType.Fail == m_check)
            {
                return;
            }

            List<MissionCardInfo> listNew = m_listGetCardData.Where(i => _listTrickCard.Any(j => j == i.Card)).OrderBy(i => i.Order).ToList();
            foreach(MissionCardInfo info in listNew)
            {
                if(info.GetSlot != _slot)
                {
                    m_check = Define.MissionCheckType.Fail;
                    return;
                }
            }

            List<MissionCardInfo> listOld = m_listGetCardData.Where(i => true == i.Clear).ToList();
            List<MissionCardInfo> listNewOffset = listNew.Where(i => i.Order == PacketDefine.MissionCardOrderType._1 || i.Order == PacketDefine.MissionCardOrderType._2 || i.Order == PacketDefine.MissionCardOrderType._3 || i.Order == PacketDefine.MissionCardOrderType._4 || i.Order == PacketDefine.MissionCardOrderType._5).OrderBy(i => i.Order).ToList();
            if (0 < listNewOffset.Count)
            {
                if (listOld.Count >= (int)listNewOffset[0].Order ||
                    (int)listNewOffset[listNewOffset.Count - 1].Order > (listOld.Count + listNew.Count))
                {
                    m_check = Define.MissionCheckType.Fail;
                }
            }

            List<MissionCardInfo> listOldOrder = m_listGetCardData.Where(i => true == i.Clear && (i.Order == PacketDefine.MissionCardOrderType.Ⅰ || i.Order == PacketDefine.MissionCardOrderType.Ⅱ || i.Order == PacketDefine.MissionCardOrderType.Ⅲ || i.Order == PacketDefine.MissionCardOrderType.Ⅳ)).ToList();
            PacketDefine.MissionCardOrderType order = 0 < listOldOrder.Count ? listOldOrder[listOldOrder.Count - 1].Order : PacketDefine.MissionCardOrderType._5;
            foreach (MissionCardInfo info in listNew)
            {
                info.Clear = true;

                if (info.Order < order)
                {
                    order = info.Order == order - 1 ? ++order : order;
                }
                else
                {
                    m_check = Define.MissionCheckType.Fail;
                }
            }

            if (0 >= m_listGetCardData.Where(i => false == i.Clear).Count())
            {
                m_check = Define.MissionCheckType.Success;
            }
        }

        public bool CommunicationToken(uint _trick, uint _slot, PacketDefine.Card _card, PacketDefine.CommunicationTokenType _type) { return true; }
        public bool CommunicationTokenUseOnlyNone(uint _slot, PacketDefine.Card _card) { return false; }

        public Define.MissionCheckType StageEndCheck()
        {
            if(Define.MissionCheckType.Success == m_check ||
                Define.MissionCheckType.Fail == m_check)
            {
                return m_check;
            }

            for(uint slot = 0; slot < m_room.GetPlayerList.Length; ++slot)
            {
                Player? player = m_room.GetPlayerList[slot];
                if(null == player)
                {
                    continue;
                }

                List<MissionCardInfo> listMission = m_listGetCardData.Where(i => i.GetSlot == slot && i.Clear == false).ToList();
                if(0 < listMission.Count &&
                    0 == player.ListCard.Count)
                {
                    return Define.MissionCheckType.Fail;
                }
            }

            return Define.MissionCheckType.None;
        }

        private void GenerateGetCard(uint _count, List<PacketDefine.Card> _listDeck, List<PacketDefine.MissionCardOrderType> _listOrder, out List<MissionCardInfo> _listGetCardData)
        {
            _listGetCardData = new List<MissionCardInfo>();

            if (0 >= _count)
            {
                return;
            }

            List<PacketDefine.Card> listShuffleCard = _listDeck.Where(i => ((uint)i >> 8) != (uint)PacketDefine.CardType.Rocket).OrderBy(i => Guid.NewGuid()).ToList();
            List<PacketDefine.MissionCardOrderType> listOrder = _listOrder.ToList();

            for (int i = listOrder.Count; i < _count; i++)
            {
                listOrder.Add(PacketDefine.MissionCardOrderType.None);
            }

            List<PacketDefine.MissionCardOrderType> listShuffleOrder = listOrder.OrderBy(i => Guid.NewGuid()).ToList();

            for (int i = 0; i < _count; ++i)
            {
                _listGetCardData.Add(new MissionCardInfo(listShuffleCard[i], listShuffleOrder[i]));
            }
        }

        private void GenerateCardOrder(uint _count, out List<PacketDefine.MissionCardOrderType> _listOrder)
        {
            _listOrder = new List<PacketDefine.MissionCardOrderType>();

            Random rand = new Random(DateTime.Now.Millisecond);
            List<PacketDefine.MissionCardOrderType> listNumber = m_listOrderNumber.OrderBy(i => Guid.NewGuid()).ToList();
            List<PacketDefine.MissionCardOrderType> listOffset = m_listOrderOffset.OrderBy(i => Guid.NewGuid()).ToList();
            bool useOrderTypeLast = false;

            for (uint i = 0; i < _count; ++i)
            {
                int random = rand.Next(listNumber.Count + listOffset.Count + (false == useOrderTypeLast ? 1 : 0));

                if (random < listNumber.Count)
                {
                    _listOrder.Add(listNumber[0]);
                    listNumber.RemoveAt(0);
                }
                else if (random < listOffset.Count)
                {
                    _listOrder.Add(listOffset[0]);
                    listOffset.RemoveAt(0);
                }
                else
                {
                    _listOrder.Add(PacketDefine.MissionCardOrderType.Ω);
                    useOrderTypeLast = true;
                }
            }
        }

        public PacketDefine.MissionType MissionType => PacketDefine.MissionType.GetCard;

        public List<MissionCardInfo> GetNotSelectedMissinoCardList => m_listGetCardData.Where(i => i.GetSlot == uint.MaxValue).ToList();

        private Room m_room;
        private uint m_turnslot;
        private List<MissionCardInfo> m_listGetCardData;
        private List<PacketDefine.MissionCardOrderType> m_listOrderNumber;
        private List<PacketDefine.MissionCardOrderType> m_listOrderOffset;
        private Define.MissionCheckType m_check;
    }
}
