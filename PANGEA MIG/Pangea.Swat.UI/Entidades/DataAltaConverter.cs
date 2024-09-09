using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pangea.Swat.UI.Entidades
{
    public class DeltaAltaJsonConverter : JsonConverter
    {
        private readonly Type[] _types;

        public DeltaAltaJsonConverter(params Type[] types)
        {
            _types = types;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            //JToken t = JToken.FromObject(value);

            //if (t.Type != JTokenType.Object)
            //{
            //    t.WriteTo(writer);
            //}
            //else
            //{
            //    JObject o = (JObject)t;
            //    IList<string> propertyNames = o.Properties().Select(p => p.Name).ToList();

            //    o.AddFirst(new JProperty("Header", new JArray(propertyNames)));
            //    o.AddFirst(new JProperty("Body", new JArray(propertyNames)));

            //    o.WriteTo(writer);
            //}

            if (value == null)
            {
                serializer.Serialize(writer, null);
                return;
            }

            var properties = value.GetType().GetProperties();   //.Where(p => p.PropertyType == typeof(int));

            writer.WriteStartObject();

            foreach (var property in properties)
            {
                try
                {
                    var prop2 = Type.GetType(property.PropertyType.FullName).GetProperties();
                    if (prop2.Count() > 0)
                    {
                        //                    writer.WritePropertyName(property.Name);
                        foreach (var prop in prop2)
                        {
                            writer.WritePropertyName(prop.Name);
                            writer.WriteValue(prop.GetValue(prop, null));
                            //serializer.Serialize(writer, prop.GetValue(property, null));
                        }
                    }
                    // write property name
                    writer.WritePropertyName(property.Name);
                    // let the serializer serialize the value itself
                    // (so this converter will work with any other type, not just int)
                    serializer.Serialize(writer, property.GetValue(value, null));
                }
                catch (Exception ex)
                { }
            }
            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanConvert(Type objectType)
        {
            return _types.Any(t => t == objectType);
        }
    }
}
