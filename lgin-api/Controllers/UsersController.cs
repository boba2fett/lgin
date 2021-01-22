using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using System.IdentityModel.Tokens.Jwt;
using WebApi.Helpers;
using Microsoft.Extensions.Options;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using WebApi.Services;
using WebApi.Entities;
using WebApi.Models.Users;
using WebApi.Models.Groups;
using WebApi.Models.UserGroups;

namespace WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private IUserService _userService;
        private IMapper _mapper;
        private readonly AppSettings _appSettings;

        public UsersController(
            IUserService userService,
            IMapper mapper,
            IOptions<AppSettings> appSettings)
        {
            _userService = userService;
            _mapper = mapper;
            _appSettings = appSettings.Value;
        }

        public int LoggedInUser{
            get {
                int id=-1;
                Int32.TryParse(User.Identity.Name, out id);
                return id;
            }
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody]AuthenticateModel model)
        {
            var user = _userService.Authenticate(model.Username, model.Password);

            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            // return basic user info and authentication token
            return Ok(new
            {
                Id = user.Id,
                Username = user.Username,
                Token = tokenString
            });
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register([FromBody]RegisterModel model)
        {
            // map model to entity
            var user = _mapper.Map<User>(model);

            try
            {
                // create user
                _userService.Create(user, model.Password);
                return Ok();
            }
            catch (AppException ex)
            {
                // return error message if there was an exception
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _userService.GetAll(LoggedInUser);
            var model = _mapper.Map<IList<UserModel>>(users);
            return Ok(model);
        }

        [HttpGet("{id}/groups")]
        public IActionResult GetGroups(int id)
        {
            var groups = _userService.GetGroups(id, LoggedInUser);
            var model = _mapper.Map<IList<GroupModel>>(groups);
            return Ok(model);
        }

        [HttpGet("{id}/usergroups")]
        public IActionResult GetUserGroups(int id)
        {
            var ugroups = _userService.GetUserGroups(id, LoggedInUser);
            var model = _mapper.Map<IList<UserGroupModel>>(ugroups);
            return Ok(model);
        }

        [HttpPost("{id}/addGroup")]
        public IActionResult AddGroup(int id, [FromBody]AddUserGroupModel model)
        {
            _userService.AddGroup(id, model.GroupId, LoggedInUser);
            return Ok();
        }

        [HttpPost("{id}/addGroups")]
        public IActionResult AddGroups(int id, [FromBody]AddRmUserGroupsModel model)
        {
            _userService.AddGroups(id, model.GroupIds, LoggedInUser);
            return Ok();
        }

        [HttpPost("{id}/rmGroup")]
        public IActionResult RmGroup(int id, [FromBody]AddUserGroupModel model)
        {
            var userGroup = new UserGroup
            {
                UserId = id,
                GroupId = model.GroupId
            };
            _userService.RemoveGroup(userGroup, LoggedInUser);
            return Ok();
        }

        [HttpPost("{id}/rmGroups")]
        public IActionResult RmGroups(int id, [FromBody]AddRmUserGroupsModel model)
        {
            _userService.RemoveGroups(id, model.GroupIds, LoggedInUser);
            return Ok();
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var user = _userService.GetById(id,LoggedInUser);
            var model = _mapper.Map<UserModel>(user);
            return Ok(model);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody]UpdateModel model)
        {
            // map model to entity and set id
            var user = _mapper.Map<User>(model);
            user.Id = id;

            try
            {
                // update user 
                _userService.Update(user, LoggedInUser, model.Password);
                return Ok();
            }
            catch (AppException ex)
            {
                // return error message if there was an exception
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _userService.Delete(id, LoggedInUser);
            return Ok();
        }
    }
}
