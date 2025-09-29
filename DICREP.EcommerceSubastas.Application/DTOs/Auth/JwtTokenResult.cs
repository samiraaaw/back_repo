using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DICREP.EcommerceSubastas.Application.DTOs.Auth
{
    public class JwtTokenResult
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
    }

}
