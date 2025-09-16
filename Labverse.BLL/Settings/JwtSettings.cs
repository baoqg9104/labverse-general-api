﻿namespace Labverse.BLL.Settings;

public class JwtSettings
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenExpireMinutes { get; set; } = 60;
    public int RefreshTokenExpireDays { get; set; } = 7;
}