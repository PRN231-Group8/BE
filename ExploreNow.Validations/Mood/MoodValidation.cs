using FluentValidation;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN231.ExploreNow.Validations.Mood
{
    public class MoodValidation : AbstractValidator<MoodRequest>
    {
        public MoodValidation() 
        {
            RuleFor(m => m.MoodTag)
                .NotEmpty().WithMessage("MoodTag is required");
            RuleFor(m => m.IconName)
                .NotEmpty().WithMessage("IconName is required");
        }
    }
}
