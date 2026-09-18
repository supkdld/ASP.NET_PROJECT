using ProjectASPNET.DTOs;
using ProjectASPNET.Models;

namespace ProjectASPNET.Mappers;

public static class UserMapper
{
    public static UserDto ToDto(User user)
    {
        return new UserDto
        {
            Id = user.IdUser,
            Login = user.Login,
            RoleId = user.RoleId
        };
    }
}
