using FluentValidation;
using Mblog.API.Models.DTOs.Article;

namespace Mblog.API.Validators;

/// <summary>
/// 创建文章请求校验器
/// </summary>
public class CreateArticleRequestValidator : AbstractValidator<CreateArticleRequest>
{
    public CreateArticleRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("文章标题不能为空")
            .MaximumLength(255).WithMessage("文章标题长度不能超过 255 个字符");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug 不能为空")
            .MaximumLength(255).WithMessage("Slug 长度不能超过 255 个字符")
            .Matches(@"^[a-z0-9\-]+$").WithMessage("Slug 只能包含小写字母、数字和连字符");

        RuleFor(x => x.Summary)
            .MaximumLength(500).WithMessage("文章摘要长度不能超过 500 个字符");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("文章内容不能为空");

        RuleFor(x => x.Status)
            .Must(s => s == "draft" || s == "published")
            .WithMessage("状态只能是 draft 或 published");

        RuleFor(x => x.SeoTitle)
            .MaximumLength(255).WithMessage("SEO 标题长度不能超过 255 个字符");

        RuleFor(x => x.SeoDescription)
            .MaximumLength(500).WithMessage("SEO 描述长度不能超过 500 个字符");
    }
}

/// <summary>
/// 更新文章请求校验器
/// </summary>
public class UpdateArticleRequestValidator : AbstractValidator<UpdateArticleRequest>
{
    public UpdateArticleRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("文章标题不能为空")
            .MaximumLength(255).WithMessage("文章标题长度不能超过 255 个字符");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug 不能为空")
            .MaximumLength(255).WithMessage("Slug 长度不能超过 255 个字符")
            .Matches(@"^[a-z0-9\-]+$").WithMessage("Slug 只能包含小写字母、数字和连字符");

        RuleFor(x => x.Summary)
            .MaximumLength(500).WithMessage("文章摘要长度不能超过 500 个字符");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("文章内容不能为空");

        RuleFor(x => x.Status)
            .Must(s => s == "draft" || s == "published" || s == "archived")
            .WithMessage("状态只能是 draft / published / archived");

        RuleFor(x => x.SeoTitle)
            .MaximumLength(255).WithMessage("SEO 标题长度不能超过 255 个字符");

        RuleFor(x => x.SeoDescription)
            .MaximumLength(500).WithMessage("SEO 描述长度不能超过 500 个字符");
    }
}

/// <summary>
/// 更新文章状态请求校验器
/// </summary>
public class UpdateArticleStatusRequestValidator : AbstractValidator<UpdateArticleStatusRequest>
{
    public UpdateArticleStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("状态不能为空")
            .Must(s => s == "draft" || s == "published" || s == "archived")
            .WithMessage("状态只能是 draft / published / archived");
    }
}