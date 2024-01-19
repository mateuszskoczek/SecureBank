using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecureBank.Database
{
    public partial class Account
    {
        #region PROPERTIES

        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }

        [Required]
        [MaxLength(300)]
        public string Email { get; set; }

        [Required]
        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        [Required]
        public byte LoginFailedCount { get; set; } = 0;

        [Required]
        public bool TemporaryPassword { get; set; } = true;

        [MaxLength(1000)]
        public string? LockReason { get; set; } = null;

        #endregion



        #region NAVIGATION

        public virtual ICollection<AccountPassword> AccountPasswords { get; set; }

        #endregion
    }
}
