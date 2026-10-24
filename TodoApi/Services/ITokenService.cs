using TodoApi.Models;
using TodoApi.Models.Dtos;

namespace TodoApi.Services;

public interface ITokenService
{
    LoginResponse CreateToken(User user);
}
