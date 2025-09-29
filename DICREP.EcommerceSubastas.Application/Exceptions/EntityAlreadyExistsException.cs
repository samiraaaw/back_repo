using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DICREP.EcommerceSubastas.Application.Exceptions
{
    public class EntityAlreadyExistsException : Exception
    {
         public EntityAlreadyExistsException() { }
         public EntityAlreadyExistsException(string message) : base(message) { }
         public EntityAlreadyExistsException(string message, Exception innerException): base(message, innerException) { }
         public EntityAlreadyExistsException(string name, object key)
             : base($"Entity \"{name}\" with key ({key}) already exists.")
         { }

    }
}
