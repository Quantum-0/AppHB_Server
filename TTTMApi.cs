using System;
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
            return Servers;
        }

        [WebMethod(Description = "Добавление своего сервера в список серверов")]
        [ScriptMethod(UseHttpGet = true)]
        public CreatingResult Add(string Name, int Port, int Color)
        {
            var request = this.Context.Request;
            var IP = string.Empty;
            if (request.ServerVariables.AllKeys.Contains("HTTP_CLIENT_IP"))
                IP = request.ServerVariables["HTTP_CLIENT_IP"];
            else if (request.ServerVariables.AllKeys.Contains("HTTP_X_FORWARDED_FOR"))
                IP = request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            else
                IP = request.ServerVariables["REMOTE_ADDR"];//.UserHostAddress;
            var AK = Hash(Name + IP + Port.ToString() + DateTime.Now.ToString());
            Servers.Add(new Server(IP, Name, Port, Color, AK));
            var ping = PingHost("127.0.0.1", Port);
            var Result = new CreatingResult() { Created = true, Ping = ping, AccessKey = AK };
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

        static string Hash(string input)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    // can be "x2" if you want lowercase
                    sb.Append(b.ToString("X2"));
                }

                return sb.ToString();
            }
        }
    }

    public class CreatingResult
    {
        public bool Created { get; set; }
        public bool Ping { get; set; }
        public string AccessKey { get; set; }
    }

    public class RemovingResult
    {
        public bool Result;
    }

    public class Server
    {
        public string IP { get; set; }
        public string Name { get; set; }
        public int Port { get; set; }
        private string AccessKey;
        public int Color { get; set; } 
        public DateTime CreationDate { get; set; }

        public bool CheckAK(string AK)
        {
            return (AccessKey == AK);
        }

        public Server()
        {

        }

        public Server(string IP, string Name, int Port, int Color, string AK)
        {
            this.IP = IP;
            this.Name = Name;
            this.Color = Color;
            this.Port = Port;
            this.AccessKey = AK;
            CreationDate = DateTime.Now;
        }
    }


}