using Docms.Queries.DocumentHistories;
using Newtonsoft.Json;
using System;

namespace Docms.Web.Api.Serialization
{
    public class DocumentHistoryConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsAssignableFrom(typeof(DocumentHistory));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var history = (DocumentHistory)value;
            writer.WriteStartObject();
            switch (history.Discriminator)
            {
                case DocumentHistoryDiscriminator.DocumentCreated:
                    writer.WritePropertyName("$type");
                    writer.WriteValue("document_created");
                    break;
                case DocumentHistoryDiscriminator.DocumentUpdated:
                    writer.WritePropertyName("$type");
                    writer.WriteValue("document_updated");
                    break;
                case DocumentHistoryDiscriminator.DocumentDeleted:
                    writer.WritePropertyName("$type");
                    writer.WriteValue("document_deleted");
                    break;
                default:
                    break;
            }
            writer.WritePropertyName("id");
            writer.WriteValue(history.Id);
            writer.WritePropertyName("timestamp");
            writer.WriteValue(history.Timestamp);
            writer.WritePropertyName("path");
            writer.WriteValue(history.Path);
            if (history.FileSize.HasValue)
            {
                writer.WritePropertyName("fileSize");
                writer.WriteValue(history.FileSize);
            }
            if (!string.IsNullOrEmpty(history.Hash))
            {
                writer.WritePropertyName("hash");
                writer.WriteValue(history.Hash);
            }
            if (history.Created.HasValue)
            {
                writer.WritePropertyName("created");
                writer.WriteValue(history.Created);
            }
            if (history.LastModified.HasValue)
            {
                writer.WritePropertyName("lastModified");
                writer.WriteValue(history.LastModified);
            }
            writer.WriteEndObject();
        }
    }
}
