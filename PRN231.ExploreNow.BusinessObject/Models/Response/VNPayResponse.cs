namespace PRN231.ExploreNow.BusinessObject.Models.Response
{
	public class VNPayResponse
	{
		public bool Success { get; set; }
		public string OrderDescription { get; set; }
		public string OrderId { get; set; }
		public string Token { get; set; }
		public double Amount { get; set; }
		public string Message { get; set; }
		public string TransactionId { get; set; }
		public string PaymentMethod { get; set; }
		public string PaymentId { get; set; }
		public string VnPayResponseCode { get; set; }
	}
}
