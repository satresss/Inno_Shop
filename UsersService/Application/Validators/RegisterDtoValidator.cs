using FluentValidation;
using UserService.Application.DTO;

namespace UserService.Application.Validators
{
    public class RegisterDtoValidator : AbstractValidator<RegisterDto>
    {
        public RegisterDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Имя обязательно для заполнения")
                .MinimumLength(2).WithMessage("Имя должно содержать минимум 2 символа")
                .MaximumLength(100).WithMessage("Имя не должно превышать 100 символов");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email обязателен для заполнения")
                .EmailAddress().WithMessage("Некорректный формат email")
                .MaximumLength(200).WithMessage("Email не должен превышать 200 символов");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Пароль обязателен для заполнения")
                .MinimumLength(6).WithMessage("Пароль должен содержать минимум 6 символов")
                .MaximumLength(100).WithMessage("Пароль не должен превышать 100 символов")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)").WithMessage("Пароль должен содержать хотя бы одну заглавную букву, одну строчную букву и одну цифру");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("Подтверждение пароля обязательно")
                .Equal(x => x.Password).WithMessage("Пароли не совпадают");
        }
    }
}

