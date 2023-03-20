using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    [Authorize]
    public class PresenceHub : Hub
    {
        private readonly PresenceTracker _presenceTracker;
        private readonly IUnitOfWork _uow;
        public PresenceHub(PresenceTracker presenceTracker, IUnitOfWork uow)
        {
            _uow = uow;
            _presenceTracker = presenceTracker;
            
        }
        public override async Task OnConnectedAsync()
        {
            var username = Context.User.GetUsername();
            _presenceTracker.UserConnected(username, Context.ConnectionId);

            var user = await _uow.UserRepository.GetUserByUsernameAsync(username);
            var likedByUsers = await _uow.LikesRepository.GetUserLikes(new LikesParams { UserId=user.Id, Predicate = "likedBy"});
            var onlineUsers = _presenceTracker.GetOnlineUsers();

            List<string> usersToSendNotification = new List<string>();
            foreach (var name in onlineUsers)
            {
                foreach (var likedByUser in likedByUsers)
                {
                    if (likedByUser.UserName == name) 
                    {
                        usersToSendNotification.Add(name);
                    }
                }
            }
            List<string> connectionIds = new List<string>();
            foreach (var x in usersToSendNotification)
            {
                var connections = await PresenceTracker.GetConnectionsForUser(x);
                foreach (var connection in connections)
                {
                    connectionIds.Add(connection);
                }
            }
            
            await Clients.Clients(connectionIds).SendAsync("UserIsOnline", username);

            var currentUsers = _presenceTracker.GetOnlineUsers();
            await Clients.All.SendAsync("GetOnlineUsers", currentUsers);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var username = Context.User.GetUsername();
            _presenceTracker.UserDisconnected(username, Context.ConnectionId);
            await Clients.Others.SendAsync("UserIsOffline", username);

            var currentUsers = _presenceTracker.GetOnlineUsers();
            await Clients.All.SendAsync("GetOnlineUsers", currentUsers);
            
            await base.OnDisconnectedAsync(exception);
        }
    }
}