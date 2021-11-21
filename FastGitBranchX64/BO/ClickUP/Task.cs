using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastGitBranchX64.BO.ClickUP
{
    public class Task
    {
        public string id { get; set; }
        public string custom_id { get; set; }
        public string name { get; set; }
        public string text_content { get; set; }
        public string description { get; set; }
        public Status status { get; set; }
        
    }
}
