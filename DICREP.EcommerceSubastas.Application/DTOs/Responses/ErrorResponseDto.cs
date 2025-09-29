using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DICREP.EcommerceSubastas.Application.DTOs.Responses
{
    public class ErrorResponseDto
    {
        public int ErrorCode {  get; set; }
        public string Message { get; set; } 
        public int? HttpStatusCode { get; set; }
    }
}
