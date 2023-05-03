using System.Collections;
using System.Security.AccessControl;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using API.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;

namespace API.SignalR
{
    [Authorize]
    public class MessageHub : Hub
    {
        private readonly IMapper _mapper;
        private readonly IHubContext<PresenceHub> _presenceHub;
        public PresenceTracker _presenceTracker { get; }
        private readonly IUnitOfWork _uow;
        public MessageHub(IMapper mapper, IUnitOfWork uow,
        IHubContext<PresenceHub> presenceHub, PresenceTracker presenceTracker)
        {
            _uow = uow;
            _presenceTracker = presenceTracker;
            _presenceHub = presenceHub;
            _mapper = mapper;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var otherUser = httpContext.Request.Query["user"];
            var groupName = GetGroupName(Context.User.GetUsername(), otherUser);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            var group = await AddToGroup(groupName);

            await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

            var messages = await _uow.MessageRepository.GetMessageThread(Context.User.GetUsername(), otherUser);

            if(_uow.HasChanges()) {
                await _uow.Complete();
            }

            await Clients.Caller.SendAsync("ReceiveMessageThread", messages);
        }

        private string GetGroupName(string caller, string other)
        {
            var stringCompare = string.CompareOrdinal(caller, other);
            return stringCompare <= 0 ? $"{caller}-{other}" : $"{other}-{caller}";
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var group = await RemoveFromMessageGroup();
            await Clients.Group(group.Name).SendAsync("UpdatedGroup", group);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
            var username = Context.User.GetUsername();

            if (username == createMessageDto.RecipientUsername.ToLower())
                throw new HubException("You cannot send a message to yourself");

            var sender = await _uow.UserRepository.GetUserByUsernameAsync(username);
            var recipient = await _uow.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

            if (recipient == null)
                throw new HubException("User not found");

            var message = new Message
            {
                Sender = sender,
                SenderId = sender.Id,
                SenderUsername = sender.UserName,
                Recipient = recipient,
                RecipientId = recipient.Id,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content
            };

            var groupName = GetGroupName(sender.UserName, recipient.UserName);
            var group = await _uow.MessageRepository.GetGroup(groupName);

            if (group.Connections.Any(a => a.Username == recipient.UserName))
            {
                message.DateRead = DateTime.UtcNow;
            }
            else
            {
                var connections = await this._presenceTracker.GetConnections(recipient.UserName);

                if (connections != null && connections.Count() > 0)
                {
                    await _presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived"
                    , new { username = sender.UserName, knownAs = sender.KnownAs });
                }
            }

            _uow.MessageRepository.AddMessage(message);

            if (await _uow.Complete())
            {
                await Clients.Group(groupName).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
            }
        }

        private async Task<Group> AddToGroup(string groupName)
        {
            var group = await _uow.MessageRepository.GetGroup(groupName);
            var connection = new Connection(Context.ConnectionId, Context.User.GetUsername());

            if (group == null)
            {
                group = new Group(groupName);
                group.Connections.Add(connection);
                _uow.MessageRepository.AddGroup(group);
            }
            else
            {
                group.Connections.Add(connection);
            }

            if (await _uow.Complete())
            {
                return group;
            }
            throw new HubException("Failed to add to group.");
        }

        private async Task<Group> RemoveFromMessageGroup()
        {
            var group = await _uow.MessageRepository.GetGroupForConnection(Context.ConnectionId);
            _uow.MessageRepository.RemoveConnection(group.Connections.FirstOrDefault(f => f.ConnectionId == Context.ConnectionId));

            if (await _uow.Complete())
            {
                return group;
            }

            throw new HubException("Failed to remove from group.");
        }
    }
}