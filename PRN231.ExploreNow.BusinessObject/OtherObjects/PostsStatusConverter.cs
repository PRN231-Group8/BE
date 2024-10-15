using PRN231.ExploreNow.BusinessObject.Enums;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PRN231.ExploreNow.BusinessObject.OtherObjects
{
	public class PostsStatusConverter : JsonConverter<PostsStatus>
	{
		public override PostsStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			string? value = reader.GetString();
			if (string.IsNullOrEmpty(value))
			{
				throw new JsonException("PostsStatus cannot be null or empty.");
			}

			if (Enum.TryParse(typeof(PostsStatus), value, true, out object? result))
			{
				return (PostsStatus)result;
			}

			throw new JsonException($"Invalid PostsStatus value: {value}");
		}

		public override void Write(Utf8JsonWriter writer, PostsStatus value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.ToString());
		}
	}
}
