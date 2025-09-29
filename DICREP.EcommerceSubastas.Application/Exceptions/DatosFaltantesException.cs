using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DICREP.EcommerceSubastas.Application.Exceptions
{
    public class DatosFaltantesException : Exception 
    {
        public DatosFaltantesException() { }
        public DatosFaltantesException(string message) : base(message) { }
        public DatosFaltantesException(string message, Exception innerException) : base(message, innerException) { }
        public DatosFaltantesException(string name, object key)
            : base($"Entity \"{name}\" with key ({key}) already exists.")
        { }
    }
}

