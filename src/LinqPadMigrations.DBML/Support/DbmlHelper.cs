
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using DbmlSchema;
namespace LinqPadMigrations.DBML
{
    public static class DbmlHelper
    {
        public static Database FromXml(string dbmlXmlContents)
        {
            return DeserializeFromXml<Database>(dbmlXmlContents);
        }

        public static string ToXml(Database dbml)
        {
            return SerializeToXml(dbml);
        }

        private static string SerializeToXml(object targetInstance)
        {
            // Must have encoding="UTF-8" or NONE at all.  UTF-16 will NOT work for SqlMetal.exe
            // Force UTF-8 encoding on TextWriter: http://stackoverflow.com/questions/955611/xmlwriter-to-write-to-a-string-instead-of-to-a-file
            // Another thread [not used] to remove encoding when serializing XML: http://stackoverflow.com/questions/6297249/remove-encoding-from-xmlserializer

            string xml = string.Empty;

            using (TextWriter writer = new Utf8StringWriter())
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(writer))
                {
                    new XmlSerializer(targetInstance.GetType()).Serialize(xmlWriter, targetInstance);
                }

                xml = writer.ToString();
            }

            return xml;
        }

        private static T DeserializeFromXml<T>(string objectXml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            StringReader stringReader = new StringReader(objectXml);
            XmlTextReader xmlReader = new XmlTextReader(stringReader);
            object value = serializer.Deserialize(xmlReader);
            return (T)value;
        }
    }

    internal class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
    }

}
