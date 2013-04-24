using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace WebCrawler
{
    class Crawler
    {
        TcpClient tc;
        int ultimoNivel = 2;
        public List<String> listUrl { get; private set; }

        public Crawler()
        {
            listUrl = new List<string>();
            StartCrawler("www.portalgamearts.com", "/", 80, 1);
        }

        private void StartCrawler(String site, String pagina, int porta, int nivel)
        {
            tc = new TcpClient();

            try
            {
                if (site.Contains("http"))
                    tc.Connect(site.Split('/')[2], porta);
                else
                    tc.Connect(site.Split('/')[0], porta);

                using (NetworkStream ns = tc.GetStream())
                {
                    using (StreamWriter sw = new StreamWriter(ns))
                    {
                        using (StreamReader sr = new StreamReader(ns))
                        {
                            StringBuilder str = new StringBuilder();
                            str.Append("");
                            str.AppendFormat("GET {0} HTTP/1.0\r\n", pagina);
                            if (site.Contains("http")) str.AppendFormat("Host: {0}\r\n", site.Split('/')[2]);
                            else str.AppendFormat("Host: {0}\r\n", site.Split('/')[0]);
                            str.Append("\r\n");

                            sw.Write(str);
                            sw.Flush();

                            if (nivel <= ultimoNivel)
                            {
                                while (sr.Peek() >= 0)
                                {
                                    String url = GetNextTarget(sr.ReadLine());
                                    if (!String.IsNullOrEmpty(url) && url.IndexOf('@') == -1 && url.IndexOf('#') == -1 && url.IndexOf("javascript:void(0)") == -1)
                                    {
                                        if (url.Contains("http"))
                                        {
                                            int posicao = url.IndexOf('/', 9);

                                            if (posicao != -1)
                                            {
                                                AddSite(url.TrimEnd('"'));
                                                StartCrawler(url.TrimEnd('"'), url.Substring(posicao), 80, nivel + 1);
                                            }
                                            else
                                            {
                                                AddSite(url.TrimEnd('"') + "/");
                                                StartCrawler(url.TrimEnd('"'), "/", 80, nivel + 1);
                                            }
                                        }
                                        else
                                        {
                                            AddSite(site + url.TrimEnd('"', '/'));
                                            StartCrawler(site + url.TrimEnd('"', '/'), url.TrimEnd('"'), 80, nivel + 1);

                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                tc.Close();
            }
            catch
            {

            }
        }

        private string GetNextTarget(String page)
        {
            string url = String.Empty;
            int start_link = page.IndexOf("<a href=");
            if (start_link != -1)
            {
                int start_quote = page.IndexOf('"', start_link);
                int end_quote = page.IndexOf('"', start_quote + 1);
                url = page.Substring(start_quote + 1, (end_quote - start_quote));
            }

            return url;
        }

        private void AddSite(string url)
        {
            string site = string.Empty;

            if (url.Contains("http") || url.Contains("https"))
            {
                site = url.Substring(url.IndexOf("/", 0)).TrimStart('/');
            }
            else
                site = url;

            if (!listUrl.Contains(site))
            {
                listUrl.Add(site.TrimEnd('/'));
            }
        }

        public void ContentSite(String site, int porta, StreamWriter file)
        {
            tc = new TcpClient();

            try
            {
                tc.Connect(site.IndexOf('/') != -1 ? site.Split('/')[0] : site, porta);
                using (NetworkStream ns = tc.GetStream())
                {
                    using (StreamWriter sw = new StreamWriter(ns))
                    {
                        using (StreamReader sr = new StreamReader(ns))
                        {
                            string pagina = site.IndexOf('/') != -1 ? site.Substring(site.IndexOf('/', 8)) : "/";

                            StringBuilder str = new StringBuilder();
                            str.Append("");
                            str.AppendFormat("GET {0} HTTP/1.0\r\n", pagina);
                            if (site.Contains("http")) str.AppendFormat("Host: {0}\r\n", site.Split('/')[2]);
                            else str.AppendFormat("Host: {0}\r\n", site.Split('/')[0]);
                            str.Append("\r\n");

                            sw.Write(str);
                            sw.Flush();

                            Console.WriteLine("[Processando...]");
                            while (sr.Peek() >= 0)
                            {
                                file.WriteLine(sr.ReadLine());
                                Console.WriteLine(sr.ReadLine());
                            }
                        }
                    }
                }

                tc.Close();
                Console.WriteLine("[Final!]");
            }
            catch
            {
                file.WriteLine("Conexão falhou!");
                Console.WriteLine("Conexão falhou!");
            }
        }
    }
}
