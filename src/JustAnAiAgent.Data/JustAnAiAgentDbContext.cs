using JustAnAiAgent.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace JustAnAiAgent.Data;

public class JustAnAiAgentDbContext(DbContextOptions<JustAnAiAgentDbContext> options) : DbContext(options)
{
    public DbSet<Conversation> Conversations => Set<Conversation>();

    public DbSet<UserConversation> UserConversations => Set<UserConversation>();

    public DbSet<Message> Messages => Set<Message>();

    public DbSet<AgenticProject> AgenticProjects => Set<AgenticProject>();

    public DbSet<ProjectTask> ProjectTasks => Set<ProjectTask>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Conversation>()
            .HasKey(c => c.Id);

        modelBuilder.Entity<Conversation>()
            .HasOne(c => c.AgenticProject)
            .WithMany(ap => ap.Conversations)
            .HasForeignKey(c => c.ProjectId);

        modelBuilder.Entity<Conversation>()
            .HasMany(c => c.Messages)
            .WithOne(m => m.Conversation)
            .HasForeignKey(m => m.ConversationId);

        modelBuilder.Entity<Conversation>()
            .HasMany(c => c.Users)
            .WithOne(cu => cu.Conversation)
            .HasForeignKey(cu => cu.ConversationId);


        modelBuilder.Entity<Message>()
            .HasKey(m => m.Id);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.User)
            .WithMany()
            .HasForeignKey(m => m.UserId);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Conversation)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ConversationId);

        modelBuilder.Entity<Message>()
            .HasMany(m => m.AgenticProjects)
            .WithOne()
            .HasForeignKey(ap => ap.MessageId);


        modelBuilder.Entity<AgenticProject>()
            .HasKey(ap => ap.Id);

        modelBuilder.Entity<AgenticProject>()
            .HasOne(ap => ap.Message);

        modelBuilder.Entity<AgenticProject>()
            .HasMany(ap => ap.Conversations)
            .WithOne()
            .HasForeignKey(c => c.ProjectId);

        modelBuilder.Entity<AgenticProject>()
            .HasMany(ap => ap.ProjectTasks)
            .WithOne()
            .HasForeignKey(c => c.ProjectId);

        modelBuilder.Entity<ProjectTask>()
            .HasKey(pt => pt.Id);

        modelBuilder.Entity<ProjectTask>()
            .HasOne(pt => pt.AgenticProject)
            .WithMany(ap => ap.ProjectTasks)
            .HasForeignKey(pt => pt.ProjectId);


        modelBuilder.Entity<UserConversation>()
            .HasKey(uc => new { uc.UserId, uc.ConversationId });

        modelBuilder.Entity<UserConversation>()
            .HasOne(uc => uc.User)
            .WithMany()
            .HasForeignKey(uc => uc.UserId);

        modelBuilder.Entity<UserConversation>()
            .HasOne(uc => uc.Conversation)
            .WithMany(c => c.Users)
            .HasForeignKey(uc => uc.ConversationId);


    }
}