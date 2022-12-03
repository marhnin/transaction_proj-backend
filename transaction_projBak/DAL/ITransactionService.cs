using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using transaction_projBak.Model;

namespace transaction_projBak.DAL
{
    public interface ITransactionService
    {
        public bool InsertData(List<Transaction> tList );
       
    }
}
