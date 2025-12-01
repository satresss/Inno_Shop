using FluentValidation;
using UserService.Application.DTO;

namespace UserService.Application.Validators
{
    public class ResetPasswordDtoValidator : AbstractValidator<ResetPasswordDto>
    {
        public ResetPasswordDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email обязателен для заполнения")
                .EmailAddress().WithMessage("Некорректный формат email");

            RuleFor(x => x.Token)
                .NotEmpty().WithMessage("Токен обязателен для заполнения");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("Новый пароль обязателен для заполнения")
                .MinimumLength(6).WithMessage("Пароль должен содержать минимум 6 символов")
                .MaximumLength(100).WithMessage("Пароль не должен превышать 100 символов")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)").WithMessage("Пароль должен содержать хотя бы одну заглавную букву, одну строчную букву и одну цифру");

            RuleFor(x => x.ConfirmNewPassword)
                .NotEmpty().WithMessage("Подтверждение пароля обязательно")
                .Equal(x => x.NewPassword).WithMessage("Пароли не совпадают");
        }
    }
}

