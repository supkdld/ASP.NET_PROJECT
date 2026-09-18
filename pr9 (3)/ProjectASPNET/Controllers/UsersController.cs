using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectASPNET.DTOs;
using ProjectASPNET.Mappers;
using ProjectASPNET.Models;

namespace ProjectASPNET.Controllers;

[ApiController]
[Route("api/users")]
[Produces("application/json")]
public class UsersController : ApiControllerBase
{
    private readonly PharmacyContext _context;

    public UsersController(PharmacyContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<UserDto>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _context.Users
            .AsNoTracking()
            .OrderBy(u => u.IdUser);

        return Ok(GetPage(query, page, pageSize).Select(UserMapper.ToDto).ToList());
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<UserDto> GetById(int id)
    {
        var user = _context.Users
            .AsNoTracking()
            .FirstOrDefault(u => u.IdUser == id);

        if (user == null)
        {
            return NotFound();
        }

        return Ok(UserMapper.ToDto(user));
    }

    [HttpPost]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public ActionResult<UserDto> Create(CreateUserDto dto)
    {
        if (!RoleExists(dto.RoleId))
        {
            return FieldError(nameof(dto.RoleId), $"Роль с Id = {dto.RoleId} не найдена.");
        }

        if (LoginExists(dto.Login))
        {
            return ConflictError($"Пользователь с логином '{dto.Login}' уже существует.");
        }

        var user = new User
        {
            Login = dto.Login,
            PasswordUser = dto.PasswordUser,
            RoleId = dto.RoleId
        };

        _context.Users.Add(user);
        _context.SaveChanges();

        var result = UserMapper.ToDto(user);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Update(int id, UpdateUserDto dto)
    {
        var user = _context.Users.Find(id);

        if (user == null)
        {
            return NotFound();
        }

        if (!RoleExists(dto.RoleId))
        {
            return FieldError(nameof(dto.RoleId), $"Роль с Id = {dto.RoleId} не найдена.");
        }

        if (LoginExists(dto.Login, id))
        {
            return ConflictError($"Пользователь с логином '{dto.Login}' уже существует.");
        }

        user.Login = dto.Login;
        user.PasswordUser = dto.PasswordUser;
        user.RoleId = dto.RoleId;

        _context.SaveChanges();

        return NoContent();
    }

    [HttpPatch("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Patch(int id, PatchUserDto dto)
    {
        var user = _context.Users.Find(id);

        if (user == null)
        {
            return NotFound();
        }

        if (dto.RoleId.HasValue)
        {
            if (!RoleExists(dto.RoleId.Value))
            {
                return FieldError(nameof(dto.RoleId), $"Роль с Id = {dto.RoleId.Value} не найдена.");
            }

            user.RoleId = dto.RoleId.Value;
        }

        if (dto.Login != null)
        {
            if (LoginExists(dto.Login, id))
            {
                return ConflictError($"Пользователь с логином '{dto.Login}' уже существует.");
            }

            user.Login = dto.Login;
        }

        if (dto.PasswordUser != null)
        {
            user.PasswordUser = dto.PasswordUser;
        }

        _context.SaveChanges();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Delete(int id)
    {
        var user = _context.Users.Find(id);

        if (user == null)
        {
            return NotFound();
        }

        if (_context.Carts.Any(c => c.UserId == id) || _context.Orders.Any(o => o.UserId == id))
        {
            return ConflictError("Нельзя удалить пользователя, у которого есть корзины или заказы.");
        }

        _context.Users.Remove(user);
        _context.SaveChanges();

        return NoContent();
    }

    private bool RoleExists(int roleId)
    {
        return _context.Roles.Any(r => r.IdRole == roleId);
    }

    private bool LoginExists(string login, int? excludeId = null)
    {
        return _context.Users
            .Any(u => u.Login == login && (excludeId == null || u.IdUser != excludeId.Value));
    }
}
