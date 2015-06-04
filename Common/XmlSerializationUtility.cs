using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Common
{
    public static class XmlSerializationUtility
    {
        public static MemoryStream SerializeToStream<T>(T entity)
        {
            MemoryStream memoryStream = new MemoryStream();
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            xmlSerializer.Serialize(memoryStream, entity);

            return memoryStream;
        }

        public static XDocument SerializeToXDocument<T>(T entity)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                xmlSerializer.Serialize(memoryStream, entity);

                memoryStream.Position = 0;

                return XDocument.Load(memoryStream);
            }
        }

        public static T DeserializeFromXDocument<T>(XDocument document)
            where T : class
        {
            XmlReader xmlReader = document.CreateReader();
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            return xmlSerializer.Deserialize(xmlReader) as T;
        }
    }
}
