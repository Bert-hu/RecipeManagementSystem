using System.Reflection;
using System.Runtime.Serialization;

namespace Rms.Services.Core.Utils
{
    public class USerializationBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            Assembly ass = Assembly.GetExecutingAssembly();
            return ass.GetType(typeName);
        }
    }
}
