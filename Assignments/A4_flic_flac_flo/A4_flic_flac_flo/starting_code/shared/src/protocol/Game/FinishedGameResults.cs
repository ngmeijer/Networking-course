using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shared
{
    public class FinishedGameResults : ASerializable
    {
        public string PlayerLostName;
        public string PlayerWonName;

        public bool HasSurrendered;

        public override void Deserialize(Packet pPacket)
        {
            PlayerLostName = pPacket.ReadString();
            PlayerWonName = pPacket.ReadString();
            HasSurrendered = pPacket.ReadBool();
        }

        public override void Serialize(Packet pPacket)
        {
            pPacket.Write(PlayerLostName);
            pPacket.Write(PlayerWonName);
            pPacket.Write(HasSurrendered);
        }
    }
}
