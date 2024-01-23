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
    public partial class Transfer
    {
        #region PROPERTIES

        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(26)]
        public string SenderAccountNumber { get; set; }

        [MaxLength(100)]
        public string? SenderName { get; set; }

        [MaxLength(100)]
        public string? SenderAddress { get; set; }

        [Required]
        [MaxLength(26)]
        public string ReceiverAccountNumber { get; set; }

        [MaxLength(100)]
        public string? ReceiverName { get; set; }

        [MaxLength(100)]
        public string? ReceiverAddress { get; set; }

        [Required]
        [Precision(14, 2)]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(200)]
        public string? Title { get; set; }

        [Required]
        public DateTime Date {  get; set; }

        #endregion
    }
}
