
using GrpcService2.Models;
using Microsoft.EntityFrameworkCore;

namespace GrpcService2.Data
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<User> Users { set; get; }
    }
}
