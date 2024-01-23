using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecureBank.API.Encryption
{
    public class EncryptionConfiguration
    {
        #region PROPERTIES

        public byte[] Key { get; private set; }
        public byte[] IV { get; private set; }

        #endregion



        #region CONSTRUCTORS

        public EncryptionConfiguration(IConfiguration configuration)
        {
            Key = Encoding.UTF8.GetBytes(configuration.GetSection("Encryption")["Key"]);
            IV = Encoding.UTF8.GetBytes(configuration.GetSection("Encryption")["IV"]);
        }

        #endregion
    }
}
