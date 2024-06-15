using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BalthasarIL
{
    public class SerializationFormatters
    {
        private Type? assemblyType = null;
        private object instance = null;

        private MethodInfo serialize;
        private MethodInfo deserialize;
        public SerializationFormatters(string DllPath= "BalthasarIL.BinaryFormatters.dll", string Formatter= "BinaryFormatter") 
        {
            var assembly = Assembly.LoadFrom(DllPath).GetTypes();
            if(Formatter=="BinaryFormatter"){
                //这只在传统的序列化器上使用，如果需要解锁任意类序列化，请使用BalthasarIL.BinaryFormatters.dll且不需要修改这里
                var lacType = assembly.First(p => p.Name == "LocalAppContextSwitches");
                var enField = lacType.GetField("s_binaryFormatterEnabled", BindingFlags.Static | BindingFlags.NonPublic);
                enField.SetValue(null, 1);
            }
            this.assemblyType = assembly.First(p => p.Name == Formatter);
            this.instance = this.assemblyType.GetConstructor(Type.EmptyTypes).Invoke(null);
            this.serialize = this.assemblyType.GetMethod("Serialize");
            this.deserialize = this.assemblyType.GetMethod("Deserialize");
        }
        public SerializationFormatters(ISurrogateSelector selector, StreamingContext context, string DllPath = "BalthasarIL.BinaryFormatters.dll", string Formatter = "BinaryFormatter")
        {
            Type[] assembly = null;
            try
            {
                assembly = Assembly.LoadFrom(DllPath).GetTypes();
            }
            catch(Exception e) { 
                assembly = Assembly.Load(DllPath).GetTypes(); 
            }
            if (Formatter == "BinaryFormatter")
            {
                //这只在传统的序列化器上使用，如果需要解锁任意类序列化，请使用BalthasarIL.BinaryFormatters.dll且不需要修改这里
                var lacType = assembly.First(p => p.Name == "LocalAppContextSwitches");
                var enField = lacType.GetField("s_binaryFormatterEnabled", BindingFlags.Static | BindingFlags.NonPublic);
                enField.SetValue(null, 1);
            }
            this.assemblyType = assembly.First(p => p.Name == Formatter);
            this.instance = assemblyType.GetConstructor(new Type[] { typeof(ISurrogateSelector), typeof(StreamingContext) }).Invoke(new object[] { selector, context });
            this.serialize = this.assemblyType.GetMethod("Serialize");
            this.deserialize = this.assemblyType.GetMethod("Deserialize");
        }
        
        public void Serialize(Stream stream,object obj)
        {
            this.serialize.Invoke(this.instance, new[] { stream,obj });
        }

        public object Deserialize(Stream stream) {
            return this.deserialize.Invoke(this.instance, new[] { stream });
        }
    }
}
