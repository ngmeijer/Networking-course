using shared;
using shared.src;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace server
{
    public class DataProcessor
    {
        private DataSender _dataSender;

        public DataProcessor(DataSender pHandler) {
            _dataSender = pHandler;
        }

        public void SyncMessagesAcrossClients(TcpClient pSender, SimpleMessage pMessage, Dictionary<TcpClient, NewAvatar> pClientAvatars)
        {
            foreach (KeyValuePair<TcpClient, NewAvatar> receiver in pClientAvatars)
            {
                //If it is not a whisper message, send it to all avatars.
                if (!isWhisperMessage(pMessage))
                {
                    _dataSender.SendMessage(receiver.Key, pMessage);
                    continue;
                }

                //Take out the /whisper command
                pMessage = filterMessage(pMessage, "/whisper");

                //
                if (receiver.Key != pSender && isReceiverInRange(2, pMessage.Position, receiver.Value.Position))
                {
                    _dataSender.SendMessage(receiver.Key, pMessage);
                }
            }
        }

        private SimpleMessage filterMessage(SimpleMessage pMessage, string pExcludedeText)
        {
            string text = pMessage.Text;
            string command = pExcludedeText;
            int index = text.IndexOf(command);
            string filteredMessage = pMessage.Text.Substring(index + command.Length).Trim();
            pMessage.Text = filteredMessage;

            return pMessage;
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
            double xDifference = pAvatarSenderPosition.x - pAvatarReceiverPosition.x;
            double yDifference = pAvatarSenderPosition.y - pAvatarReceiverPosition.y;
            double zDifference = pAvatarSenderPosition.z - pAvatarReceiverPosition.z;

            double distance = Math.Sqrt(Math.Pow(xDifference, 2) + Math.Pow(yDifference, 2) + Math.Pow(zDifference, 2));
            Console.WriteLine($"Distance: {distance}");
            if (distance <= pMaxDistance)
                return true;

            return false;
        }
    }
}
