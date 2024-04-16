using System;

namespace shared
{
    /**
     * Empty placeholder class for the PlayerInfo object which is being tracked for each client by the server.
     * Add any data you want to store for the player here and make it extend ASerializable.
     */

    public class PlayerInfo : ASerializable
    {
        public string PlayerName;
        public int PlayerId;
        public int MoveCount;
        public bool HasSurrendered;

        public override void Deserialize(Packet pPacket)
        {
            PlayerName = pPacket.ReadString();
            PlayerId = pPacket.ReadInt();
            MoveCount = pPacket.ReadInt();
            HasSurrendered = pPacket.ReadBool();
        }

        public override void Serialize(Packet pPacket)
        {
            pPacket.Write(PlayerName);
            pPacket.Write(PlayerId);
            pPacket.Write(MoveCount);
            pPacket.Write(HasSurrendered);
        }
    }
}
