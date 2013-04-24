using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCrawler
{
    class Program
    {
        static void Main(string[] args)
        {
            String path = @"C:\Users\Murillo\Temp\{0}.txt";
            Crawler c = new Crawler();

            Random r = new Random();
            string nameFile = String.Format(path, "file-" + r.Next(100, 999999));
            StreamWriter file = new StreamWriter(nameFile, true);

            foreach (String item in c.listUrl)
            {
                Console.WriteLine(item);
                file.WriteLine(item.TrimEnd('"'));
                c.ContentSite(item, 80, file);
                file.WriteLine("");
            }

            file.Close();
            file.Dispose();

            Console.ReadKey();
        }
    }
}
