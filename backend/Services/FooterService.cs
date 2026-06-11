using Microsoft.EntityFrameworkCore;
using Mblog.API.Data;
using Mblog.API.Models.DTOs;
using Mblog.API.Models.Entities;

namespace Mblog.API.Services;

public interface IFooterService
{
    Task<FooterConfigResponse?> GetAsync();
    Task<FooterConfigResponse> SaveAsync(UpdateFooterConfigRequest request);
}

public class FooterService : IFooterService
{
    private readonly AppDbContext _db;

    public FooterService(AppDbContext db) => _db = db;

    public async Task<FooterConfigResponse?> GetAsync()
    {
        var config = await _db.FooterConfigs.FirstOrDefaultAsync();
        if (config == null) return null;

        return new FooterConfigResponse
        {
            Id = config.Id,
            Copyright = config.Copyright,
            IcpNumber = config.IcpNumber,
            IcpUrl = config.IcpUrl,
            PoliceNumber = config.PoliceNumber,
            PoliceUrl = config.PoliceUrl,
            SocialLinks = config.SocialLinks,
            Blocks = config.Blocks,
            Style = config.Style,
            IsVisible = config.IsVisible,
            ContactEmail = config.ContactEmail,
            ContactWeChat = config.ContactWeChat,
            ContactPhone = config.ContactPhone
        };
    }

    public async Task<FooterConfigResponse> SaveAsync(UpdateFooterConfigRequest request)
    {
        var config = await _db.FooterConfigs.FirstOrDefaultAsync();
        if (config == null)
        {
            config = new FooterConfig();
            _db.FooterConfigs.Add(config);
        }

        if (request.Copyright != null) config.Copyright = request.Copyright;
        if (request.IcpNumber != null) config.IcpNumber = request.IcpNumber;
        if (request.IcpUrl != null) config.IcpUrl = request.IcpUrl;
        if (request.PoliceNumber != null) config.PoliceNumber = request.PoliceNumber;
        if (request.PoliceUrl != null) config.PoliceUrl = request.PoliceUrl;
        if (request.SocialLinks != null) config.SocialLinks = request.SocialLinks;
        if (request.Blocks != null) config.Blocks = request.Blocks;
        if (request.Style != null) config.Style = request.Style;
        if (request.IsVisible.HasValue) config.IsVisible = request.IsVisible.Value;
        if (request.ContactEmail != null) config.ContactEmail = request.ContactEmail;
        if (request.ContactWeChat != null) config.ContactWeChat = request.ContactWeChat;
        if (request.ContactPhone != null) config.ContactPhone = request.ContactPhone;

        await _db.SaveChangesAsync();

        return (await GetAsync())!;
    }
}