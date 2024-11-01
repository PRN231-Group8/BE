using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using PRN231.ExploreNow.BusinessObject.Models.Request;

namespace PRN231.ExploreNow.Validations.Transportation
{
    public class TransportationValidator : AbstractValidator<TransportationRequestModel>
    {
        public TransportationValidator()
        {
            RuleFor(x => x.Price).GreaterThan(0).WithMessage("Price must be above zero");
            RuleFor(x => x.Capacity).GreaterThan(0).WithMessage("Capacity must be above zero");
            RuleFor(x => x.Type).IsInEnum().WithMessage("Invalid transportation type");
            RuleFor(x => x.TourId).NotEmpty().WithMessage("Tour ID is required");
        }
    }
}
