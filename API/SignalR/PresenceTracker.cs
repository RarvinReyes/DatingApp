namespace API.SignalR
{
    public class PresenceTracker
    {
        private static readonly Dictionary<string, List<string>> OnlineUsers = new Dictionary<string, List<string>>();

        public Task<bool> UserConnected(string username, string connectionId)
        {
            bool isNowOnline = false;
            lock (OnlineUsers)
            {
                if (OnlineUsers.ContainsKey(username))
                    OnlineUsers[username].Add(connectionId);
                else
                {
                    OnlineUsers.Add(username, new List<string> { connectionId });
                    isNowOnline = true;
                }
            }
            return Task.FromResult(isNowOnline);
        }

        public Task<bool> UserDisconnected(string username, string connectionId)
        {
            bool isNowOffline = false;
            lock (OnlineUsers)
            {
                if (!OnlineUsers.ContainsKey(username))
                    return Task.FromResult(isNowOffline);

                OnlineUsers[username].Remove(connectionId);

                if (OnlineUsers[username].Count() == 0)
                {
                    OnlineUsers.Remove(username);
                    isNowOffline = true;
                }
            }
            return Task.FromResult(isNowOffline);
        }

        public async Task<string[]> GetOnlineUsers()
        {
            string[] onlineUsers;
            lock (OnlineUsers)
            {
                onlineUsers = OnlineUsers.OrderBy(k => k.Key).Select(k => k.Key).ToArray();
            }

            return await Task.FromResult(onlineUsers);
        }

        public async Task<List<string>> GetConnections(string username)
        {
            List<string> onlineUsers;
            lock (OnlineUsers)
            {
                onlineUsers = OnlineUsers.Where(w => w.Key == username).Select(k => k.Value).FirstOrDefault();
            }

            return await Task.FromResult(onlineUsers);
        }

        public static Task<bool> GetOnlineStatus(string username)
        {
            bool status = false;
            lock (OnlineUsers)
            {
                status = OnlineUsers.ContainsKey(username);
            }
            return Task.FromResult(status);
        }
    }
}