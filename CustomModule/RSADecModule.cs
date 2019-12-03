using CustomModule.RSA;
using Parcs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CustomModule
{
    public class RSADecModule : IModule
    {
        public void Run(ModuleInfo info, CancellationToken token = default(CancellationToken))
        {
            BigInteger Privatek = BigInteger.Parse(info.Parent.ReadString());
            BigInteger shared = BigInteger.Parse(info.Parent.ReadString());
            string text = info.Parent.ReadString();
            var texts = text.Split('.').ToList();
            
            Console.WriteLine(Privatek);
            Console.WriteLine(shared);
            Console.WriteLine(text);
            RSAKey privateKey = new RSAKey(Privatek, shared);

            var list = new List<BigInteger>();

            texts.ForEach(z => list.Add(BigInteger.Parse(z, NumberStyles.Float,
                CultureInfo.InvariantCulture)));

            var result = RSAProvider.Decrypt(list, privateKey);
            //var trueResult = result.Aggregate((x, y) => x + y);
            //info.Parent.WriteData(trueResult.ToString());
            info.Parent.WriteData(result);
        }
    }
}
