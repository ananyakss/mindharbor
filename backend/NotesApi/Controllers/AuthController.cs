using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NotesApi.Models;
using NotesApi.Options;
using NotesApi.Services;

namespace NotesApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserService _userService;
    private readonly AuthService _authService;
    private readonly JwtOptions _jwtOptions;
    private readonly bool _isDevelopment;

    public AuthController(
        UserService userService,
        AuthService authService,
        IOptions<JwtOptions> jwtOptions,
        IWebHostEnvironment environment)
    {
        _userService = userService;
        _authService = authService;
        _jwtOptions = jwtOptions.Value;
        _isDevelopment = environment.IsDevelopment();
    }

    [HttpPost("signup")]
    public async Task<IActionResult> SignUp([FromBody] SignUpRequest request)
    {
        var existingEmail = await _userService.GetByEmailAsync(request.Email);
        if (existingEmail is not null)
        {
            return Conflict(new { message = "Email is already registered." });
        }

        var existingUsername = await _userService.GetByUsernameAsync(request.Username);
        if (existingUsername is not null)
        {
            return Conflict(new { message = "Username is already taken." });
        }

        var newUser = new User
        {
            Username = request.Username.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            HashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        await _userService.CreateAsync(newUser);

        await _userService.RecordLoginAsync(newUser.Id!);
        newUser = (await _userService.GetByIdAsync(newUser.Id!))!;

        var accessToken = _authService.GenerateAccessToken(newUser);
        var refreshToken = _authService.CreateRefreshToken(newUser.Id!);
        await _userService.RemoveRefreshTokensForUserAsync(newUser.Id!);
        await _userService.SaveRefreshTokenAsync(refreshToken);

        SetRefreshCookie(refreshToken);

        return Ok(new
        {
            accessToken,
            user = new UserResponse(
                newUser.Id!,
                newUser.Username,
                newUser.Email,
                newUser.LoginStreak,
                newUser.GamificationPoints)
        });
    }

    [HttpPost("signin")]
    public async Task<IActionResult> SignIn([FromBody] SignInRequest request)
    {
        var user = await _userService.GetByEmailAsync(request.Email);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.HashedPassword))
        {
            await Task.Delay(Random.Shared.Next(50, 150));
            return Unauthorized(new { message = "Invalid credentials." });
        }

        await _userService.RecordLoginAsync(user.Id!);
        user = (await _userService.GetByIdAsync(user.Id!))!;

        var accessToken = _authService.GenerateAccessToken(user);
        var refreshToken = _authService.CreateRefreshToken(user.Id!);

        await _userService.RemoveRefreshTokensForUserAsync(user.Id!);
        await _userService.SaveRefreshTokenAsync(refreshToken);

        SetRefreshCookie(refreshToken);

        return Ok(new
        {
            accessToken,
            user = new UserResponse(
                user.Id!,
                user.Username,
                user.Email,
                user.LoginStreak,
                user.GamificationPoints)
        });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        if (!Request.Cookies.TryGetValue(_jwtOptions.RefreshTokenCookieName, out var token) || string.IsNullOrWhiteSpace(token))
        {
            return Unauthorized(new { message = "Refresh token missing." });
        }

        var existingToken = await _userService.GetRefreshTokenAsync(token);
        if (existingToken is null || existingToken.ExpiresAt <= DateTime.UtcNow || existingToken.RevokedAt is not null)
        {
            await _userService.DeleteRefreshTokenAsync(token);
            Response.Cookies.Delete(_jwtOptions.RefreshTokenCookieName);
            return Unauthorized(new { message = "Refresh token invalid." });
        }

        var user = await _userService.GetByIdAsync(existingToken.UserId);
        if (user is null)
        {
            Response.Cookies.Delete(_jwtOptions.RefreshTokenCookieName);
            return Unauthorized(new { message = "User not found." });
        }

        var newAccessToken = _authService.GenerateAccessToken(user);
        var newRefreshToken = _authService.CreateRefreshToken(user.Id!);

        existingToken.RevokedAt = DateTime.UtcNow;
        existingToken.ReplacedByToken = newRefreshToken.Token;
        await _userService.UpdateRefreshTokenAsync(existingToken);
        await _userService.SaveRefreshTokenAsync(newRefreshToken);

        SetRefreshCookie(newRefreshToken);

        return Ok(new
        {
            accessToken = newAccessToken,
            user = new UserResponse(
                user.Id!,
                user.Username,
                user.Email,
                user.LoginStreak,
                user.GamificationPoints)
        });
    }

    [HttpPost("signout")]
    public async Task<IActionResult> SignOutUser()
    {
        if (Request.Cookies.TryGetValue(_jwtOptions.RefreshTokenCookieName, out var token) && !string.IsNullOrWhiteSpace(token))
        {
            await _userService.DeleteRefreshTokenAsync(token);
        }

        Response.Cookies.Delete(_jwtOptions.RefreshTokenCookieName);
        return NoContent();
    }

    private void SetRefreshCookie(RefreshToken refreshToken)
    {
        var cookieOptions = _authService.BuildRefreshCookieOptions(refreshToken.ExpiresAt, _isDevelopment);
        Response.Cookies.Append(_jwtOptions.RefreshTokenCookieName, refreshToken.Token, cookieOptions);
    }

    public record SignUpRequest(string Username, string Email, string Password);

    public record SignInRequest(string Email, string Password);

    public record UserResponse(string Id, string Username, string Email, int LoginStreak, int GamificationPoints);
}

