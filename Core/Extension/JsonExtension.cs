using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;
using Tie;

namespace Sys
{
    public static class Json
    {
        private static readonly DataContractJsonSerializerSettings setting = new DataContractJsonSerializerSettings
        {
            DateTimeFormat = new DateTimeFormat("yyyy-MM-dd'T'HH:mm:ssK"),
            SerializeReadOnlyTypes = false,
        };

        public static T Deserialize<T>(this string json)
        {
            return (T)Deserialize(typeof(T), json);
        }

        public static string Serialize<T>(this T graph)
        {
            return Serialize(typeof(T), graph);
        }

        public static object Deserialize(Type type, string json)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(type, setting);
            using (MemoryStream stream = new MemoryStream())
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.Write(json);
                writer.Flush();
                stream.Position = 0;
                return serializer.ReadObject(stream);
            }
        }

        public static string Serialize(Type type, object graph)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(type, setting);

            using (MemoryStream stream = new MemoryStream())
            {
                serializer.WriteObject(stream, graph);
                stream.Position = 0;
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        public static T ReadObject<T>(this string json)
        {
            if (json == null)
                return default(T);

            var val = Script.Evaluate(json);
            return Valizer.Devalize<T>(val);
        }

        public static string WriteObject<T>(this T graph)
        {
            var val = Valizer.Valize(graph);
            return val.ToJson();
        }

    }
}
