using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly IMapper _mapper;
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;
        public AccountController(IMapper mapper, DataContext context, ITokenService tokenService)
        {
            _tokenService = tokenService;
            _context = context;
            _mapper = mapper;
            
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register([FromBody]RegisterDto registerDto) 
        {
            using var hmac = new HMACSHA512();

            if(await UserExists(registerDto.Username)) return BadRequest("Username is taken");

            var user = _mapper.Map<AppUser>(registerDto);
            user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
            user.PasswordSalt = hmac.Key;

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var token = _tokenService.CreateToken(user);
            return new UserDto 
            {
                Username = user.UserName,
                Token = token
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(user => user.UserName == loginDto.Username);

            if (user == null) return Unauthorized("User does not exists");

            using var hmac = new HMACSHA512(user.PasswordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            for (int i = 0; i < user.PasswordHash.Length; i++)
            {
                if(computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid password");
            }

            var token = _tokenService.CreateToken(user);
            return new UserDto 
            {
                Username = user.UserName,
                Token = token
            };
        }

        private async Task<bool> UserExists(string username) 
        {
            return await _context.Users.AnyAsync(user => user.UserName == username);
        }
    }
}