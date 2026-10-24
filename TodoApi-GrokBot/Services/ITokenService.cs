using TodoApiGrokBot.Models;
using TodoApiGrokBot.Models.Dtos;

namespace TodoApiGrokBot.Services;

public interface ITokenService
{
    LoginResponse CreateToken(User user);
}
