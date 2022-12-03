using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using transaction_projBak.Model;
using transaction_projBak.Util;

namespace transaction_projBak.DAL
{
    public class TransactionService : ITransactionService
    {
        public bool InsertData(List<Transaction> tList)
        {
            StringBuilder sqlStr = new StringBuilder();
            SqlCommand cmd = new SqlCommand();
            for (int i = 0; i < tList.Count; i++)
            {
                Transaction objDeail = (Transaction)tList[i];
                sqlStr.Append("INSERT INTO tb_transaction (transactionId, amount,currencyCode,transactionDate,status,fileType) VALUES(@transactionId" + i.ToString() + ",@amount" + i.ToString() + ",@currencyCode" + i.ToString() + ",@transactionDate" + i.ToString() + ",@status" + i.ToString() + ",@fileType" + i.ToString() + ")");
                cmd.Parameters.Add(new SqlParameter("@transactionId"+i.ToString(), objDeail.transactionId));
                cmd.Parameters.Add(new SqlParameter("@amount" + i.ToString(), objDeail.amount));
                cmd.Parameters.Add(new SqlParameter("@currencyCode" + i.ToString(), objDeail.currencyCode));
                cmd.Parameters.Add(new SqlParameter("@transactionDate" + i.ToString(), objDeail.transactionDate));
                cmd.Parameters.Add(new SqlParameter("@status" + i.ToString(), objDeail.status));
                cmd.Parameters.Add(new SqlParameter("@fileType" + i.ToString(), objDeail.fileType));
            }
            cmd.CommandText = sqlStr.ToString();
            return new SqlHelper().ExecuteQuery(cmd);
        }
    }
}
