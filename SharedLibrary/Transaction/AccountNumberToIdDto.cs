using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Transaction
{
    public class AccountNumberToIdDto
    {
        public string AccountNumber { get; set; } = string.Empty;
        public int UserId { get; set; }
    }
}
