using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace JustAnAiAgent.Data;

class JustAnAiAgentDbContext : DbContext
{
    //public DbSet<Blog> Blogs => Set<Blog>();
    //public DbSet<Post> Posts => Set<Post>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=MyEfAppDb;Trusted_Connection=True;");
}
