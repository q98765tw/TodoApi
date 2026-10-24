namespace TodoApi.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";
    public const int MinKeyBytes = 32;

    public string Key { get; set; } = string.Empty;
    public int ExpiryDays { get; set; } = 7;
}
