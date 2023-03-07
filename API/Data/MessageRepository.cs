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
        public void AddMessage(Message message)
        {
            _context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _context.Messages.Remove(message);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages.FindAsync(id);
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
            var messages = await _context.Messages
                .Include(msg => msg.Sender).ThenInclude(sender => sender.Photos)
                .Include(msg => msg.Recipient).ThenInclude(recipient => recipient.Photos)
                .Where( msg => msg.RecipientUsername == currentUserName
                            && msg.RecipientDeleted == false
                            && msg.SenderUsername == recipientUserName ||
                               msg.RecipientUsername == recipientUserName
                            && msg.SenderDeleted == false
                            && msg.SenderUsername == currentUserName)
                .OrderBy(msg => msg.MessageSent)
                .ToListAsync();

            var unreadMessages = messages
                .Where(msg => msg.DateRead == null && msg.RecipientUsername == currentUserName)
                .ToList();  

            if (unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
                {
                    message.DateRead = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
            }

            return _mapper.Map<IEnumerable<MessageDto>>(messages);
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}