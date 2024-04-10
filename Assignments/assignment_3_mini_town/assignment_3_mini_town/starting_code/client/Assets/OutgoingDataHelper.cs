using shared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class OutgoingDataHelper : MonoBehaviour
{
    public AvatarAreaManager AreaManager;
    public TcpClient TCPClient;
    public int OwnID = -1;

    public void sendObject(ISerializable pOutObject)
    {
        try
        {
            Packet outPacket = new Packet();
            outPacket.Write(pOutObject);
            StreamUtil.Write(TCPClient.GetStream(), outPacket.GetBytes());
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    public void SendNewMessage(string pOutString)
    {
        try
        {
            Vector3 currentPosition = AreaManager.GetAvatarView(OwnID).transform.localPosition;
            SimpleMessage message = new SimpleMessage()
            {
                SenderID = OwnID,
                Text = pOutString,
                Position = new shared.src.Vector3(currentPosition.x, currentPosition.y, currentPosition.z)
            };
            sendObject(message);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            //_client.Close();
            //connectToServer();
        }
    }

    public void SendSkinUpdate()
    {
        SkinUpdate skinUpdate = new SkinUpdate()
        {
            ID = OwnID,
            SkinID = -1
        };

        sendObject(skinUpdate);
    }
}
