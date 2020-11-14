using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JWTExample.Models.Dtos
{
    public class AuthenticationRequestDto
    {
        public string UserName { get; set; }

        public string Password { get; set; }
    }
}
