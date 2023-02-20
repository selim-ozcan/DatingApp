using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly DataContext _context;

        public UsersController(DataContext dataContext)
        {
            _context = dataContext;
            
        }

        [HttpGet()]
        public async Task<ActionResult<IEnumerable<AppUser>>> GetUsers() 
        {
            return await this._context.Users.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AppUser>> GetUser(int id) 
        {
            return await this._context.Users.FindAsync(id);
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<AppUser>> GetUserByUsername(string username) 
        {
            return await this._context.Users.SingleOrDefaultAsync(user => user.UserName.Equals(username));
        }

    }
}