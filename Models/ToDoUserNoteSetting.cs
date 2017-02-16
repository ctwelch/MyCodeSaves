using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicBridge.Models
{
    public class ToDoUserNoteSetting
    {
        public int Id { get; set; }
        public string ToDoId { get; set; }
        public string UserId { get; set; }
        public string ActCode { get; set; }

    }
}
