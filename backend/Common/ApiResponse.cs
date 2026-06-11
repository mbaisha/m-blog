namespace Mblog.API.Common;

/// <summary>
/// 统一 API 响应结构体
/// </summary>
public class ApiResponse
{
    /// <summary>业务状态码，200 表示成功，其余表示失败</summary>
    public int Code { get; set; }

    /// <summary>提示信息，成功或失败的具体描述</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>错误详情列表</summary>
    public List<string>? Errors { get; set; }

    /// <summary>是否成功</summary>
    public bool Success => Code == 200;

    /// <summary>
    /// 返回成功响应（无数据）
    /// </summary>
    public static ApiResponse Ok(string message = "操作成功")
    {
        return new ApiResponse { Code = 200, Message = message };
    }

    /// <summary>
    /// 返回成功响应（含数据）
    /// </summary>
    public static ApiResponse<T> Ok<T>(T data, string message = "操作成功")
    {
        return new ApiResponse<T> { Code = 200, Message = message, Data = data };
    }

    /// <summary>
    /// 返回失败响应
    /// </summary>
    public static ApiResponse Fail(string message, int code = 400)
    {
        return new ApiResponse { Code = code, Message = message };
    }

    /// <summary>
    /// 返回失败响应（含错误详情）
    /// </summary>
    public static ApiResponse Fail(string message, List<string> errors, int code = 400)
    {
        return new ApiResponse { Code = code, Message = message, Errors = errors };
    }
}

/// <summary>
/// 统一 API 响应结构体（含数据）
/// </summary>
public class ApiResponse<T> : ApiResponse
{
    /// <summary>响应数据</summary>
    public T? Data { get; set; }
}

/// <summary>
/// 分页响应结构体
/// </summary>
public class PagedResponse<T>
{
    /// <summary>当前页码，从 1 开始</summary>
    public int Page { get; set; }

    /// <summary>每页条数</summary>
    public int PageSize { get; set; }

    /// <summary>总数据条数</summary>
    public int TotalCount { get; set; }

    /// <summary>总页数</summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    /// <summary>当前页数据</summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// 是否有上一页
    /// </summary>
    public bool HasPrevious => Page > 1;

    /// <summary>
    /// 是否有下一页
    /// </summary>
    public bool HasNext => Page < TotalPages;
}