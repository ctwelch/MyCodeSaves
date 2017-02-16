using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicBridge.Models
{
    public class NoteAutomation : IAutomation
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string ToDoId { get; set; }
        public string NoteToInsert { get; set; }
        public void Run()
        {
            //model.ProcessEfiledDoc();
        }
    }


}
