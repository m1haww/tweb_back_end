using BusinessLogic.Database;
using BusinessLogic.Interfaces;
using Domain.DTOs;
using Domain.Entities.App;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic.Services;

public class RevenuecatUserService : IRevenuecatUserService
{
    private readonly AppDbContext _db;

    public RevenuecatUserService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<AppUser?> SetUserAsync(AddRevenuecatUserDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.AppUserId))
            return null;

        var attrs = dto.SubscriberAttributes;
        var installDate = attrs?.ClickDate ?? DateTime.UtcNow;

        var existing = await _db.AppUsers
            .FirstOrDefaultAsync(u => u.AppId == dto.AppUserId, ct);

        if (existing != null)
        {
            existing.CountryCode = dto.CountryCode ?? string.Empty;
            existing.InstallDate = installDate;
            existing.CampaignId = attrs?.CampaignId;
            existing.KeywordId = attrs?.KeywordId;
            existing.AdGroupId = attrs?.AdGroupId;
            await _db.SaveChangesAsync(ct);
            return existing;
        }

        var appUser = new AppUser
        {
            Id = Guid.NewGuid(),
            AppId = dto.AppUserId,
            CountryCode = dto.CountryCode ?? string.Empty,
            InstallDate = installDate,
            TotalRevenue = 0,
            CampaignId = attrs?.CampaignId,
            KeywordId = attrs?.KeywordId,
            AdGroupId = attrs?.AdGroupId
        };

        _db.AppUsers.Add(appUser);
        await _db.SaveChangesAsync(ct);
        return appUser;
    }
}
