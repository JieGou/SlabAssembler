using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Urbbox.SlabAssembler.Managers
{
    public class XmlModelDatabase<T> where T : new()
    {
        public string FilePathName { get; private set; }

        public XmlModelDatabase(string file)
        {
            FilePathName = file;
        }

        public T LoadData()
        {
            try
            {
                var deserializer = new XmlSerializer(typeof(T));
                using (TextReader reader = new StreamReader(FilePathName))
                    return (T)deserializer.Deserialize(reader);
            }
            catch (IOException)
            {
                return new T();
            }
        }

        public Task<T> LoadDataAsync()
        {
            return Task.Run(() => LoadData());
        }

        public void SaveData(T data)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(T));
                using (TextWriter writer = new StreamWriter(FilePathName))
                    serializer.Serialize(writer, data);
            }
            catch (IOException) { }
        }

        public Task SaveDataAsync(T data)
        {
            return Task.Run(() => SaveData(data));
        }
    }
}
