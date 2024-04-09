using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shared.src.protocol
{
    public class ExistingAvatars : ISerializable
    {
        public List<NewAvatar> Avatars;

        public void Deserialize(Packet pPacket)
        {
            for (int i = 0; i < Avatars.Count; i++)
            {
                NewAvatar currentAvatar = Avatars[i];
                currentAvatar.ID = pPacket.ReadInt();
                currentAvatar.SkinID = pPacket.ReadInt();

                for (int positionIndex = 0; positionIndex < 3; positionIndex++)
                {
                    currentAvatar.Position[positionIndex] = pPacket.ReadFloat();
                }
            }
        }

        public void Serialize(Packet pPacket)
        {
            for (int i = 0; i < Avatars.Count; i++)
            {
                NewAvatar currentAvatar = Avatars[i];
                pPacket.Write(currentAvatar.ID);
                pPacket.Write(currentAvatar.SkinID);
                for (int positionIndex = 0; positionIndex < 3; positionIndex++)
                {
                    pPacket.Write(currentAvatar.Position[positionIndex]);
                }
            }
        }
    }
}
