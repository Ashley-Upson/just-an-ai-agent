using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace JustAnAiAgent.Data.Interfaces;

public interface IJustAnAiAgentModelBuildProvider
{
    void Configure(DbContextOptionsBuilder optionsBuilder);
    void Create(ModelBuilder modelBuilder);
    void MigrateDatabase(DatabaseFacade database);
}