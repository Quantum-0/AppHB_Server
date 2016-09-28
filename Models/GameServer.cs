using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace AppHB_Server2.Models
{
    public class GameServer
    {
        public int ServerID { get; set; }
        [Required]
        public string Title { get; set; }
        public string IP { get; set; }
    }
}