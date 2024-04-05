using shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace server
{
    public class RequestHandler
    {
        public RequestHandler()
        {
        }

        public AvatarContainer HandleNewAvatarCreation(TcpClient pClient, int pID)
        {
            AvatarContainer newAvatar = new AvatarContainer()
            {
                ID = pID,
            };
            SendObject(pClient, newAvatar);

            return newAvatar;
        }

        public void HandleMessageRequest(TcpClient pClient, int pID, string pMessage)
        {
            SimpleMessage message = new SimpleMessage()
            {
                SenderID = pID,
                Text = pMessage
            };

            SendObject(pClient, message);
        }

        public void HandleSkinRequest(TcpClient pClient, int pID)
        {
            SkinRequest request = new SkinRequest()
            {
                ID = pID,
                SkinID = -1,
            };

            SendObject(pClient, request);
        }

        public void HandlePositionRequest(TcpClient pClient, int pID)
        {
            PositionRequest request = new PositionRequest()
            {
                ID = pID,
                Position = new float[3]
                {
                    1, 1, 1,
                },
            };
            SendObject(pClient, request);
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
    }
}
