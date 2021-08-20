using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace SerialPortController
{
    public class ConfigHelper
    {
        /// <summary>
        /// 序列化指定对象到指定文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="obj"></param>
        public static void Save(string path, Object obj)
        {
            using (FileStream fs = File.Create(path))
            {
                XmlSerializer ser = new XmlSerializer(obj.GetType());
                ser.Serialize(fs, obj);
            }
        }
        /// <summary>
        /// 从指定文件反序列化对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public static T Get<T>(string path)
        {
            T obj = Activator.CreateInstance<T>();
            if (File.Exists(path))
                using (FileStream fs = File.Open(path, FileMode.OpenOrCreate))
                {
                    XmlSerializer ser = new XmlSerializer(typeof(T));
                    obj = (T)ser.Deserialize(fs);
                }

            return obj;
        }
    }
}
