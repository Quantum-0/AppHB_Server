using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;
using System.Xml.Serialization;

namespace WebServiceTest2
{
    [ScriptService]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [WebService(Description = "Server's API for TTTM", Namespace = "quantum0.apphb.com", Name = "TTTM API")]
    public class TTTMApi : WebService
    {
        private static List<Server> Servers = new List<Server>();

        [WebMethod(Description = "Получение списка серверов")]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json, UseHttpGet = true)]
        public List<Server> Get()
        {
            return Servers;
        }

        [WebMethod(Description = "Добавление своего сервера в список серверов")]
        public bool Add(string Name, int Port)
        {
            var request = this.Context.Request;
            var IP = request.UserHostAddress;
            Servers.Add(new Server(IP, Name, Port));
            return true;
        }
    }

    public class Server
    {
        public string IP { get; set; }
        public string Name { get; set; }
        public int Port { get; set; }

        public Server()
        {

        }
        public Server(string IP, string Name, int Port)
        {
            this.IP = IP;
            this.Name = Name;
            this.Port = Port;
        }
    }
}