using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Transaction;
using System.Security.Claims;
using UserService.Models;
using UserService.Models.Dtos;
using UserService.Services;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IMapper _mapper;

        public UsersController(IAuthService authService,IMapper mapper)
        {
            _authService = authService;
            _mapper = mapper;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _authService.RegisterAsync(dto);
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            return Ok(result);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            var result = await _authService.RefreshTokenAsync(refreshToken);
            return Ok(result);
        }
    
    [HttpGet("{id}")]
        [Authorize(Roles = "Merchant,User")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _authService.GetByIdAsync(id);
            if (user == null) return NotFound("User not found");

            return Ok(user);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Merchant")]
        public async Task<IActionResult> GetAll()
        {
            var users = await _authService.GetAllAsync();

            return Ok(users);
        }

        [HttpPost("bulk")]
        public async Task<ActionResult<List<UserDto>>> GetUsersByIds([FromBody] List<int> userIds)
        {
            if (userIds == null || !userIds.Any())
                return BadRequest("User IDs are required");

            var users = await _authService.GetUsersByIdsAsync(userIds);
            var userDtos = _mapper.Map <List<UserDto>>(users);

            return Ok(userDtos);
        }

        [HttpPut("{id}/role")]
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateUserRoleDto dto)
        {
            try
            {
                await _authService.UpdateUserRoleAsync(id, dto.Role);
                return NoContent();
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
        [HttpPost("transfer")]
        //[Authorize(Roles = "Merchant,User")]
        public async Task<IActionResult> Transfer([FromBody] TransferRequestDto dto)
        {
            try
            {
                await _authService.TransferAsync(dto);
                return Ok("Transfer successful");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Unexpected error: {ex.Message}");
            }
        }

        [HttpPost("account-to-id")]
        public async Task<ActionResult<List<SharedLibrary.Transaction.AccountNumberToIdDto>>> GetUserIdsFromAccountNumbers([FromBody] List<string> accountNumbers)
        {
            if (accountNumbers == null || !accountNumbers.Any())
                return BadRequest("Account numbers list cannot be empty.");

            var result = await _authService.GetUserIdsFromAccountNumbersAsync(accountNumbers);
            return Ok(result);
        }

    }
}
