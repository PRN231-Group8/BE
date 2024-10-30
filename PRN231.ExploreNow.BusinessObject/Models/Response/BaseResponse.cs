namespace PRN231.ExploreNow.BusinessObject.Models.Response;

public class BaseResponse<T>
{
	public bool IsSucceed { get; set; }
	public T? Result { get; set; }
	public List<T> Results { get; set; }
	public string? Message { get; set; }
	public int TotalElements { get; set; }
	public int TotalPages { get; set; }
	public bool Last { get; set; }
	public int Size { get; set; }
	public int Number { get; set; }
	public SortInfo Sort { get; set; }
	public int NumberOfElements { get; set; }
	public bool First { get; set; }
	public bool Empty { get; set; }

	public class SortInfo
	{
		public bool Empty { get; set; }
		public bool Sorted { get; set; }
		public bool Unsorted { get; set; }
	}

	public BaseResponse(List<T> items, int totalElements, int pageNumber, int pageSize)
	{
		IsSucceed = true;
		Results = items;
		TotalElements = totalElements;
		Size = pageSize;
		Number = pageNumber;
		TotalPages = (int)Math.Ceiling(totalElements / (double)pageSize);
		NumberOfElements = items.Count;

		First = pageNumber == 0;
		Last = pageNumber >= TotalPages - 1;
		Empty = !items.Any();

		Sort = new SortInfo
		{
			Empty = false,
			Sorted = true,
			Unsorted = false
		};
	}

	// Default constructor
	public BaseResponse()
	{
		Results = new List<T>();
		Sort = new SortInfo();
	}
}