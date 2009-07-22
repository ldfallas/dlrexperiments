using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Reflection;
using Langexplr.Experiments;

namespace JSONNetTests
{
    class Program
    {

  

        static void Main(string[] args)
        {

            

            using (TextReader rdr = new StreamReader(@"c:\temp\public_timeline.json"))
            {
                JsonTextReader reader = new JsonTextReader(rdr);
                JsonSerializer serializer = new JsonSerializer();
                JArray o = (JArray)serializer.Deserialize(reader);
                JObject obj;
                Type wt = typeof(Program);
                MethodInfo mi = wt.GetMethod("DoAction");
                Console.WriteLine(o.GetType());


                dynamic dArray = new FSDynArrayWrapper(o);

                foreach (var aJObject in dArray.FindAllByFavoritedAlsoByUserWithScreen_Name("false","bnmeeks"))
                {

                    dynamic tobj = new FSDynJObjectWrapper(aJObject);
                    Console.WriteLine("========");
                    Console.WriteLine(tobj.user.screen_name);
                    Console.Write("\t'{0}'",tobj.text);
                }

          /*      foreach (var v in o) {
                    Console.Write(v);
                }
                var types = from x in o 
                            
                            select (new DynJObjectWrapper((JObject)x["user"]));
                foreach (dynamic t in types)
                {
                    DoAction(t);
                }


                dynamic dArray = new ExpDynJArrayWrapper(o);

                foreach (var s in dArray.FindAllfavorited("false"))
                {
                    Console.Write(s.ToString());
                }
*/
                Console.ReadKey();
            }
        }

        private static void DoAction(dynamic t)
        {
            Console.WriteLine(t.screen_name);
        }
    }
}
