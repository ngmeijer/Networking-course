using shared;
using shared.src.protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace server
{
    public class DataSender
    {
        public DataSender()
        {
        }

        public void SendNewAvatar(TcpClient pClient, NewAvatar pContainer)
        {
            SendObject(pClient, pContainer);
        }

        public void SendMessage(TcpClient pClient, SimpleMessage pMessage)
        {
            SendObject(pClient, pMessage);
        }

        public void SendPositionUpdate(TcpClient pClient, PositionUpdate pObject)
        {
            SendObject(pClient, pObject);
        }
      
        public void SendObject(TcpClient pClient, ISerializable pOutObject)
        {
            try
            {
                Packet outPacket = new Packet();
                outPacket.Write(pOutObject);
                StreamUtil.Write(pClient.GetStream(), outPacket.GetBytes());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void SendHeartBeat(TcpClient pClient, HeartBeat pHeartBeat)
        {
            SendObject(pClient, new HeartBeat());
        }

        public void SendAvatarRemove(TcpClient pClient, DeadAvatar pDeadAvatar)
        {
            SendObject(pClient, pDeadAvatar);
        }

        public void SendExistingClients(TcpClient pClient, ExistingAvatars pObject)
        {
            SendObject(pClient, pObject);
        }

        public void SendSkinUpdate(TcpClient pClient, SkinUpdate pObject)
        {
            SendObject(pClient, pObject);
        }
    }
}
