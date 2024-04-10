using shared;
using shared.src;
using shared.src.protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace server
{
    public class DataProcessor
    {
        private DataSender _dataSender;

        public DataProcessor(DataSender pHandler)
        {
            _dataSender = pHandler;
        }

        public void SyncMessagesAcrossClients(TcpClient pSender, SimpleMessage pMessage, Dictionary<TcpClient, NewAvatar> pClientAvatars)
        {
            foreach (KeyValuePair<TcpClient, NewAvatar> receiver in pClientAvatars)
            {
                _dataSender.SendMessage(receiver.Key, pMessage);
                //If it is not a whisper message, send it to all avatars.
            }
        }

        private SimpleMessage filterMessage(string pMessageText, string pExcludedeText)
        {
            string text = pMessageText;
            string command = pExcludedeText;
            int index = text.IndexOf(command);
            string filteredMessage = pMessageText.Substring(index + command.Length).Trim();
            pMessageText = filteredMessage;

            return new SimpleMessage() { Text = pMessageText};
        }

        private bool isWhisperMessage(SimpleMessage pMessage)
        {
            string[] data = pMessage.Text.Split();
            if (data[0] == "/whisper")
                return true;

            return false;
        }

        private bool isReceiverInRange(float pMaxDistance, Vector3 pAvatarSenderPosition, Vector3 pAvatarReceiverPosition)
        {
            double distance = pAvatarSenderPosition.Distance(pAvatarReceiverPosition);
            Console.WriteLine($"Distance to other client: {distance}");
            if (distance <= pMaxDistance)
                return true;

            return false;
        }

        public void SyncWhisperMessagesAcrossClients(TcpClient pSender, WhisperMessage pMessage, Dictionary<TcpClient, NewAvatar> pClientAvatars)
        {
            SimpleMessage newMessage = filterMessage(pMessage.Text, "/whisper");
            newMessage.SenderID = pMessage.SenderID;
            _dataSender.SendMessage(pSender, newMessage);

            foreach (KeyValuePair<TcpClient, NewAvatar> receiver in pClientAvatars)
            {
                if (receiver.Key != pSender && isReceiverInRange(2, pMessage.Position, receiver.Value.Position))
                {
                    _dataSender.SendMessage(receiver.Key, newMessage);
                }
            }
        }
    }
}
