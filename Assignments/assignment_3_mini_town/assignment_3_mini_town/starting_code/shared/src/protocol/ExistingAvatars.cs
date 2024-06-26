﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace shared.src.protocol
{
    public class ExistingAvatars : ISerializable
    {
        public int AvatarCount;
        public NewAvatar[] Avatars;

        public void Serialize(Packet pPacket)
        {
            pPacket.Write(AvatarCount);

            for (int avatarIndex = 0; avatarIndex < Avatars.Length; avatarIndex++)
            {
                NewAvatar newAvatar = Avatars[avatarIndex];

                Avatars[avatarIndex] = newAvatar;

                pPacket.Write(Avatars[avatarIndex].ID);
                pPacket.Write(Avatars[avatarIndex].SkinID);
                pPacket.Write(Avatars[avatarIndex].Position);
            }
        }
        public void Deserialize(Packet pPacket)
        {
            AvatarCount = pPacket.ReadInt();

            Avatars = new NewAvatar[AvatarCount];
            for (int avatarIndex = 0; avatarIndex < Avatars.Length; avatarIndex++)
            {
                NewAvatar newAvatar = new NewAvatar();
                newAvatar.ID = pPacket.ReadInt();
                newAvatar.SkinID = pPacket.ReadInt();
                newAvatar.Position = pPacket.ReadVector3();

                Avatars[avatarIndex] = newAvatar;
            }
        }
    }
}
