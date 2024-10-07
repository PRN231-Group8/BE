using FluentValidation;
using PRN231.ExploreNow.BusinessObject.Enums;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN231.ExploreNow.Validations.User
{
    public class UserProfileValidation : AbstractValidator<UserRequestModel>
    {
        public UserProfileValidation() 
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("FirstName is required")
                .MaximumLength(50).WithMessage("Maximum lenght must be lower than 50 character");
            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("LastName is required")
                .MaximumLength(50).WithMessage("Maximum lenght must be lower than 50 character");
            RuleFor(x => x.Gender)
                .NotEmpty().WithMessage("Gender is required");
            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Address is required");
        }
    }
}
