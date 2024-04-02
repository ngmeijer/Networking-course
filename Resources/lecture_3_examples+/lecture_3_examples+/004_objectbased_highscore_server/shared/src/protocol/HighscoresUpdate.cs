using System.Collections.Generic;

namespace shared
{
    /**
     * Sent from SERVER 2 CLIENT.
     */
    public class HighscoresUpdate : ISerializable
    {
        public List<Score> scores;

        public void Serialize(Packet pPacket)
        {
            int count = (scores == null ? 0 : scores.Count);

            pPacket.Write(count);

            for (int i = 0; i < count; i++)
            {
                pPacket.Write(scores[i]);
            }
        }

        public void Deserialize(Packet pPacket)
        {
            scores = new List<Score>();

            int count = pPacket.ReadInt();

            for (int i = 0; i < count; i++)
            {
                scores.Add(pPacket.Read<Score>());
            }
        }
    }
}
