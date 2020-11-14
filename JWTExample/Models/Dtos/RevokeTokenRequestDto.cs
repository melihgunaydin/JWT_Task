using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JWTExample.Models.Dtos
{
    public class RevokeTokenRequestDto
    {
        public string RefreshToken { get; set; }
    }
}
