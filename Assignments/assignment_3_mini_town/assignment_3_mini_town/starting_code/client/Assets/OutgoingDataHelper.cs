using shared;
using shared.src;
using shared.src.protocol;
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
            SimpleMessage message = new SimpleMessage()
            {
                SenderID = OwnID,
                Text = pOutString,
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
    public void SendWhisperMessage(string pOutString)
    {
        try
        {
            UnityEngine.Vector3 position = AreaManager.GetAvatarView(OwnID).transform.localPosition;
            WhisperMessage message = new WhisperMessage()
            {
                SenderID = OwnID,
                Text = pOutString,
                Position = new shared.src.Vector3(position.x, position.y, position.z)
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
