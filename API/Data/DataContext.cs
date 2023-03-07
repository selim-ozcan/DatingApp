using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<AppUser> Users { get; set; }
        public DbSet<UserLike> Likes { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserLike>()
                .HasKey(like => new {like.SourceUserId, like.TargetUserId});

            builder.Entity<UserLike>()
                .HasOne(like => like.SourceUser)
                .WithMany(user => user.LikedUsers)
                .HasForeignKey(like => like.SourceUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserLike>()
                .HasOne(like => like.TargetUser)
                .WithMany(user => user.LikedByUsers)
                .HasForeignKey(like => like.TargetUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Message>()
                .HasOne(msg => msg.Sender)
                .WithMany(sender => sender.MessagesSent)
                .HasForeignKey(msg => msg.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Message>()
                .HasOne(msg => msg.Recipient)
                .WithMany(recipient => recipient.MessagesReceived)
                .HasForeignKey(msg => msg.RecipientId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}