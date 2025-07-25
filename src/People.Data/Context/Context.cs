using Microsoft.EntityFrameworkCore;
using People.Data.Entities;

namespace People.Data.Context
{
    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> options) : base(options) { }

        public virtual DbSet<Person> MyEntities { get; set; }
    }
}
