using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class ExtensionMethods
{
    public static T DeepClone<T>(T a)
    {
        using (MemoryStream stream = new MemoryStream())
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, a);
            stream.Position = 0;
            return (T)formatter.Deserialize(stream);
        }
    }
}
