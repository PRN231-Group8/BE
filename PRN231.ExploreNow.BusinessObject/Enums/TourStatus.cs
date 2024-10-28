namespace PRN231.ExploreNow.BusinessObject.Enums;
// ACTIVE -> INACTIVE: Khi cần tạm ngừng bán tour để cập nhật
// INACTIVE -> ACTIVE: Khi tour sẵn sàng mở bán trở lại
// ACTIVE/INACTIVE -> CANCELLED: Khi quyết định hủy tour vĩnh viễn trong tương lai

public enum TourStatus
{
	ACTIVE,
	INACTIVE,
	CANCELLED
}
