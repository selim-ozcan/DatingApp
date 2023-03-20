using System.Data.Common;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    [Authorize]
    public class MessageHub : Hub
    {
        private readonly IMapper _mapper;
        private readonly IHubContext<PresenceHub> _presenceHub;
        private readonly IUnitOfWork _uow;

        public MessageHub(IUnitOfWork uow, IMapper mapper, IHubContext<PresenceHub> presenceHub)
        {
            _uow = uow;
            _presenceHub = presenceHub;
            _mapper = mapper;
        }
        public override async Task OnConnectedAsync()
        {
            var recipientUsername = Context.GetHttpContext().Request.Query["user"];
            var senderUsername = Context.User.GetUsername();

            var groupName = GetGroupName(senderUsername, recipientUsername);
           
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await AddToGroup(groupName);

            var messageThread = await _uow.MessageRepository.GetMessageThread(senderUsername, recipientUsername);
            if (_uow.HasChanges()) await _uow.Complete();

            await Clients.Group(groupName).SendAsync("ReceiveMessageThread", messageThread);
        }

        private string GetGroupName(string sender, string recipient)
        {
            var stringCompare = string.CompareOrdinal(sender, recipient) < 0;
            var groupName = stringCompare ? sender + "-" + recipient : recipient + "-" + sender;
            return groupName;
        }
        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
            var username = Context.User.GetUsername();

            if (username == createMessageDto.RecipientUsername.ToLower()) throw new HubException("You cannot send messages to yourself");

            var sender = await _uow.UserRepository.GetUserByUsernameAsync(username);
            var recipient = await _uow.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

            if (recipient == null) throw new HubException("Not found recipient");

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content
            };

            var groupName = GetGroupName(sender.UserName, recipient.UserName);
            var group = await _uow.MessageRepository.GetMessageGroup(groupName);

            if (group.Connections.Any(x => x.Username == recipient.UserName))
            {
                message.DateRead = DateTime.UtcNow;
            }
            else
            {
                var connections = await PresenceTracker.GetConnectionsForUser(recipient.UserName);
                if (connections != null)
                {
                    await _presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived", sender.KnownAs);
                }
            }

            _uow.MessageRepository.AddMessage(message);
            if (await _uow.Complete())
            {
                await Clients.Group(groupName).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
                return;
            } 

            throw new HubException("Failed to send message");
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await RemoveFromMessageGroup();
            await base.OnDisconnectedAsync(exception);
        }

        private async Task<bool> AddToGroup(string groupName)
        {
            var group = await _uow.MessageRepository.GetMessageGroup(groupName);
            var connection = new Connection(Context.ConnectionId, Context.User.GetUsername());
             
            if(group == null)
            {
                group = new Group(groupName);
                _uow.MessageRepository.AddGroup(group);  
            } 
            group.Connections.Add(connection);

            return await _uow.Complete();
        }

        private async Task RemoveFromMessageGroup()
        {
            var connection = await _uow.MessageRepository.GetConnection(Context.ConnectionId);
            _uow.MessageRepository.RemoveConnection(connection);
            await _uow.Complete();
        } 
    }
}