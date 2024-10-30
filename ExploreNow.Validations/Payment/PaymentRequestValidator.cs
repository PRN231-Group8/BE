using FluentValidation;
using PRN231.ExploreNow.BusinessObject.Models.Request;

namespace PRN231.ExploreNow.Validations.Payment
{
	public class PaymentRequestValidator : AbstractValidator<PaymentRequest>
	{
		public PaymentRequestValidator()
		{
			RuleFor(x => x.TourTripId)
				.NotEmpty().WithMessage("TourTripId is required.");
		}
	}
}
