using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DICREP.EcommerceSubastas.Application.DTOs.Responses
{
    public class ResponseDTO<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } 
        public T Data { get; set; }
        public ErrorResponseDto Error { get; set; }
    }

}
