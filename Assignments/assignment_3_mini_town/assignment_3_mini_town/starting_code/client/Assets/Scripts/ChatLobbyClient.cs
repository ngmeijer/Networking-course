using shared;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Xml.Linq;
using UnityEngine;

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

    [SerializeField] private string _server = "localhost";
    [SerializeField] private int _port = 55555;

    private TcpClient _client;

    [Tooltip("How far away from the center of the scene will we spawn avatars?")]
    public float spawnRange = 10;
    [Tooltip("What is the minimum angle from the center we are spawning the avatar at?")]
    public float spawnMinAngle = 0;
    [Tooltip("What is the maximum angle from the center we are spawning the avatar at?")]
    public float spawnMaxAngle = 180;

    private void Start()
    {
        connectToServer();

        //register for the important events
        _areaManager = FindObjectOfType<AvatarAreaManager>();
        _areaManager.OnAvatarAreaClicked += onAvatarAreaClicked;

        _panelWrapper = FindObjectOfType<PanelWrapper>();
        _panelWrapper.OnChatTextEntered += onChatTextEntered;
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
        Debug.Log("ChatLobbyClient: you clicked on " + pClickPosition);
        //TODO pass data to the server so that the server can send a position update to all clients (if the position is valid!!)
    }

    private void onChatTextEntered(string pText)
    {
        _panelWrapper.ClearInput();
        sendMessage(pText);
    }

    private void onClosedConnection()
    {
        _client.Close();
    }

    private void sendMessage(string pOutString)
    {
        try
        {
            SimpleMessage message = new SimpleMessage();
            message.Text = pOutString;
            sendObject(message);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            //_client.Close();
            //connectToServer();
        }
    }

    private ISerializable receiveObject()
    {
        try
        {
            byte[] inBytes = StreamUtil.Read(_client.GetStream());
            Packet inPacket = new Packet(inBytes);
            ISerializable incomingObject = inPacket.ReadObject();
            Debug.Log(incomingObject);
            return incomingObject;
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            return null;
        }
    }

    private void sendObject(ISerializable pOutObject)
    {
        try
        {
            Packet outPacket = new Packet();
            outPacket.Write(pOutObject);
            StreamUtil.Write(_client.GetStream(), outPacket.GetBytes());
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
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
                Debug.Log($"Object received: {inObject}");
                handleIncomingObjectAction(inObject);
            }
        }
        catch (Exception e)
        {
            //for quicker testing, we reconnect if something goes wrong.
            Debug.Log($"Error: {e.Message}.");
            //_client.Close();
            //connectToServer();
        }
    }

    private void handleIncomingObjectAction(ISerializable pInObject)
    {
        switch (pInObject)
        {
            case SimpleMessage message:
                showMessage(0, message.Text);
                break;
            case AvatarContainer avatarContainer:
                handleAvatarCreation(avatarContainer);
                break;
            case PositionRequest positionRequest:
                handlePositionRequest(positionRequest);
                break;
            case SkinRequest skinRequest:
                handleSkinRequest(skinRequest);
                break;
        }
    }

    private void handleSkinRequest(SkinRequest pSkinRequest)
    {
        _areaManager.GetAvatarView(pSkinRequest.ID).SetSkin(pSkinRequest.SkinID);
    }

    private void handleAvatarCreation(AvatarContainer pContainer)
    {
        Debug.Log("Adding avatar");
        _areaManager.AddAvatarView(pContainer.ID);
        _areaManager.GetAvatarView(pContainer.ID).SetSkin(pContainer.SkinID);

        PositionRequest positionRequest = new PositionRequest()
        {
            ID = pContainer.ID,
        };
        handlePositionRequest(positionRequest);
    }

    private void handlePositionRequest(PositionRequest pIncomingObject)
    {
        Vector3 randomPos = getRandomPosition();
        Debug.Log($"Generated position: {randomPos}");
        ISerializable outObject = new PositionRequest()
        {
            ID = pIncomingObject.ID,
            Position = new float[3]
            {
                  randomPos.x,
                  randomPos.y,
                  randomPos.z
             },
        };
        sendObject(outObject);
    }

    private void showMessage(int pSenderID, string pText)
    {
        //This is a stub for what should actually happen
        //What should actually happen is use an ID that you got from the server, to get the correct avatar
        //and show the text message through that
        List<int> allAvatarIds = _areaManager.GetAllAvatarIds();

        if (allAvatarIds.Count == 0)
        {
            Debug.Log("No avatars available to show text through:" + pText);
            return;
        }

        int randomAvatarId = allAvatarIds[UnityEngine.Random.Range(0, allAvatarIds.Count)];
        AvatarView avatarView = _areaManager.GetAvatarView(randomAvatarId);
        avatarView.Say(pText);
    }

    /**
     * Returns a position somewhere in town.
     */
    private Vector3 getRandomPosition()
    {
        //set a random position
        float randomAngle = UnityEngine.Random.Range(spawnMinAngle, spawnMaxAngle) * Mathf.Deg2Rad;
        float randomDistance = UnityEngine.Random.Range(0, spawnRange);
        return new Vector3(Mathf.Cos(randomAngle), 0, Mathf.Sin(randomAngle)) * randomDistance;
    }

    private void OnApplicationQuit()
    {
        _client.GetStream().Close();
        _client.Close();
        Debug.Log("closed client");
    }
}