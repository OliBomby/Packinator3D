using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

namespace Packinator3D.datastructure.converters;

public class ColorConverter : JsonConverter<Color> {
    public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType != JsonTokenType.StartArray) {
            throw new JsonException();
        }

        reader.Read();
        float r = reader.GetSingle();
        reader.Read();
        float g = reader.GetSingle();
        reader.Read();
        float b = reader.GetSingle();
        reader.Read();
        float a = reader.GetSingle();
        reader.Read();

        return new Color(r, g, b, a);
    }

    public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options) {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.R);
        writer.WriteNumberValue(value.G);
        writer.WriteNumberValue(value.B);
        writer.WriteNumberValue(value.A);
        writer.WriteEndArray();
    }
}