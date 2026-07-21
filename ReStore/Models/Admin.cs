using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace ReStore.Models
{
    public class Admin
    {
        public int Id { get; set; }
        public string AdminAd { get; set; }
        public string AdminSifre { get; set; }
        public string AdminEmail { get; set; }  
    }
}
