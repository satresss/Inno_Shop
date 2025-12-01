using FluentValidation;
using UserService.Application.DTO;

namespace UserService.Application.Validators
{
    public class RefreshTokenDtoValidator : AbstractValidator<RefreshTokenDto>
    {
        public RefreshTokenDtoValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("Refresh token обязателен для заполнения");
        }
    }
}

