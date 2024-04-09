using shared;
using shared.src.protocol;
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
    private int _ownID;

    [Tooltip("How far away from the center of the scene will we spawn avatars?")]
    public float spawnRange = 13;
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
        PositionUpdate outObject = new PositionUpdate()
        {
            ID = _ownID,
            Position = new shared.src.Vector3(pClickPosition.x, pClickPosition.y, pClickPosition.z)
        };

        sendObject(outObject);
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
            Vector3 currentPosition = _areaManager.GetAvatarView(_ownID).transform.localPosition;
            SimpleMessage message = new SimpleMessage()
            {
                SenderID = _ownID,
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
            Debug.Log($"Error thrown at Try/Catch Update(): {e.Message}.");
            //_client.Close();
            //connectToServer();
        }
    }

    private void handleIncomingObjectAction(ISerializable pInObject)
    {
        switch (pInObject)
        {
            case SimpleMessage message:
                handleSimpleMessage(message);
                break;
            case NewAvatar avatarContainer:
                handleNewAvatar(avatarContainer);
                break;
            case PositionUpdate positionUpdate:
                handlePositionUpdate(positionUpdate);
                break;
            case SkinRequest skinRequest:
                handleSkinRequest(skinRequest);
                break;
            case HeartBeat heartBeat:
                handleHeartBeatUpdate();
                break;
            case DeadAvatar deadAvatar:
                //handleAvatarRemove(deadAvatar);
                break;
            case ExistingAvatars existingAvatars:
                handleExistingAvatars(existingAvatars);
                break;
        }
    }

    private void handleExistingAvatars(ExistingAvatars pExistingAvatars)
    {
        Debug.Log($"avatar id array null? {pExistingAvatars.Avatars == null}");
        Debug.Log($"avatar id array length {pExistingAvatars.Avatars.Length}");

        foreach (NewAvatar avatar in pExistingAvatars.Avatars)
        {
            if(avatar != null)
                Debug.Log($"avatar id: {avatar.ID}");
        }
    }

    private void handleAvatarRemove(DeadAvatar pDeadAvatar)
    {
        _areaManager.RemoveAvatarView(pDeadAvatar.ID);
    }

    private void handleHeartBeatUpdate()
    {
        HeartBeat heartBeat = new HeartBeat()
        {
            ClientID = _ownID
        };
        sendObject(heartBeat);
    }

    private void handleSkinRequest(SkinRequest pIncomingObject)
    {
        _areaManager.GetAvatarView(pIncomingObject.ID).SetSkin(pIncomingObject.SkinID);
    }

    private void handlePositionUpdate(PositionUpdate pIncomingObject)
    {
        Vector3 newPosition = new Vector3(pIncomingObject.Position.x, pIncomingObject.Position.y, pIncomingObject.Position.z);
        _areaManager.GetAvatarView(pIncomingObject.ID).Move(newPosition);
    }

    private void handleNewAvatar(NewAvatar pIncomingObject)
    {
        try
        {
            Vector3 avatarPosition = new Vector3(pIncomingObject.Position.x, pIncomingObject.Position.y, pIncomingObject.Position.z);

            //The position is not assigned yet, so it must mean this is
            //the NewAvatar that is meant to be controlled by this client
            if (avatarPosition == Vector3.zero)
            {
                Debug.Log("Position is not set yet for new Avatar.");
                _ownID = pIncomingObject.ID;
                avatarPosition = sendInitialPosition(pIncomingObject);
            }

            createNewAvatar(pIncomingObject.ID, pIncomingObject.SkinID, avatarPosition);
        }
        catch(Exception e)
        {
            Debug.Log($"Error on adding new avatar: {e.Message}");
        }
    }

    private void createNewAvatar(int pID, int pSkinID, Vector3 pPosition)
    {
        AvatarView avatarView = _areaManager.AddAvatarView(pID);
        avatarView.SetSkin(pSkinID);
        avatarView.transform.localPosition = pPosition;
    }

    private Vector3 sendInitialPosition(NewAvatar pIncomingObject)
    {
        Vector3 randomPos = getRandomPosition();
        ISerializable outObject = new NewAvatar()
        {
            ID = pIncomingObject.ID,
            SkinID = pIncomingObject.SkinID,
            Position = new shared.src.Vector3(randomPos.x, randomPos.y, randomPos.z)
        };
        sendObject(outObject);

        return randomPos;
    }

    private void handleSimpleMessage(SimpleMessage pIncomingObject)
    {
        AvatarView avatarView = _areaManager.GetAvatarView(pIncomingObject.SenderID);
        avatarView.Say(pIncomingObject.Text);
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