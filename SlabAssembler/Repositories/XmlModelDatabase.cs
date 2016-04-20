using System.IO;
using System.Xml.Serialization;

namespace Urbbox.SlabAssembler.Repositories
{
    public class XmlModelDatabase<T> where T : new()
    {
        public string File { get; set; }

        public XmlModelDatabase(string file)
        {
            File = file;
        }

        public T LoadData()
        {
            try
            {
                var deserializer = new XmlSerializer(typeof(ConfigurationData));
                using (TextReader reader = new StreamReader(File))
                    return (T) deserializer.Deserialize(reader);
            }
            catch (IOException) { }

            return new T();
        }

        public void SaveData(T data)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(ConfigurationData));
                using (TextWriter writer = new StreamWriter(File))
                    serializer.Serialize(writer, data);
            }
            catch (IOException) { }
        }
    }
}
