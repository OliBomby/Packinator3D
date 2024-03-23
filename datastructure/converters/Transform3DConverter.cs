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
        var column00 = reader.GetSingle();
        reader.Read();
        var column01 = reader.GetSingle();
        reader.Read();
        var column02 = reader.GetSingle();
        reader.Read();
        var column10 = reader.GetSingle();
        reader.Read();
        var column11 = reader.GetSingle();
        reader.Read();
        var column12 = reader.GetSingle();
        reader.Read();
        var column20 = reader.GetSingle();
        reader.Read();
        var column21 = reader.GetSingle();
        reader.Read();
        var column22 = reader.GetSingle();
        reader.Read();
        var originX = reader.GetSingle();
        reader.Read();
        var originY = reader.GetSingle();
        reader.Read();
        var originZ = reader.GetSingle();
        reader.Read();

        return new Transform3D(column00, column01, column02, column10, column11, column12, column20, column21, column22, originX, originY, originZ);
    }

    public override void Write(Utf8JsonWriter writer, Transform3D value, JsonSerializerOptions options) {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.Basis.Column0.X);
        writer.WriteNumberValue(value.Basis.Column0.Y);
        writer.WriteNumberValue(value.Basis.Column0.Z);
        writer.WriteNumberValue(value.Basis.Column1.X);
        writer.WriteNumberValue(value.Basis.Column1.Y);
        writer.WriteNumberValue(value.Basis.Column1.Z);
        writer.WriteNumberValue(value.Basis.Column2.X);
        writer.WriteNumberValue(value.Basis.Column2.Y);
        writer.WriteNumberValue(value.Basis.Column2.Z);
        writer.WriteNumberValue(value.Origin.X);
        writer.WriteNumberValue(value.Origin.Y);
        writer.WriteNumberValue(value.Origin.Z);
        writer.WriteEndArray();
    }
}