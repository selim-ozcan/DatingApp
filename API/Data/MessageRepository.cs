using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public MessageRepository(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
            
        }

        public void AddGroup(Group group)
        {
            _context.Groups.Add(group);
        }

        public void AddMessage(Message message)
        {
            _context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _context.Messages.Remove(message);
        }

        public async Task<Connection> GetConnection(string connectionId)
        {
            var connection = await _context.Connections.FindAsync(connectionId);
            return connection;
        }
  
        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages.FindAsync(id);
        }

        public async Task<Group> GetMessageGroup(string groupName)
        {
            return await _context.Groups.Include(group => group.Connections).FirstOrDefaultAsync(group => group.Name == groupName);
        }

        public Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            var query = _context.Messages
                .OrderByDescending(msg => msg.MessageSent)
                .AsQueryable();

            query = messageParams.Container switch
            {
                "Inbox" => query.Where(msg => msg.RecipientUsername == messageParams.Username && msg.RecipientDeleted == false),
                "Outbox" => query.Where(msg => msg.SenderUsername == messageParams.Username && msg.SenderDeleted == false),
                _ => query.Where(msg => msg.RecipientUsername == messageParams.Username && msg.RecipientDeleted == false && msg.DateRead == null),
                
            };

            var messages = query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);
            return PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUserName, string recipientUserName)
        {
            var query = _context.Messages
                .Where( msg => msg.RecipientUsername == currentUserName
                            && msg.RecipientDeleted == false
                            && msg.SenderUsername == recipientUserName ||
                               msg.RecipientUsername == recipientUserName
                            && msg.SenderDeleted == false
                            && msg.SenderUsername == currentUserName)
                .OrderBy(msg => msg.MessageSent)
                .AsQueryable();

            var unreadMessages = query
                .Where(msg => msg.DateRead == null && msg.RecipientUsername == currentUserName)
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

        public void RemoveConnection(Connection connection)
        {
            _context.Connections.Remove(connection);
        }
    }
}