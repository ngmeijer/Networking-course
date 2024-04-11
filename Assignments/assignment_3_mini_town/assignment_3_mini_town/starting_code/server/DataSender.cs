﻿using shared;
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

        public void SendPacket(TcpClient pClient, Packet pPacket)
        {
            try
            {
                StreamUtil.Write(pClient.GetStream(), pPacket.GetBytes());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
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

        public void SendHeartBeat(TcpClient pClient, HeartBeat pHeartBeat)
        {
            SendObject(pClient, new HeartBeat());
        }

        public void SendAvatarRemove(TcpClient pClient, DeadAvatar pDeadAvatar)
        {
            SendObject(pClient, pDeadAvatar);
        }

        public void SendExistingClients(TcpClient[] pClients, ExistingAvatars pObject)
        {
            Packet outPacket = new Packet();
            outPacket.Write(pObject);
            for (int i = 0; i < pClients.Length; i++)
            {
                try
                {
                    StreamUtil.Write(pClients[i].GetStream(), outPacket.GetBytes());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        public void SendSkinUpdate(TcpClient pClient, SkinUpdate pObject)
        {
            SendObject(pClient, pObject);
        }
    }
}