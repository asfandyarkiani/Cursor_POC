using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cache.ConfigModels
{
    public class KeyVaultResponse
    {
        public bool IsSuccess { get; set; } = false;
        public string Secret { get; set; } = string.Empty;
    }
}
