using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace API.SignalR
{
     public class PresenceTracker
    {
        private static readonly Dictionary<string, List<string>> OnlineUsers = new Dictionary<string, List<string>>();

        public void UserConnected(string username, string connectionId)
        {
            lock(OnlineUsers)
            {
                if (OnlineUsers.ContainsKey(username))
                {
                    OnlineUsers[username].Add(connectionId);
                } 
                else
                {
                    OnlineUsers.Add(username, new List<string>() { connectionId });
                }
            }
        }

        public void UserDisconnected(string username, string connectionId)
        {
            lock(OnlineUsers)
            {
                if (!OnlineUsers.ContainsKey(username)) return;
                OnlineUsers[username].Remove(connectionId);

                if (OnlineUsers[username].Count == 0) OnlineUsers.Remove(username);
            }
        }

        public string[] GetOnlineUsers()
        {
            string[] onlineUsers;
            lock(OnlineUsers)
            {
                onlineUsers = OnlineUsers.OrderBy(k => k.Key).Select(k => k.Key).ToArray();
            }

            return onlineUsers;
        }

        public static Task<List<string>> GetConnectionsForUser(string username)
        {
            List<string> connectionIds;
            lock(OnlineUsers)
            {
                connectionIds = OnlineUsers.GetValueOrDefault(username);

            }

            return Task.FromResult(connectionIds);
        }
    }
}