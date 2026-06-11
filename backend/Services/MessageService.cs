using Microsoft.EntityFrameworkCore;
using Mblog.API.Common;
using Mblog.API.Data;
using Mblog.API.Models.DTOs;
using Mblog.API.Models.Entities;

namespace Mblog.API.Services;

public interface IMessageService
{
    Task<MessageResponse> CreateAsync(CreateMessageRequest request, string ipAddress, string? userAgent);
    Task<PagedResponse<MessageResponse>> GetApprovedAsync(int page, int pageSize);
    Task<PagedResponse<MessageFlatResponse>> GetAdminListAsync(AdminMessageQueryRequest query);
    Task<MessageResponse> GetAdminDetailAsync(Guid id);
    Task<MessageResponse> AdminUpdateAsync(Guid id, AdminUpdateMessageRequest request);
    Task<bool> AdminDeleteAsync(Guid id);
}

public class MessageService : IMessageService
{
    private readonly AppDbContext _db;

    public MessageService(AppDbContext db) => _db = db;

    /// <summary>
    /// 将 Message 实体递归转换为 MessageResponse DTO（子消息也递归转换）
    /// </summary>
    private static MessageResponse MapTree(Message m, Dictionary<Guid, List<Message>>? childrenMap = null)
    {
        if (childrenMap != null)
        {
            var children = childrenMap.TryGetValue(m.Id, out var childList)
                ? childList.OrderBy(c => c.CreatedAt).Select(c => MapTree(c, childrenMap)).ToList()
                : new List<MessageResponse>();

            return new MessageResponse
            {
                Id = m.Id,
                ParentId = m.ParentId,
                Nickname = m.Nickname,
                Email = m.Email,
                Content = m.Content,
                IpCity = m.IpCity,
                Status = m.Status,
                AdminReply = m.AdminReply,
                AdminRepliedAt = m.AdminRepliedAt,
                CreatedAt = m.CreatedAt,
                Children = children,
            };
        }

        return new MessageResponse
        {
            Id = m.Id,
            ParentId = m.ParentId,
            Nickname = m.Nickname,
            Email = m.Email,
            Content = m.Content,
            IpCity = m.IpCity,
            Status = m.Status,
            AdminReply = m.AdminReply,
            AdminRepliedAt = m.AdminRepliedAt,
            CreatedAt = m.CreatedAt,
            Children = new List<MessageResponse>(),
        };
    }

    /// <summary>
    /// 递归加载指定父 ID 集合的所有后代消息，并构建 parent→children 映射
    /// </summary>
    private async Task<Dictionary<Guid, List<Message>>> LoadDescendantsAsync(HashSet<Guid> parentIds, Func<IQueryable<Message>, IQueryable<Message>>? filter = null)
    {
        var map = new Dictionary<Guid, List<Message>>();
        var currentLevel = parentIds.ToList();

        while (currentLevel.Count > 0)
        {
            var query = _db.Messages.Where(m => currentLevel.Contains(m.ParentId!.Value));
            if (filter != null)
                query = filter(query);

            var children = await query.ToListAsync();
            if (children.Count == 0) break;

            var nextLevel = new List<Guid>();
            foreach (var child in children)
            {
                if (!map.ContainsKey(child.ParentId!.Value))
                    map[child.ParentId!.Value] = new List<Message>();
                map[child.ParentId!.Value].Add(child);
                nextLevel.Add(child.Id);
            }

            currentLevel = nextLevel;
        }

        return map;
    }

    public async Task<MessageResponse> CreateAsync(CreateMessageRequest request, string ipAddress, string? userAgent)
    {
        var message = new Message
        {
            Nickname = request.Nickname.Trim(),
            Email = request.Email?.Trim(),
            Content = request.Content.Trim(),
            ParentId = request.ParentId,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Status = "pending",
            CreatedAt = DateTimeOffset.UtcNow,
        };

        _db.Messages.Add(message);
        await _db.SaveChangesAsync();

        return new MessageResponse
        {
            Id = message.Id,
            ParentId = message.ParentId,
            Nickname = message.Nickname,
            Email = message.Email,
            Content = message.Content,
            IpCity = message.IpCity,
            Status = message.Status,
            AdminReply = message.AdminReply,
            AdminRepliedAt = message.AdminRepliedAt,
            CreatedAt = message.CreatedAt,
            Children = new List<MessageResponse>(),
        };
    }

    public async Task<PagedResponse<MessageResponse>> GetApprovedAsync(int page, int pageSize)
    {
        var query = _db.Messages
            .Where(m => m.ParentId == null && m.Status == "approved")
            .OrderByDescending(m => m.CreatedAt);

        var total = await query.CountAsync();
        var roots = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        var rootIds = roots.Select(r => r.Id).ToHashSet();
        var childrenMap = await LoadDescendantsAsync(rootIds, q => q.Where(c => c.Status == "approved"));

        var items = roots.Select(r => MapTree(r, childrenMap)).ToList();

        return new PagedResponse<MessageResponse>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<PagedResponse<MessageFlatResponse>> GetAdminListAsync(AdminMessageQueryRequest query)
    {
        // 返回全部留言（含子回复），扁平列表，每行带父留言者信息
        var q = _db.Messages.AsQueryable();

        if (!string.IsNullOrEmpty(query.Status))
            q = q.Where(m => m.Status == query.Status);
        if (!string.IsNullOrEmpty(query.Keyword))
            q = q.Where(m => m.Nickname.Contains(query.Keyword) || m.Content.Contains(query.Keyword));

        q = q.OrderByDescending(m => m.CreatedAt);

        var total = await q.CountAsync();
        var items = await q
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        // 收集所有父 ID
        var parentIds = items.Where(m => m.ParentId.HasValue).Select(m => m.ParentId!.Value).Distinct().ToList();
        var parents = parentIds.Count > 0
            ? await _db.Messages.Where(m => parentIds.Contains(m.Id)).ToDictionaryAsync(m => m.Id, m => m.Nickname)
            : new Dictionary<Guid, string>();

        var result = items.Select(m => new MessageFlatResponse
        {
            Id = m.Id,
            ParentId = m.ParentId,
            ParentNickname = m.ParentId.HasValue ? parents.GetValueOrDefault(m.ParentId!.Value, "已删除") : null,
            Nickname = m.Nickname,
            Email = m.Email,
            Content = m.Content,
            IpCity = m.IpCity,
            Status = m.Status,
            AdminReply = m.AdminReply,
            AdminRepliedAt = m.AdminRepliedAt,
            CreatedAt = m.CreatedAt,
        }).ToList();

        return new PagedResponse<MessageFlatResponse>
        {
            Items = result,
            TotalCount = total,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<MessageResponse> GetAdminDetailAsync(Guid id)
    {
        var roots = await _db.Messages.Where(m => m.Id == id).ToListAsync();
        var rootIds = new HashSet<Guid> { id };
        var childrenMap = await LoadDescendantsAsync(rootIds);
        return MapTree(roots.First(), childrenMap);
    }

    public async Task<MessageResponse> AdminUpdateAsync(Guid id, AdminUpdateMessageRequest request)
    {
        var message = await _db.Messages.FirstOrDefaultAsync(m => m.Id == id);
        if (message == null)
            throw new KeyNotFoundException($"Message {id} not found");

        if (!string.IsNullOrEmpty(request.Status))
            message.Status = request.Status;

        if (!string.IsNullOrEmpty(request.AdminReply))
        {
            message.AdminReply = request.AdminReply;
            message.AdminRepliedAt = DateTimeOffset.UtcNow;
        }

        await _db.SaveChangesAsync();

        var rootIds = new HashSet<Guid>
        {
            message.ParentId ?? message.Id
        };
        var childrenMap = await LoadDescendantsAsync(rootIds);
        return MapTree(message.ParentId.HasValue
            ? (await _db.Messages.FirstAsync(m => m.Id == message.ParentId))
            : message, childrenMap);
    }

    public async Task<bool> AdminDeleteAsync(Guid id)
    {
        var allIds = new List<Guid> { id };
        var toProcess = new Queue<Guid>([id]);

        while (toProcess.Count > 0)
        {
            var parentId = toProcess.Dequeue();
            var children = await _db.Messages.Where(m => m.ParentId == parentId).ToListAsync();
            foreach (var child in children)
            {
                allIds.Add(child.Id);
                toProcess.Enqueue(child.Id);
            }
        }

        var messages = await _db.Messages.Where(m => allIds.Contains(m.Id)).ToListAsync();
        if (messages.Count == 0) return false;

        _db.Messages.RemoveRange(messages);
        await _db.SaveChangesAsync();
        return true;
    }
}