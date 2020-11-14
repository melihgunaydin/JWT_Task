using JWTExample.Models.Dtos;
using JWTExample.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JWTExample.Services.Abstract
{
    public interface IUserService
    {
        AuthenticationResponseDto Authenticate(AuthenticationRequestDto model);
        AuthenticationResponseDto RefreshToken(string token);
        bool RevokeToken(string token);

    }
}
