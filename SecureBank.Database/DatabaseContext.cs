using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecureBank.Database
{
    public class DatabaseContext : DbContext
    {
        #region CONSTRUCTORS

        public DatabaseContext() { }

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        #endregion



        #region PROPERTIES

        public virtual DbSet<Account> Accounts { get; set; }
        public virtual DbSet<AccountPassword> AccountPasswords { get; set; }
        public virtual DbSet<AccountPasswordIndex> AccountPasswordIndexes { get; set; }
        public virtual DbSet<AccountLoginRequest> AccountLoginRequests { get; set; }
        public virtual DbSet<Transfer> Transfers { get; set; }

        #endregion



        #region METHODS

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlite("name=Default");

        #endregion
    }
}
