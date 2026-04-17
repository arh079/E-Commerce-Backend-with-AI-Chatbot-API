
using ChatAPI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace ChatAPI.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {


        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public virtual DbSet<Message> Messages { get; set; }

        public virtual DbSet<Conversation> Conversations { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<UserWishlist> Wishlists { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {

            base.OnModelCreating(builder);

            //  Conversation
            builder.Entity<Conversation>(entity =>
            {
                entity.HasKey(c => c.Id);

                entity.Property(c => c.Title)
                      .HasMaxLength(200);

                entity.Property(c => c.CreatedAt)
                      .IsRequired();

                entity.Property(c => c.LastUpdatedAt)
                      .IsRequired();

                entity.HasOne(c => c.User)
                      .WithMany(u => u.Conversations)
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(c => c.Messages)
                .WithOne(m => m.Conversation)
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            });

            //  Message
            builder.Entity<Message>(entity =>
            {
                entity.HasKey(m => m.Id);

                entity.Property(m => m.Role)
                      .HasMaxLength(20)
                      .IsRequired();

                entity.Property(m => m.Content)
                      .IsRequired();

                entity.Property(m => m.CreatedAt)
                      .IsRequired();

                entity.HasOne(m => m.Conversation)
                      .WithMany(c => c.Messages)
                      .HasForeignKey(m => m.ConversationId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<ApplicationUser>(entity =>
            {
                entity.HasIndex(m => m.Email);
                entity.Property(m=>m.Email).IsRequired();


            });
            builder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.ProductId);
            });

            builder.Entity<UserWishlist>(entity =>
            {
                entity.HasKey(x => new { x.UserId, x.ProductId });

                entity.HasOne(x => x.User)
                      .WithMany(u => u.Wishlists)
                      .HasForeignKey(x => x.UserId);

                entity.HasOne(x => x.Product)
                      .WithMany(p => p.UserWishlists)
                      .HasForeignKey(x => x.ProductId);
            });
        }

    }

    }

