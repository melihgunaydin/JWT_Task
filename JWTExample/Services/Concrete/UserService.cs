using JWTExample.Helpers;
using JWTExample.Models.Dtos;
using JWTExample.Models.Entities;
using JWTExample.Services.Abstract;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace JWTExample.Services.Concrete
{
    public class UserService : IUserService
    {
        private readonly AppSettings _appSettings;
        private DataContext _context;


        public UserService(IOptions<AppSettings> appSettings, DataContext context)
        {
            _appSettings = appSettings.Value;
            _context = context;
        }

        public AuthenticationResponseDto Authenticate(AuthenticationRequestDto model)
        {
            var user = _context.Users.SingleOrDefault(x => x.Username == model.UserName && x.Password == model.Password);

            // return null if user not found
            if (user == null) return null;

            // authentication successful so generate jwt and refresh tokens
            var jwtToken = generateJwtToken(user);
            var refreshToken = generateRefreshToken();
            RefreshToken _refreshToken = new RefreshToken();
            _refreshToken = refreshToken;
            _refreshToken.UserId = user.Id;

            // save refresh token
            _context.Add(refreshToken);
            _context.SaveChanges();

            return new AuthenticationResponseDto(user, jwtToken, refreshToken.Token);
        }


        public AuthenticationResponseDto RefreshToken(string token)
        {
            var refreshToken = _context.RefreshTokens.SingleOrDefault(u => u.Token == token);
            var user = _context.Users.SingleOrDefault(u => u.Id == refreshToken.UserId);
            // return null if no user found with token
            if (user == null) return null;


            // return null if token is no longer active
            if (!refreshToken.IsActive) return null;

            // replace old refresh token with a new one and save
            var newRefreshToken = generateRefreshToken();
            newRefreshToken.UserId = user.Id;
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.ReplacedByToken = newRefreshToken.Token;

            _context.Add(newRefreshToken);
            _context.SaveChanges();

            _context.Update(refreshToken);
            _context.SaveChanges();

            // generate new jwt
            var jwtToken = generateJwtToken(user);

            return new AuthenticationResponseDto(user, jwtToken, newRefreshToken.Token);
        }

        public bool RevokeToken(string token)
        {
            var refreshToken = _context.RefreshTokens.SingleOrDefault(u => u.Token == token);
            var user = _context.Users.SingleOrDefault(u => u.Id == refreshToken.UserId);

            // return false if no user found with token
            if (user == null) return false;

            

            // return false if token is not active
            if (!refreshToken.IsActive) return false;

            // revoke token and save
            refreshToken.Revoked = DateTime.UtcNow;
            _context.Update(refreshToken);
            _context.SaveChanges();

            return true;
        }

        private string generateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private RefreshToken generateRefreshToken()
        {

            using (var rngCryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                var randomBytes = new byte[64];
                rngCryptoServiceProvider.GetBytes(randomBytes);
                return new RefreshToken
                {
                    Token = Convert.ToBase64String(randomBytes),
                    Expires = DateTime.UtcNow.AddMinutes(15),
                    Created = DateTime.UtcNow
                };
            }
        }
    }
}
