using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shared.src.protocol
{
    public class WhisperMessage : ISerializable
    {
        public int SenderID;
        public string Text;
        public Vector3 Position = new Vector3();

        public void Serialize(Packet pPacket)
        {
            pPacket.Write(SenderID);
            pPacket.Write(Text);
            pPacket.Write(Position);
        }

        public void Deserialize(Packet pPacket)
        {
            SenderID = pPacket.ReadInt();
            Text = pPacket.ReadString();
            Position = pPacket.ReadVector3();
        }
    }
}
