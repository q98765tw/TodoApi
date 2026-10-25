namespace TodoApiGrokBot.Services;

/// <summary>台北時間 helper：固定 UTC+8（台灣無日光節約，不依賴容器內 tzdata）。</summary>
public static class TaipeiTime
{
    public static readonly TimeSpan Offset = TimeSpan.FromHours(8);

    public static DateTime Now(TimeProvider clock) => clock.GetUtcNow().UtcDateTime + Offset;

    public static DateOnly Today(TimeProvider clock) => DateOnly.FromDateTime(Now(clock));

    /// <summary>台北當地時間（Kind 不重要）轉成對應的 UTC 時間。</summary>
    public static DateTime ToUtc(DateTime taipeiLocal) => DateTime.SpecifyKind(taipeiLocal - Offset, DateTimeKind.Utc);
}
