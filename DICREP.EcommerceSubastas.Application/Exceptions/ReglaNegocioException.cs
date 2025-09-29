using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DICREP.EcommerceSubastas.Application.Exceptions
{
    public class ReglaNegocioException : Exception
    {
        public int ErrorCode { get; }
        public ReglaNegocioException(string message, int errorCode = 0) : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}
