﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;
using System.Xml.Serialization;

namespace WebServiceTest2
{
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [WebService(Description = "Server's API for TTTM", Namespace = "quantum0.apphb.com", Name = "TTTM API")]
    [ScriptService]
    public class TTTMApiClass : WebService
    {
        private static List<Server> Servers = new List<Server>();        

        [WebMethod(Description = "Получение списка серверов")]
        [ScriptMethod(UseHttpGet = true)]
        public List<Server> Get()
        {
            Servers.RemoveAll(s => DateTime.Now.Subtract(s.CreationDate).TotalMinutes > 10);
            for (int i = 0; i < Servers.Count; i++)
            {
                Servers[i].id = i;

                if (Servers[i].Port != 0)
                    ; // Пропинговать

            }
            return Servers;
        }

        [WebMethod(Description = "Добавление своего сервера в список серверов")]
        [ScriptMethod(UseHttpGet = true)]
        public CreatingResult Add(string Name, string ServerName, int Color)
        {
            var request = this.Context.Request;
            var IP = string.Empty;
            if (request.ServerVariables.AllKeys.Contains("HTTP_CLIENT_IP"))
                IP = request.ServerVariables["HTTP_CLIENT_IP"];
            else if (request.ServerVariables.AllKeys.Contains("HTTP_X_FORWARDED_FOR"))
                IP = request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            else
                IP = request.ServerVariables["REMOTE_ADDR"];
            var AK = WebServiceTest2.Server.Hash(Name + IP + DateTime.Now.ToString() + '1');
            Servers.Add(new Server(IP, Name, ServerName, Color, AK));
            var Result = new CreatingResult() { AccessKey = AK };
            return Result;
        }

        [WebMethod(Description = "Удаление своего сервера из списка серверов")]
        [ScriptMethod(UseHttpGet = true)]
        public RemovingResult Remove(string AccessKey)
        {
            return new RemovingResult() { Result = Servers.RemoveAll(s => s.CheckAK(AccessKey)) > 0 };
        }

        [WebMethod(Description = "Пинг сервера")]
        [ScriptMethod(UseHttpGet = true)]
        public bool PingHost(string _HostURI, int _PortNumber)
        {
            return false;

            try
            {
                TcpClient client = new TcpClient();
                client.ReceiveTimeout = 1000;
                client.SendTimeout = 1000;
                client.Connect(_HostURI, _PortNumber);
                client.Close();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        [WebMethod(Description = "Запрос на подключение")]
        [ScriptMethod(UseHttpGet = true)]
        public bool WantConnect(string PublicKey)
        {
            var SearchResult = Servers.FindAll(s => PublicKey == s.PublicKey);
            if (SearchResult.Count == 1)
            {
                SearchResult[0].setWanted();
                return true;
            }
            else
                return false;
        }

        [WebMethod(Description = "Проверка, желает ли кто-то подключиться")]
        [ScriptMethod(UseHttpGet = true)]
        public bool GetWant(string AccessKey)
        {
            var SearchResult = Servers.FindAll(s => s.CheckAK(AccessKey));
            if (SearchResult.Count == 1)
                return SearchResult[0].getWanted();
            else
                return false;
        }

        [WebMethod(Description = "Запись готовности к подключению")]
        [ScriptMethod(UseHttpGet = true)]
        public bool WriteReady(string AccessKey, int Port)
        {
            var SearchResult = Servers.FindAll(s => s.CheckAK(AccessKey));
            if (SearchResult.Count == 1)
            {
                SearchResult[0].Port = Port;
                return true;
            }
            else
                return false;
        }

        [WebMethod(Description = "Чтение готовности к подключению")]
        [ScriptMethod(UseHttpGet = true)]
        public int ReadReady(string PublicKey)
        {
            var SearchResult = Servers.FindAll(s => s.PublicKey == PublicKey);
            if (SearchResult.Count == 1)
            {
                return SearchResult[0].Port;
            }
            else
                return 0;
        }
    }

    public class CreatingResult
    {
        public bool Created { get; set; } = true;
        public string AccessKey { get; set; }
    }

    public class RemovingResult
    {
        public bool Result;
    }

    public class Server
    {
        public int id { get; set; }
        public string IP { get; set; }
        public string Name { get; set; }
        public int Port { get; set; }
        private string AccessKey;
        public string PublicKey { get; set; }
        public int Color { get; set; } 
        public DateTime CreationDate { get; set; }
        public string ServerName { get; set; }
        private bool Wanted;

        public void setWanted()
        {
            Wanted = true;
        }
        public bool getWanted()
        {
            return Wanted;
        }

        public bool CheckAK(string AK)
        {
            return (AccessKey == AK);
        }

        public Server()
        {

        }

        public static string Hash(string input)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("X2"));
                }

                return sb.ToString();
            }
        }

        public Server(string IP, string Name, string ServerName, int Color, string AK)
        {
            this.IP = IP;
            this.Name = Name;
            this.ServerName = ServerName;
            this.Color = Color;
            this.AccessKey = AK;
            PublicKey = Hash(Name + IP + DateTime.Now.ToString() + '2');
            CreationDate = DateTime.Now;
        }
    }


}