using FluentValidation;
using Mblog.API.Models.DTOs.Category;

namespace Mblog.API.Validators;

/// <summary>
/// 创建分类请求校验器
/// </summary>
public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("分类名称不能为空")
            .MaximumLength(128).WithMessage("分类名称长度不能超过 128 个字符");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug 不能为空")
            .MaximumLength(128).WithMessage("Slug 长度不能超过 128 个字符")
            .Matches(@"^[a-z0-9\-]+$").WithMessage("Slug 只能包含小写字母、数字和连字符");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("分类描述长度不能超过 500 个字符");

        RuleFor(x => x.SeoTitle)
            .MaximumLength(255).WithMessage("SEO 标题长度不能超过 255 个字符");

        RuleFor(x => x.SeoDescription)
            .MaximumLength(500).WithMessage("SEO 描述长度不能超过 500 个字符");
    }
}

/// <summary>
/// 更新分类请求校验器
/// </summary>
public class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("分类名称不能为空")
            .MaximumLength(128).WithMessage("分类名称长度不能超过 128 个字符");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug 不能为空")
            .MaximumLength(128).WithMessage("Slug 长度不能超过 128 个字符")
            .Matches(@"^[a-z0-9\-]+$").WithMessage("Slug 只能包含小写字母、数字和连字符");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("分类描述长度不能超过 500 个字符");

        RuleFor(x => x.SeoTitle)
            .MaximumLength(255).WithMessage("SEO 标题长度不能超过 255 个字符");

        RuleFor(x => x.SeoDescription)
            .MaximumLength(500).WithMessage("SEO 描述长度不能超过 500 个字符");
    }
}