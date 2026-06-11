using FluentValidation;
using Mblog.API.Models.DTOs.Tag;

namespace Mblog.API.Validators;

/// <summary>
/// 创建标签请求校验器
/// </summary>
public class CreateTagRequestValidator : AbstractValidator<CreateTagRequest>
{
    public CreateTagRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("标签名称不能为空")
            .MaximumLength(128).WithMessage("标签名称长度不能超过 128 个字符");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug 不能为空")
            .MaximumLength(128).WithMessage("Slug 长度不能超过 128 个字符")
            .Matches(@"^[a-z0-9\-]+$").WithMessage("Slug 只能包含小写字母、数字和连字符");

        RuleFor(x => x.Color)
            .Matches(@"^#[0-9a-fA-F]{6}$").WithMessage("颜色格式不正确，应为 #RRGGBB 格式")
            .When(x => !string.IsNullOrEmpty(x.Color));
    }
}

/// <summary>
/// 更新标签请求校验器
/// </summary>
public class UpdateTagRequestValidator : AbstractValidator<UpdateTagRequest>
{
    public UpdateTagRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("标签名称不能为空")
            .MaximumLength(128).WithMessage("标签名称长度不能超过 128 个字符");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug 不能为空")
            .MaximumLength(128).WithMessage("Slug 长度不能超过 128 个字符")
            .Matches(@"^[a-z0-9\-]+$").WithMessage("Slug 只能包含小写字母、数字和连字符");

        RuleFor(x => x.Color)
            .Matches(@"^#[0-9a-fA-F]{6}$").WithMessage("颜色格式不正确，应为 #RRGGBB 格式")
            .When(x => !string.IsNullOrEmpty(x.Color));
    }
}