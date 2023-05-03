using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        public DataContext _context { get; }
        private readonly IMapper _mapper;
        public MessageRepository(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;

        }
        public void AddMessage(Message message)
        {
            _context.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _context.Messages.Remove(message);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages.FindAsync(id);
        }

        public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            var query = _context.Messages
                .OrderByDescending(x => x.MessageSent)
                .AsQueryable();

            query = messageParams.Container switch
            {
                "Inbox" => query.Where(w => w.RecipientUsername == messageParams.Username && !w.RecipientDeleted),
                "Outbox" => query.Where(w => w.SenderUsername == messageParams.Username && !w.SenderDeleted),
                _ => query.Where(w => w.RecipientUsername == messageParams.Username && !w.RecipientDeleted && w.DateRead == null)
            };

            var messages = query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);

            return await PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string targetUsername)
        {
            var query = _context.Messages
            .Where(w => (w.RecipientUsername == currentUsername && w.SenderUsername == targetUsername && !w.RecipientDeleted)
                    || (w.RecipientUsername == targetUsername && w.SenderUsername == currentUsername && !w.SenderDeleted))
            .OrderBy(o => o.MessageSent)
            .AsQueryable();

            var unreadMessages = query
                .Where(w => w.DateRead == null && w.RecipientUsername == currentUsername)
                .ToList();

            if (unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
                {
                    message.DateRead = DateTime.UtcNow;
                }
            }

            return await query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider).ToListAsync();
        }

        public async void AddGroup(Group group)
        {
            await _context.Groups.AddAsync(group);
        }

        public void RemoveConnection(Connection connection)
        {
            _context.Connections.Remove(connection);
        }

        public async Task<Connection> GetConnection(string connectionId)
        {
            return await _context.Connections.FindAsync(connectionId);
        }

        public async Task<Group> GetGroup(string groupName)
        {
            return await _context.Groups.Include(i => i.Connections).FirstOrDefaultAsync(f => f.Name == groupName);
        }

        public async Task<List<string>> GetConnectionsForUSer(string username)
        {
            return await _context.Connections
            .Where(w => w.Username == username)
            .Select(s => s.ConnectionId)
            .ToListAsync();
        }

        public async Task<Group> GetGroupForConnection(string connectionId)
        {
            return await _context.Groups.Include(i => i.Connections)
            .Where(w => w.Connections.Any(c => c.ConnectionId == connectionId))
            .FirstOrDefaultAsync();
        }
    }
}