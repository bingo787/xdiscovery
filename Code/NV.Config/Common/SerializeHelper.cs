using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NV.Config
{
    using System;
    using System.IO;
    using System.Xml.Serialization;

    public static class SerializeHelper
    {
        public static T LoadFromFile<T>(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return (T)Activator.CreateInstance(typeof(T));
            }
            using (Stream stream = File.OpenRead(fileName))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                return (T)serializer.Deserialize(stream);
            }
        }

        public static void SaveToFile(object o, string fileName)
        {
            using (Stream stream = File.Create(fileName))
            {
                new XmlSerializer(o.GetType()).Serialize(stream, o);
            }
        }
    }
}
