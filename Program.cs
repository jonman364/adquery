using System;
using System.DirectoryServices;
using System.Collections.Generic;
using System.Text;

namespace adlib {
    enum SearchType { UNKNOWN, USER, COMPUTER, GROUP };

    class Program {
        static void Main(string[] args) {
            SearchType type = SearchType.UNKNOWN;
            bool readStdIn = false;
            bool format = true;
            List<string> terms = new List<string>();
            List<string> inopts = new List<string>();
            List<string> outopts = new List<string>();

            if (args.Length < 4){
                printUsage();
                return;
            }

            if(args[0].Equals("computer", StringComparison.OrdinalIgnoreCase))
                type = SearchType.COMPUTER;
            else if(args[0].Equals("user", StringComparison.OrdinalIgnoreCase))
                type = SearchType.USER;
            else if(args[0].Equals("group", StringComparison.OrdinalIgnoreCase))
                type = SearchType.GROUP;


            for (int i = 1; i < args.Length; i++) {
                string arg = args[i];
                if(arg.StartsWith("-i", StringComparison.OrdinalIgnoreCase))
                    inopts.Add(arg.Substring(2));
                else if(arg.StartsWith("-o", StringComparison.OrdinalIgnoreCase))
                    outopts.Add(arg.Substring(2));
                else if(arg.Equals("-n"))
                    format = false;
                else if(arg.Equals("-"))
                    readStdIn = true;
                else {
                    terms.Add(arg);
                }
            }

            if (readStdIn)
                getStdIn(ref terms);


            if (terms.Count != inopts.Count) {
                Console.WriteLine("The number of search terms does not match the number of attributes IN[{0}] TERMS[{1}]", inopts.Count, terms.Count);
                return;
            }

            StringBuilder filterStr;

            if(type == SearchType.USER)
                filterStr = new StringBuilder("(&(objectCategory=person)");
            else if(type == SearchType.COMPUTER)
                filterStr = new StringBuilder("(&(objectCategory=computer)");
            else if(type == SearchType.GROUP)
                filterStr = new StringBuilder("(&(objectCategory=group)");
            else {
                printUsage();
                return;
            }

            for (int i = 0; i < inopts.Count; i++)
                filterStr.Append(string.Format("({0}={1})", inopts[i], terms[i]));
            filterStr.Append(")");

            DirectorySearcher search = new DirectorySearcher();
            search.Filter = filterStr.ToString();

            foreach (string outopt in outopts)
                search.PropertiesToLoad.Add(outopt);

            try {
                SearchResultCollection results = search.FindAll();

                for(int i = 0; i < results.Count; i++) {
                    for(int k = 0; k < outopts.Count; k++)
                        printResult(results[i], outopts[k]);

                    if(format)
                        Console.WriteLine();
                }
            }
            catch(System.Runtime.InteropServices.COMException) {
                Console.WriteLine("COM exception: Are you logged into a domain account?");
            }


        }

        static void getStdIn(ref List<string> opts){
            while (Console.In.Peek() != -1)
                opts.Add(Console.ReadLine().Trim());
        }

        static void printUsage() {
            Console.WriteLine("{0} {1}\nUsage: {0} <computer | user | group> [-n] -i<attribute> <value> [-i<attribute> <value> ...] -o<attribute> [-o<attribute> ...]", 
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Name, System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
        }

        static void printResult(SearchResult result, string attrib) {
            try {
                Type type = result.Properties[attrib][0].GetType();
                if(typeof(string).IsAssignableFrom(type) || typeof(int).IsAssignableFrom(type))
                    foreach(object obj in result.Properties[attrib])
                        Console.WriteLine(obj);
                else if(typeof(Int64).IsAssignableFrom(type)) {
                    DateTime dt = DateTime.FromFileTime((Int64)result.Properties[attrib][0]);
                    Console.WriteLine(dt);
                }
                else if(typeof(byte[]).IsAssignableFrom(type)) {
                    byte[] sid = (byte[])result.Properties[attrib][0];
                    Console.WriteLine(getSid(sid));
                }
                else
                    Console.WriteLine("Unable to display {0} of type {1}", attrib, result.Properties[attrib][0].GetType());
            }
            catch (ArgumentOutOfRangeException) {
                Console.WriteLine("Attribute {0} is not available", attrib);
            }

        }

        static string getSid(byte[] sid) {
            StringBuilder retval = new StringBuilder("S-");
            retval.Append(sid[0].ToString());

            if(sid[6] != 0 || sid[5] != 0) {
                string auth = string.Format("0x{0:2x}{1:2x}{2:2x}{3:2x}{4:2x}{5:2x}", (Int16)sid[1], (Int16)sid[2], (Int16)sid[3], (Int16)sid[4], (Int16)sid[5], (Int16)sid[6]);
                retval.Append("-");
                retval.Append(auth);
            }
            else {
                Int64 iVal = (Int32)sid[1] + (Int32)(sid[2] << 8) + (Int32)(sid[3] << 16) + (Int32)(sid[4] << 24);
                retval.Append("-");
                retval.Append(iVal.ToString());
            }

            int subCount = (Int32)sid[7];
            int idxAuth = 0;
            for(int i = 0; i < subCount; i++) {
                idxAuth = 8 + i * 4;
                UInt32 subAuth = BitConverter.ToUInt32(sid, idxAuth);
                retval.Append("-");
                retval.Append(subAuth.ToString());
            }


            return retval.ToString();
        }
        
    }
}
