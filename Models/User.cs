using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicBridge.Models
{
    public class User
    {
        //private static User _instance;
        //public static User Instance
        //{
        //    get
        //    {
        //        return _instance ?? (_instance = new User());
        //    }            
        //}
        //User()
        //{

        //}
        public string UserId { get; set; }
        public int EntityNum { get; set; }
    }
}
