using FluentValidation;
using UserService.Application.DTO;

namespace UserService.Application.Validators
{
    public class ConfirmEmailDtoValidator : AbstractValidator<ConfirmEmailDto>
    {
        public ConfirmEmailDtoValidator()
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("Некорректный идентификатор пользователя");

            RuleFor(x => x.Token)
                .NotEmpty().WithMessage("Токен обязателен для заполнения");
        }
    }
}

