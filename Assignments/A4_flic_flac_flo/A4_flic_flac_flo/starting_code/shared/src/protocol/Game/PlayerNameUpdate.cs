using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shared.src.protocol.Lobby
{
    public class PlayerNameUpdate : ASerializable
    {
        public string Player1Name;
        public string Player2Name;

        public override void Deserialize(Packet pPacket)
        {
            Player1Name = pPacket.ReadString();
            Player2Name = pPacket.ReadString();
        }

        public override void Serialize(Packet pPacket)
        {
            pPacket.Write(this.Player1Name);
            pPacket.Write(this.Player2Name);
        }
    }
}
