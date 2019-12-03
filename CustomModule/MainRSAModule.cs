using CustomModule.RSA;
using Parcs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CustomModule
{
    public class MainRSAModule : IModule
    {
        public static bool mode;
        public static int pointsNum = 2;
        public static void Main(string[] args)
        {
            try
            {
                if (args.Length != 0)
                {
                    for (int i = 0; i < args.Length; i++)
                    {
                        if (args[i] == "-d")
                        {
                            mode = true;
                        }

                        if (args[i] == "-p")
                        {
                            if (args[i + 1] != null)
                            {
                                pointsNum = Convert.ToInt32(args[i + 1]);
                            }
                        }
                    }


                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }
            

            var job = new Job();
            if (!job.AddFile(Assembly.GetExecutingAssembly().Location/*"MyFirstModule.exe"*/))
            {
                Console.WriteLine("File doesn't exist");
                return;
            }

            (new MainRSAModule()).Run(new ModuleInfo(job, null));
            Console.ReadKey();
        }
        
        public void Run(ModuleInfo info, CancellationToken token = default(CancellationToken))
        {
            string key1;
            string key2;
            string[] texts;

            string text = @"Auroras are occasionally seen in latitudes below the auroral zone, 
                            when a geomagnetic storm temporarily enlarges the auroral oval.Red: At the highest altitudes, 
                           excited atomic oxygen emits at 630 nm (red); low concentration of atoms and lower sensitivity of 
                           eyes at this wavelength make this color visible only under more intense solar activity. The 
                           low number of oxygen atoms and their gradually diminishing concentration is responsible for 
                           the faint appearance of the top parts of the curtains. 
                           Scarlet, crimson, and carmine are the most often-seen hues of red for the auroras.";

            if (mode)
            {
                text = File.ReadAllText("Encoded.txt");
                
                key1 = File.ReadAllText("Key1.txt");
                key2 = File.ReadAllText("Key2.txt");
                Console.WriteLine("encoded text :{0}",text);
                Console.WriteLine("key1: {0}",key1);
                Console.WriteLine("key2: {0}", key2);

                texts = text.Split(new char[] {'|'});
                if (texts.Length != pointsNum)
                {
                    Console.WriteLine($"Source was encodden on {texts.Length} numper of points while you set {pointsNum}, use same amount");
                    throw new Exception($"Source was encodden on {texts.Length} numper of points while you set {pointsNum}, use same amount");
                }

            }
            else
            {
                try
                {
                    text = File.ReadAllText("TextSample.txt");
                }
                catch (Exception e)
                {
                    Console.WriteLine("File missing, using sample");

                }



                int len = text.Length / pointsNum;
                int ost = text.Length % pointsNum;
                texts = new string[pointsNum];

                for (int i = 0; i < pointsNum - 1; i++)
                {
                    texts[i] = text.Substring(len * i, len);
                }
                texts[pointsNum-1] = text.Substring(len * (pointsNum - 1), len + ost);

                RSAProvider rsa = new RSAProvider(Constants.KeySize.Bit64);

                key1 =  rsa.PublicKey.KeyOne.ToString();
                key2 =  rsa.PublicKey.KeyTwo.ToString();

                string prkey1 = rsa.PrivateKey.KeyOne.ToString();
                string prkey2 = rsa.PrivateKey.KeyTwo.ToString();
                Console.WriteLine("key1: {0}", prkey1);
                Console.WriteLine("key2: {0}", prkey2);

                File.WriteAllText("Key1.txt", prkey1);
                File.WriteAllText("Key2.txt", prkey2);
            }

            

            
            var points = new IPoint[pointsNum];
            var channels = new IChannel[pointsNum];
            string exe = mode ? "CustomModule.RSADecModule" : "CustomModule.RSAModule";
            for (int i = 0; i < pointsNum; ++i)
            {
                points[i] = info.CreatePoint();
                channels[i] = points[i].CreateChannel();
                points[i].ExecuteClass(exe);
            }

            

            for (int i = 0; i < pointsNum; ++i)
            {
                channels[i].WriteData(key1);
                channels[i].WriteData(key2);
                channels[i].WriteData(texts[i]);
                Console.WriteLine("sending {0}",texts[i]);
            }
            DateTime time = DateTime.Now;
            Console.WriteLine("Waiting for result...");

            string res = "";
            var builder = new StringBuilder();
            for (int i = pointsNum - 1; i >= 0; --i)
            {
                
                builder.Append(channels[i].ReadString());
                builder.Append("|");
                //res += channels[i].ReadString();
            }

            res = mode ? builder.ToString().Replace("|", "") : builder.ToString().TrimEnd('|');

            Console.WriteLine("Result found: res = {0}, time = {1}", res, System.Math.Round((DateTime.Now - time).TotalSeconds, 3));



            if (mode)
            {
                Console.WriteLine(res);
                File.WriteAllText("TextSample.txt", res,Encoding.Default);

            }
            else
            {
                File.WriteAllText("Encoded.txt", res);
            }

            Console.WriteLine("Press any key to exit");

        }
    }
}
