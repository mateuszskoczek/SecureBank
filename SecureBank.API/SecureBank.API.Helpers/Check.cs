using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecureBank.API.Helpers
{
    public class Check<T>
    {
        public Predicate<T> CheckAction { get; set; }
        public string Message { get; set; }
    }
}
