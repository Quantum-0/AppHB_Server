using System;
using System.Collections.Generic;
using System.IO;
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
            Servers.RemoveAll(s => DateTime.Now.Subtract(s.GetLastUpdate()).TotalSeconds > 60);
            var Result = new List<Server>();
            for (int i = 0; i < Servers.Count; i++)
            {
                Servers[i].id = i;

                //if (!Servers[i].getWanted())
                    Result.Add(Servers[i]);
            }
            return Result;
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

            // Проверки всякие
            if (Servers.Count(s => s.IP == IP) > 5)
                return null;
            if (Servers.Count(s => s.ServerName == ServerName) > 2)
                return null;

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
            catch (Exception)
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
            {
                SearchResult[0].Update();
                return SearchResult[0].getWanted();
            }
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
        public ServerReadyResult ReadReady(string PublicKey)
        {
            var SearchResult = Servers.FindAll(s => s.PublicKey == PublicKey);
            if (SearchResult.Count == 1)
            {
                if (SearchResult[0].Port != 0)
                    return new ServerReadyResult() { Ready = true, IP = SearchResult[0].IP, Port = SearchResult[0].Port };
                else
                    return default(ServerReadyResult);
            }
            else
                return default(ServerReadyResult);
        }

        [WebMethod(Description = "Запись клиентского EP")]
        //[ScriptMethod(UseHttpGet = true)]
        public bool WriteClientEP(string PublicKey, string IP, int Port)
        {
            var SearchResult = Servers.FindAll(s => PublicKey == s.PublicKey);
            if (SearchResult.Count == 1)
            {
                SearchResult[0].setClient(new Client() { IP = IP, Port = Port});
                return true;
            }
            else
                return false;
        }

        [WebMethod(Description = "Чтение IP клиента")]
        [ScriptMethod(UseHttpGet = true)]
        public Client ReadClientEP(string AccessKey)
        {
            var SearchResult = Servers.FindAll(s => s.CheckAK(AccessKey));
            if (SearchResult.Count == 1)
            {
                return SearchResult[0].getClient();
            }
            else
                return null;
        }
        
        [WebMethod(Description = "Обновление сервера (очистка)")]
        [ScriptMethod(UseHttpGet = true)]
        public bool Clear(string AccessKey)
        {
            var SearchResult = Servers.FindAll(s => s.CheckAK(AccessKey));
            if (SearchResult.Count == 1)
            {
                SearchResult[0].Clear();
                return true;
            }
            else
                return false; ;
        }

        [WebMethod(Description = "Получение MD5 последней версии")]
        [ScriptMethod(UseHttpGet = true)]
        public UpdData GetUpdatingData()
        {
            MD5 md5 = MD5.Create();
            var path = Server.MapPath("/TTTM.exe");
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                var hash = md5.ComputeHash(fs);
                var sb = new StringBuilder(hash.Length * 2);
                foreach (byte b in hash)
                    sb.Append(b.ToString("X2"));
                var dt = File.GetLastWriteTimeUtc(path);
                return new UpdData() { Hash = sb.ToString(), Date = dt };
            }
        }

        /*[HttpGet]
        public HttpResponseMessage GetUpdate()
        {
            var Path = "";// Server.MapPath()
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            var stream = new FileStream(Path, FileMode.Open);

        }*/
    }

    public class CreatingResult
    {
        public bool Created { get; set; } = true;
        public string AccessKey { get; set; }
    }

    public class UpdData
    {
        public string Hash;
        public DateTime Date;
    }

    public class RemovingResult
    {
        public bool Result;
    }

    public struct ServerReadyResult
    {
        public bool Ready;
        public string IP;
        public int Port;
    }

    public class Client
    {
        public string IP { get; set; }
        public int Port { get; set; }
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
        private DateTime UpdateDate { get; set; }
        public string ServerName { get; set; }
        private bool Wanted;
        private Client Client;

        public void Update()
        {
            UpdateDate = DateTime.Now;
        }
        public DateTime GetLastUpdate()
        {
            return UpdateDate;
        }

        public void setWanted()
        {
            Wanted = true;
        }
        public bool getWanted()
        {
            return Wanted;
        }

        public void setClient(Client Client)
        {
            this.Client = Client;
        }
        public Client getClient()
        {
            return Client;
        }

        public bool CheckAK(string AK)
        {
            return (AccessKey == AK);
        }

        public bool Clear()
        {
            Port = 0;
            Wanted = false;
            Client = null;
            return true;
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
            UpdateDate = DateTime.Now;
        }
    }
}