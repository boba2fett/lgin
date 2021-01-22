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
using WebApi.Models.Groups;
using WebApi.Models.Users;

namespace WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class GroupsController : ControllerBase
    {
        private IGroupService _groupService;
        private IMapper _mapper;
        private readonly AppSettings _appSettings;

        public GroupsController(
            IGroupService groupService,
            IMapper mapper,
            IOptions<AppSettings> appSettings)
        {
            _groupService = groupService;
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

        [HttpPost("add")]
        public IActionResult Register([FromBody]AddGroupModel model)
        {
            // map model to entity
            var group = _mapper.Map<Group>(model);

            try
            {
                // create group
                _groupService.Create(group, LoggedInUser);
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
            Console.WriteLine(LoggedInUser);
            var groups = _groupService.GetAll(LoggedInUser);
            var model = _mapper.Map<IList<GroupModel>>(groups);
            return Ok(model);
        }

        [HttpGet("{id}/members")]
        public IActionResult GetMember(int id)
        {
            Console.WriteLine(LoggedInUser);
            var users = _groupService.GetMember(id, LoggedInUser);
            var model = _mapper.Map<IList<UserModel>>(users);
            return Ok(model);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var group = _groupService.GetById(id,LoggedInUser);
            var model = _mapper.Map<GroupModel>(group);
            return Ok(model);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody]UpdateGroupModel model)
        {
            // map model to entity and set id
            var group = _mapper.Map<Group>(model);
            group.Id = id;

            try
            {
                // update group 
                _groupService.Update(group, LoggedInUser);
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
            _groupService.Delete(id, LoggedInUser);
            return Ok();
        }
    }
}
