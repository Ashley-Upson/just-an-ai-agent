using cCoder.Security.Objects;
using JustAnAiAgent.Objects.Entities;
using Microsoft.EntityFrameworkCore;

namespace JustAnAiAgent.Data;

public class JustAnAiAgentDbContext
        : DbContext
{
    public JustAnAiAgentDbContext(DbContextOptions<JustAnAiAgentDbContext> options) : base(options) { }

    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<UserConversation> UserConversations { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<AgenticProject> AgenticProjects { get; set; }
    public DbSet<ProjectTask> ProjectTasks { get; set; }
    public ISSOAuthInfo AuthInfo { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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
            .HasOne(ap => ap.Message)
            .WithMany(m => m.AgenticProjects)
            .HasForeignKey(ap => ap.MessageId);

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


        modelBuilder.Entity<UserConversation>(entity =>
        {
            entity.HasKey(uc => new { uc.UserId, uc.ConversationId });

            entity.HasOne(uc => uc.Conversation)
                .WithMany(c => c.Users)
                .HasForeignKey(uc => uc.ConversationId);
        });


        IEnumerable<Microsoft.EntityFrameworkCore.Metadata.IMutableForeignKey> cascadingRelationships = modelBuilder
            .Model
            .GetEntityTypes()
            .SelectMany(t => t.GetForeignKeys())
            .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade);

        foreach (Microsoft.EntityFrameworkCore.Metadata.IMutableForeignKey relationship in cascadingRelationships)
            relationship.DeleteBehavior = DeleteBehavior.Restrict;

        base.OnModelCreating(modelBuilder);
    }

    public void Migrate() =>
        Database.Migrate();
}