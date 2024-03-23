using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

namespace Packinator3D.datastructure.converters;

public class Transform3DConverter : JsonConverter<Transform3D> {
    public override Transform3D Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType != JsonTokenType.StartArray) {
            throw new JsonException();
        }

        reader.Read();
        float xx = reader.GetSingle();
        reader.Read();
        float xy = reader.GetSingle();
        reader.Read();
        float xz = reader.GetSingle();
        reader.Read();
        float yx = reader.GetSingle();
        reader.Read();
        float yy = reader.GetSingle();
        reader.Read();
        float yz = reader.GetSingle();
        reader.Read();
        float zx = reader.GetSingle();
        reader.Read();
        float zy = reader.GetSingle();
        reader.Read();
        float zz = reader.GetSingle();
        reader.Read();
        float ox = reader.GetSingle();
        reader.Read();
        float oy = reader.GetSingle();
        reader.Read();
        float oz = reader.GetSingle();
        reader.Read();

        return new Transform3D(xx, yx, zx, xy, yy, zy, xz, yz, zz, ox, oy, oz);
    }

    public override void Write(Utf8JsonWriter writer, Transform3D value, JsonSerializerOptions options) {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.Basis.X.X);
        writer.WriteNumberValue(value.Basis.X.Y);
        writer.WriteNumberValue(value.Basis.X.Z);
        writer.WriteNumberValue(value.Basis.Y.X);
        writer.WriteNumberValue(value.Basis.Y.Y);
        writer.WriteNumberValue(value.Basis.Y.Z);
        writer.WriteNumberValue(value.Basis.Z.X);
        writer.WriteNumberValue(value.Basis.Z.Y);
        writer.WriteNumberValue(value.Basis.Z.Z);
        writer.WriteNumberValue(value.Origin.X);
        writer.WriteNumberValue(value.Origin.Y);
        writer.WriteNumberValue(value.Origin.Z);
        writer.WriteEndArray();
    }
}