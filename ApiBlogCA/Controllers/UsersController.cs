﻿using Domain.Constants;
using Domain.DomainServices;
using Domain.Models;
using Domain.Models.Dtos;
using Domain.Repository.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiBlogCA.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class UsersController : ControllerBase
    {
        // GET: api/v1/Users
        [HttpGet]
        [Authorize(Roles = Roles.Admin)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<User>>> GetUsersAsync([FromServices] UsersHandler handler) {
            return Ok(await handler.GetAllUsersAsync());
        }

        // GET: api/v1/Users/5
        [HttpGet("{id}")]
        [Authorize(Roles = Roles.Admin)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<User>> GetUser([FromServices] UsersHandler handler, int id) {
            var user = await handler.GetUserCompleteAsync(id);
            if (user == null)
                return NotFound();
            return Ok(user);
        }
        // GET: api/v1/Users/confirm/{id}/{token}
        [HttpGet("confirm/{id}/{token}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<string>> GetConfirmUser([FromServices] UsersHandler handler, int id, string token) {
            var response = await handler.ConfirmUserAsync(id, token);
            return Ok(response);
        }


        // DELETE: api/v1/Users/5
        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.Admin)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteUser([FromServices] UsersHandler handler, int id) {
            await handler.DeleteUserAsync(id);
            return NoContent();
        }

        // PUT: api/v1/Users/5
        [HttpPut("{id}")]
        [Authorize(Roles = Roles.Admin)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PutUser([FromServices] UsersHandler handler, int id, User user) {
            if (id != user.UserId)
                return BadRequest();
            await handler.UpdateUserAsync(id, user);
            return NoContent();
        }

        // PUT: api/v1/Users/blocked/5
        [HttpPut("blocked/{id}")]
        [Authorize(Roles = Roles.Admin)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> PutUserBlocked([FromServices] UsersHandler handler, int id) {
            var user = await handler.UpdateUserBlockedAsync(id);
            if (user == null)
                return NotFound();
            return NoContent();
        }

        // PUT: api/v1/Users/Pending/5
        [HttpPut("Pending/{id}")]
        [Authorize(Roles = Roles.Admin)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutUserPending([FromServices] UsersHandler handler, int id) {
            var user = await handler.UpdateUserPendingAsync(id);
            if (user == null)
                return NotFound();
            return NoContent();
        }

        // PUT: api/v1/Users/Active/5
        [HttpPut("Active/{id}")]
        [Authorize(Roles = Roles.Admin)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutUserActive([FromServices] UsersHandler handler, int id) {
            var user = await handler.UpdateUserActiveAsync(id);
            if (user == null)
                return NotFound();
            return NoContent();
        }


        // POST: api/Register
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult> PostUser([FromServices] UsersHandler handler, UserDto userDto) {
            if (userDto == null)       
                return NotFound(); 
            var host = HttpContext.Request.Host.ToString(); 
            string response = await handler.RegisterUserAsync(userDto, host);
            if (response == string.Empty)
                return BadRequest();
            if (response == ErrorMessage.UserExists)       // verifica si usuario esta activo
                return ValidationProblem(response);
            if (response == ErrorMessage.UserBlocked)       // verifica si usuario esta bloqueado
                return ValidationProblem(response);

            return Created("GetUser", new { message = response });
        }

        // POST: api/ChangePassword
        [HttpPost("ChangePassword")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> PostChangePassword([FromServices] UsersHandler handler, ChangePassword changePassword) {
            if (changePassword == null)
                return BadRequest();
            var response = await handler.ChangeUserPasswordAsync(changePassword);
            if(response == ErrorMessage.EmailOrPassword)
                return ValidationProblem(response);
            return Ok(new { message = response });
        }

        // POST: api/ResetPassword
        [HttpPost("ResetPassword")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> PostResetPassword([FromServices] UsersHandler handler, ResetPassword resetPassword) {
            if (resetPassword == null)
                return BadRequest();
            var host = HttpContext.Request.Host.ToString();
            var response = await handler.ResetUserPasswordAsync(resetPassword, host);
            if (response == ErrorMessage.UserNotLogin)
                return ValidationProblem(response);
            return Ok(new { message = response });
        }

        // POST: api/RegisterAdmin
        [HttpPost("registerAdmin")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> PostUserAdmin([FromServices] UsersHandler handler, UserDto userDto) {
            if (userDto == null)
                return BadRequest();
            var response = await handler.RegisterAdminAsync(userDto);
            if (response == ErrorMessage.UserExists)
                return ValidationProblem(response);
            return Created("GetUser", new { id = response });
        }

        // POST: api/Login
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> PostUserAsync([FromServices] UsersHandler handler, Login login) {
            if (login == null)
                return BadRequest();
            var response = await handler.LoginUserAsync(login);
            if (response == ErrorMessage.UserNotLogin || response == ErrorMessage.UserBlocked || response == ErrorMessage.ResetPassword || response == ErrorMessage.UserPending)
                return ValidationProblem(response);
            return Ok(new { token = response });
        }

        ///////////////////////////// pruebas
        [HttpGet]
        [Route("authenticated")]
        [Authorize]
        public string Authenticated() => String.Format("Authenticated - {0}, Admin:{1}", User.Identity.Name, User.IsInRole(Roles.Admin));

        [HttpGet]
        [Route("testUser")]
        [Authorize(Roles = Roles.User)]
        public string TestUser() => "You are a User ";

        [HttpGet]
        [Route("testAdmin")]
        [Authorize(Roles = Roles.Admin)]
        public string TestAdmin() => "You are a Admin";
    }
}
