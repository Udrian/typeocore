using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using TypeOEngine.Typedeaf.Core.Common;

namespace TypeDCore.Models.Data
{
    /// <summary>
    /// Serializes a Color as only its A, R, G and B channels.
    /// </summary>
    public class ColorJsonConverter : JsonConverter<Color>
    {
        public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Expected a color object.");

            byte a = 255, r = 0, g = 0, b = 0;
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    return new Color(a, r, g, b);

                if (reader.TokenType != JsonTokenType.PropertyName)
                    continue;

                var name = reader.GetString();
                reader.Read();
                switch (name?.ToUpperInvariant())
                {
                    case "A": a = reader.GetByte(); break;
                    case "R": r = reader.GetByte(); break;
                    case "G": g = reader.GetByte(); break;
                    case "B": b = reader.GetByte(); break;
                    default: reader.Skip(); break;
                }
            }

            throw new JsonException("Unexpected end of color object.");
        }

        public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("A", value.A);
            writer.WriteNumber("R", value.R);
            writer.WriteNumber("G", value.G);
            writer.WriteNumber("B", value.B);
            writer.WriteEndObject();
        }
    }
}
