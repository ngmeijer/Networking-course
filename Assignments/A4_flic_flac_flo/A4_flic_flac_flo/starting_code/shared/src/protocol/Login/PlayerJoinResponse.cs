namespace shared
{
    /**
     * Send from SERVER to CLIENT to let the client know whether it was allowed to join or not.
     * Currently the only possible result is accepted.
     */
    public class PlayerJoinResponse : ASerializable
    {
        public enum RequestResult { ACCEPTED, DECLINED }; //can add different result states if you want
        public RequestResult result;
        public string Name;
        public string ResultReason;

        public override void Serialize(Packet pPacket)
        {
            pPacket.Write((int)result);
            pPacket.Write(Name);
            pPacket.Write(ResultReason);
        }

        public override void Deserialize(Packet pPacket)
        {
            result = (RequestResult)pPacket.ReadInt();
            Name = pPacket.ReadString();
            ResultReason = pPacket.ReadString();
        }
    }
}
