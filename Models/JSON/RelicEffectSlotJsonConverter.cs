﻿using System.Text.Json;
using System.Text.Json.Serialization;

namespace NightreignRelicEditor.Models.JSON;

public class RelicEffectSlotJsonConverter(bool isDeepRelic = false) : JsonConverter<RelicEffectSlot>
{
    public override RelicEffectSlot? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return JsonSerializer.Deserialize<RelicEffectSlot>(ref reader, options);
    }

    public override void Write(Utf8JsonWriter writer, RelicEffectSlot value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WritePropertyName("Effect");
        JsonSerializer.Serialize(writer, value.Effect, options);

        if (isDeepRelic)
        {
            writer.WritePropertyName("Curse");
            JsonSerializer.Serialize(writer, value.Curse, options);
        }

        writer.WriteEndObject();
    }
}