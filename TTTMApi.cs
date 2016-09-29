using System;
using System.Collections.Generic;
using System.Linq;
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
            return Servers;
        }

        [WebMethod(Description = "Добавление своего сервера в список серверов")]
        [ScriptMethod(UseHttpGet = true)]
        public CreatingResult Add(string Name, int Port)
        {
            var request = this.Context.Request;
            var IP = string.Empty;
            if (request.ServerVariables.AllKeys.Contains("HTTP_CLIENT_IP"))
                IP = request.ServerVariables["HTTP_CLIENT_IP"];
            else if (request.ServerVariables.AllKeys.Contains("HTTP_X_FORWARDED_FOR"))
                IP = request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            else
                IP = request.ServerVariables["REMOTE_ADDR"];//.UserHostAddress;
            var AK = Hash(Name + IP + Port.ToString());
            Servers.Add(new Server(IP, Name, Port, AK));
            var Result = new CreatingResult() { Created = true, Ping = false, AccessKey = AK };
            return Result;
        }

        [WebMethod(Description = "Удаление своего сервера из списка серверов")]
        [ScriptMethod(UseHttpGet = true)]
        public bool Remove(string AccessKey)
        {
            return Servers.RemoveAll(s => s.AccessKey == AccessKey) > 0;
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

    public class Server
    {
        public string IP { get; set; }
        public string Name { get; set; }
        public int Port { get; set; }
        public string AccessKey { get; set; }

        public Server()
        {

        }
        public Server(string IP, string Name, int Port, string AK)
        {
            this.IP = IP;
            this.Name = Name;
            this.Port = Port;
            this.AccessKey = AK;
        }
    }


}