using FluentValidation;
using Mblog.API.Models.DTOs.Auth;

namespace Mblog.API.Validators;

/// <summary>
/// 登录请求校验器
/// </summary>
public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("用户名不能为空")
            .MaximumLength(64).WithMessage("用户名长度不能超过 64 个字符");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("密码不能为空");
    }
}

/// <summary>
/// 刷新令牌请求校验器
/// </summary>
public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("刷新令牌不能为空");
    }
}