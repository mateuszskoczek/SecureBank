using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecureBank.Database
{
    public partial class AccountPassword
    {
        #region PROPERTIES

        [Key]
        public long Id { get; set; }

        [Required]
        [ForeignKey(nameof(Account))]
        public int AccountId { get; set; }

        [Required]
        [MaxLength(1000)]
        public byte[] Password { get; set; }

        [Required]
        [MaxLength(20)]
        public string LeftSalt { get; set; }

        [Required]
        [MaxLength(20)]
        public string RightSalt { get; set; }

        #endregion



        #region NAVIGATION

        public Account Account { get; set; }

        #endregion
    }
}
