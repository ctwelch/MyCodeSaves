using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicBridge.Models
{
    public class TranTextSegment: ITranTextSegment
    {
        public int Id { get; set; }
        public string ToDoId { get; set; }
        public string UserId { get; set; }
        public string SegmentType { get; set; }
        public int SegmentOrder { get; set; }

        // a tran text segment is either a :
        // constantString
        // userInput
        // property on a Todo

    }

    public class ConstantTranTextSegment : TranTextSegment, ITranTextSegment
    {
        public string ConstantText { get; set; }
    }

    public class UserInputTranTextSegment : TranTextSegment, ITranTextSegment
    {
        public string InputLabel { get; set; }
        public string InputValue { get; set; }
        public List<UserInput> UserInputs { get; set; }
    }

    public class ToDoPropertyTranTextSegment : TranTextSegment, ITranTextSegment
    {
        public string ToDoPropertyName { get; set; }
        public int ToDoTranNum { get; set; }
    }


    public class UserInput
    {
        public int Id { get; set; }
        public string ToDoId { get; set; }
        public string UserId { get; set; }
        public string InputLabel { get; set; }
        public string InputValue { get; set; }
        public int EntityNum { get; set; }
    }
}
