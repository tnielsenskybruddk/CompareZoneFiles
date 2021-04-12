using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Management.Automation;

namespace CompareZoneFiles
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = ConfigurationManager.AppSettings["Host"];
            var ns1 = ConfigurationManager.AppSettings["Ns1"];
            var ns2 = ConfigurationManager.AppSettings["Ns2"];
            var aPath = ConfigurationManager.AppSettings["APath"];
            var cnamePath = ConfigurationManager.AppSettings["CnamePath"];
            var txtPath = ConfigurationManager.AppSettings["TxtPath"];

            var aLines = System.IO.File.ReadAllLines(aPath);
            foreach (var line in aLines)
            {
                if (line.Contains("\t"))
                {
                    var domain = line.Split('\t')[0] == "@" ? host : line.Split('\t')[0] + "." + host;
                    Console.Title = "A " + domain;
                    if (GetRequest(ns1, "a", domain, "Address").ToString() != GetRequest(ns2, "a", domain, "Address").ToString())
                    {
                        Console.WriteLine(domain);
                    }
                }
            }

            var cnameLines = System.IO.File.ReadAllLines(cnamePath);
            foreach (var line in cnameLines)
            {
                if (line.Contains("\t"))
                {
                    var domain = line.Split('\t')[0] == "@" ? host : line.Split('\t')[0] + "." + host;
                    Console.Title = "CNAME " + domain;
                    if (GetRequest(ns1, "cname", domain, "NameHost").ToString() != GetRequest(ns2, "cname", domain, "NameHost").ToString())
                    {
                        Console.WriteLine(domain);
                    }
                }
            }

            var txtLines = System.IO.File.ReadAllLines(txtPath);
            foreach (var line in txtLines)
            {
                if (line.Contains("\t"))
                {
                    var domain = line.Split('\t')[0] == "@" ? host : line.Split('\t')[0] + "." + host;
                    Console.Title = "TXT " + domain;
                    if (GetRequest(ns1, "txt", domain, "Strings").ToString() != GetRequest(ns2, "txt", domain, "Strings").ToString())
                    {
                        Console.WriteLine(domain);
                    }
                    foreach (var txt in JArray.FromObject(GetRequest(ns1, "txt", domain, "Strings")))
                    {
                        Console.WriteLine(txt);
                    }
                    foreach (var txt in JArray.FromObject(GetRequest(ns2, "txt", domain, "Strings")))
                    {
                        Console.WriteLine(txt);
                    }
                }
            }

            Console.Read();
        }

        static object GetRequest(string server, string type, string name, string text)
        {
            object value = "";
            using (PowerShell PowerShellInst = PowerShell.Create())
            {
                PowerShellInst.AddScript("resolve-dnsname -server " + server + " -type " + type + " -name " + name);
                Collection<PSObject> PSOutput = PowerShellInst.Invoke();
                foreach (PSObject obj in PSOutput)
                {
                    if (obj != null)
                    {
                        foreach (var property in obj.Properties)
                        {
                            if (property.Name == text)
                            {
                                value = property.Value;
                            }
                            //Console.WriteLine(property.Name + ":" + property.Value);
                        }
                    }
                    break;
                }
            }
            return value;
        }
    }
}
