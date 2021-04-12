using AutoMapper;
using HotelListing.Data;
using HotelListing.Models;
using HotelListing.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HotelListing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApiUser> _userManager;
        //private readonly SignInManager<ApiUser> _signInManager;
        private readonly ILogger<AccountController> _logger;
        private readonly IMapper _mapper;
        private readonly IAuthManager _authManager;

        public AccountController(UserManager<ApiUser> userManager, /*SignInManager<ApiUser> signInManager, */
            ILogger<AccountController> logger, IMapper mapper, IAuthManager authManager)
        {
            _userManager = userManager;
            //_signInManager = signInManager;
            _logger = logger;
            _mapper = mapper;
            _authManager = authManager;
        }

        [HttpPost]
        [Route("register")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public async Task<IActionResult> Register([FromBody] UserDTO userDTO)
        {
            var location = GetControllerActionNames();

            _logger.LogInformation($"Registration attempt for {userDTO.Email}");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var user = _mapper.Map<ApiUser>(userDTO);
                user.UserName = user.Email;
                var result = await _userManager.CreateAsync(user, userDTO.Password);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        _logger.LogError($"{location}: {error.Code} {error.Description}");
                        ModelState.AddModelError(error.Code, error.Description);
                    }

                    return BadRequest($"{location}: {userDTO.Email} User registration attempt failed");
                }

                await _userManager.AddToRolesAsync(user, userDTO.Roles);

                return Accepted();
            }
            catch (Exception ex)
            {
                return InternalError($"{location}: Error", ex);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("login")]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public async Task<IActionResult> Login([FromBody] LoginUserDTO userDTO)
        {
            var location = GetControllerActionNames();

            _logger.LogInformation($"{location}: Login attempt from user {userDTO.Email}");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                if (!await _authManager.ValidateUser(userDTO))
                    return Unauthorized("Invalid user name or password");

                return Accepted(new { Token = await _authManager.CreateToken() });
            }
            catch (Exception ex)
            {
                return InternalError($"{location}: Error", ex);
            }
        }

        private ObjectResult InternalError(string message, Exception ex = null)
        {
            if (ex != null)
            {
                _logger.LogError(ex, message);
                return StatusCode(500, ex.Message);
            }
            else
            {
                _logger.LogError(message);
                return StatusCode(500, message);
            }
        }

        private string GetControllerActionNames()
        {
            var controller = ControllerContext.ActionDescriptor.ControllerName;
            var action = ControllerContext.ActionDescriptor.ActionName;

            return $"{controller} - {action}";
        }
    }
}
