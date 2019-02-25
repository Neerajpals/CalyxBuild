using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace calyxLoanProcessor.Model
{
    public class PostNewLoanRequest
    {
        public List<LoanField> loanFields { get; set; }
        public string folderName { get; set; }
        public string userName { get; set; }
        public string password { get; set; }
    }
}
