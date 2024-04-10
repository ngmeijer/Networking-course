using shared;
using shared.src;
using shared.src.protocol;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Xml.Linq;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

/**
 * The main ChatLobbyClient where you will have to do most of your work.
 * 
 * @author J.C. Wichman
 */
public class ChatLobbyClient : MonoBehaviour
{
    //reference to the helper class that hides all the avatar management behind a blackbox
    private AvatarAreaManager _areaManager;
    //reference to the helper class that wraps the chat interface
    private PanelWrapper _panelWrapper;

    private IncomingDataHelper _incomingDataHelper;
    private OutgoingDataHelper _outgoingDataHelper;

    [SerializeField] private string _server = "localhost";
    [SerializeField] private int _port = 55555;

    private TcpClient _client;

    private void Start()
    {
        connectToServer();

        //register for the important events
        _areaManager = FindObjectOfType<AvatarAreaManager>();
        _areaManager.OnAvatarAreaClicked += onAvatarAreaClicked;

        _panelWrapper = FindObjectOfType<PanelWrapper>();
        _panelWrapper.OnChatTextEntered += onChatTextEntered;

        _incomingDataHelper = GetComponent<IncomingDataHelper>();
        _incomingDataHelper.AreaManager = _areaManager;

        _outgoingDataHelper = GetComponent<OutgoingDataHelper>();
        _outgoingDataHelper.AreaManager = _areaManager;
        _outgoingDataHelper.TCPClient = _client;

        _incomingDataHelper.OnDataGoingOut += _outgoingDataHelper.sendObject;
        _incomingDataHelper.OnIDReceived += (int pID) => 
        {
            _outgoingDataHelper.OwnID = pID;    
        };
    }

    private void connectToServer()
    {
        try
        {
            _client = new TcpClient();
            _client.Connect(_server, _port);
            Debug.Log("Connected to server.");
        }
        catch (Exception e)
        {
            Debug.Log("Could not connect to server:");
            Debug.Log(e.Message);
        }
    }

    private void onAvatarAreaClicked(Vector3 pClickPosition)
    {
        PositionUpdate outObject = new PositionUpdate()
        {
            ID = _incomingDataHelper.OwnID,
            Position = new shared.src.Vector3(pClickPosition.x, pClickPosition.y, pClickPosition.z)
        };

        Debug.Log($"Clicked position: {pClickPosition}. Assigned position: {outObject.Position}");
        _outgoingDataHelper.sendObject(outObject);
    }

    private void onChatTextEntered(string pText)
    {
        _panelWrapper.ClearInput();

        if (pText == "/setskin")
            _outgoingDataHelper.SendSkinUpdate();
        else if (pText.StartsWith("/whisper"))
            _outgoingDataHelper.SendWhisperMessage(pText);
        else _outgoingDataHelper.SendNewMessage(pText);
    }

    private void onClosedConnection()
    {
        _client.Close();
    }

    private ISerializable receiveObject()
    {
        try
        {
            byte[] inBytes = StreamUtil.Read(_client.GetStream());
            Packet inPacket = new Packet(inBytes);
            ISerializable incomingObject = inPacket.ReadObject();
            Debug.Log($"Incoming object null?: {incomingObject == null}. Name: {incomingObject}.");
            return incomingObject;
        }
        catch (Exception e)
        {
            Debug.Log($"Exception at receiveObject(): {e.Message}");
            return null;
        }
    }

    // RECEIVING CODE
    private void Update()
    {
        try
        {
            if (_client.Available > 0)
            {
                ISerializable inObject = receiveObject();
                _incomingDataHelper.HandleIncomingObjectAction(inObject);
            }
        }
        catch (Exception e)
        {
            //for quicker testing, we reconnect if something goes wrong.
            Debug.Log($"Error thrown at Try/Catch Update(): {e.Message}.");
            //_client.Close();
            //connectToServer();
        }
    }

    private void OnApplicationQuit()
    {
        _client.GetStream().Close();
        _client.Close();
        Debug.Log("closed client");
    }
}