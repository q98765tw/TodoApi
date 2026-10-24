using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace TodoApiGrokBot.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal user) =>
        int.Parse(user.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);
}
