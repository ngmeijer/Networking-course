using shared.src.protocol;
using shared;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IncomingDataHelper : MonoBehaviour
{
    public AvatarAreaManager AreaManager;

    public Action<ISerializable> OnDataGoingOut = delegate { };
    public Action<int> OnIDReceived = delegate { };
    public int OwnID = -1;

    [Tooltip("How far away from the center of the scene will we spawn avatars?")]
    public float spawnRange = 13;
    [Tooltip("What is the minimum angle from the center we are spawning the avatar at?")]
    public float spawnMinAngle = 0;
    [Tooltip("What is the maximum angle from the center we are spawning the avatar at?")]
    public float spawnMaxAngle = 180;


    public void HandleIncomingObjectAction(ISerializable pInObject)
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
            case SkinUpdate skinUpdate:
                handleSkinUpdate(skinUpdate);
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
            createNewAvatar(avatar.ID, avatar.SkinID, new Vector3(avatar.Position.x, avatar.Position.y, avatar.Position.z));
        }
    }

    private void handleAvatarRemove(DeadAvatar pDeadAvatar)
    {
        AreaManager.RemoveAvatarView(pDeadAvatar.ID);
    }

    private void handleHeartBeatUpdate()
    {
        HeartBeat heartBeat = new HeartBeat()
        {
            ClientID = OwnID
        };

        OnDataGoingOut(heartBeat);
    }

    private void handleSkinUpdate(SkinUpdate pIncomingObject)
    {
        AreaManager.GetAvatarView(pIncomingObject.ID).SetSkin(pIncomingObject.SkinID);
    }

    private void handlePositionUpdate(PositionUpdate pIncomingObject)
    {
        Vector3 newPosition = new Vector3(pIncomingObject.Position.x, pIncomingObject.Position.y, pIncomingObject.Position.z);
        AreaManager.GetAvatarView(pIncomingObject.ID).Move(newPosition);
    }

    private void handleNewAvatar(NewAvatar pIncomingObject)
    {
        try
        {
            Debug.Log($"Incoming position: {pIncomingObject.Position}");
            Vector3 avatarPosition = new Vector3(pIncomingObject.Position.x, pIncomingObject.Position.y, pIncomingObject.Position.z);

            //The position is not assigned yet, so it must mean this is
            //the NewAvatar that is meant to be controlled by this client
            if (avatarPosition == Vector3.zero)
            {
                Debug.Log("Position is not set yet for new Avatar.");
                OwnID = pIncomingObject.ID;
                OnIDReceived(OwnID);
                avatarPosition = getRandomPosition();
                Debug.Log($"Generated position: {avatarPosition}");
                Debug.Log($"Received ID: {OwnID}");
                pIncomingObject.Position = new shared.src.Vector3(avatarPosition.x, avatarPosition.y, avatarPosition.z);
                Debug.Log($"Position assigned to ISerializable: {pIncomingObject.Position}");
                OnDataGoingOut(pIncomingObject);
            }

            createNewAvatar(pIncomingObject.ID, pIncomingObject.SkinID, avatarPosition);
        }
        catch (Exception e)
        {
            Debug.Log($"Error on adding new avatar: {e.Message}");
        }
    }

    private void createNewAvatar(int pID, int pSkinID, Vector3 pPosition)
    {
        AvatarView avatarView = AreaManager.AddAvatarView(pID);
        avatarView.SetSkin(pSkinID);
        avatarView.transform.localPosition = new UnityEngine.Vector3(pPosition.x, pPosition.y, pPosition.z);

        if (pID == OwnID)
        {
            Debug.Log($"Enabled ring for avatar {pID}");
            avatarView.ShowRing();
        }
    }

    private void handleSimpleMessage(SimpleMessage pIncomingObject)
    {
        AvatarView avatarView = AreaManager.GetAvatarView(pIncomingObject.SenderID);
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
}
