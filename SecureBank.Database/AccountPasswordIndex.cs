using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecureBank.Database
{
    public class AccountPasswordIndex
    {
        #region PROPERTIES

        [Key]
        public long Id { get; set; }

        [Required]
        [ForeignKey(nameof(AccountPassword))]
        public long AccountPasswordId { get; set; }

        [Required]
        public byte Index { get; set; }

        #endregion



        #region NAVIGATION

        public AccountPassword AccountPassword { get; set; }

        #endregion
    }
}
