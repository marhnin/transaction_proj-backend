using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace transaction_projBak.Model
{
    public class Transaction
    {
        public int id { get; set; }
        public string transactionId { get; set; }
        public Decimal amount { get; set; }
        public string currencyCode { get; set; }
        public DateTime transactionDate { get; set; }
        public string status { get; set; }
        public int fileType { get; set; }
    }
    public class FileUpload
    {
        [Required(ErrorMessage = "FileUrl is required")]
        public string fileUrl { get; set; }

        [Required(ErrorMessage = "File type is required")]
        public string fileType { get; set; }
    }
    public class Response
    {
        public string Status { get; set; }
        public string Message { get; set; }
    }
    public enum FileType { CSV = 0, XML = 1};
    public class TransactionDTO
    {
        public string id { get; set; }
        public string Status { get; set; }
        public string payment { get; set; }
    }
}
