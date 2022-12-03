using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.OData.Edm;
using NPOI.SS.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using transaction_projBak.DAL;
using transaction_projBak.Model;

namespace transaction_projBak.Controllers
{
    [ApiController]
    [Route("api/transaction")]
    public class TransactionController : Controller
    {
        public readonly ITransactionService _transactionService;
        string filePath;
        public TransactionController(ITransactionService transactionService, IConfiguration config)
        {
            this._transactionService = transactionService;
            filePath = config.GetValue<string>("fileUpload:filepath");
        }

        [HttpPost]
        [Route("uploadFile")]
        public IActionResult uploadFileAsync(FileUpload model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            else
            {
                List<Transaction> tranList = new List<Transaction>();
                if (model.fileType == "csv")
                {
                    byte[] data = Convert.FromBase64String(model.fileUrl);
                    string decodedString = Encoding.UTF8.GetString(data);
                    string[] stringSeparators = new string[] { "\r\n" };
                    string[] lines = decodedString.Split(stringSeparators, StringSplitOptions.None);
                    foreach (string s in lines)
                    {
                        MatchCollection matches = new Regex("((?<=\")[^\"]*(?=\"(,|$)+)|(?<=,|^)[^,\"]*(?=,|$))").Matches(s);
                        if (matches.Count > 5)
                        {
                            return BadRequest();
                        }
                        else
                        {
                            if ((String.IsNullOrEmpty(matches[0].ToString()) || String.IsNullOrWhiteSpace(matches[0].ToString())) ||
                                   (String.IsNullOrEmpty(matches[1].ToString()) || String.IsNullOrWhiteSpace(matches[1].ToString()))
                                || (String.IsNullOrEmpty(matches[2].ToString()) || String.IsNullOrWhiteSpace(matches[2].ToString()))
                                || (String.IsNullOrEmpty(matches[3].ToString()) || String.IsNullOrWhiteSpace(matches[3].ToString()))
                                || (String.IsNullOrEmpty(matches[4].ToString()) || String.IsNullOrWhiteSpace(matches[4].ToString())))
                            {
                                return StatusCode(StatusCodes.Status406NotAcceptable, new Response { Status = "Error", Message = "Invalid record" });
                            }
                            else
                            {
                                Transaction objTransaction = new Transaction();
                                objTransaction.transactionId = matches[0].ToString();
                                objTransaction.amount = Convert.ToDecimal(matches[1].ToString());
                                objTransaction.currencyCode = matches[2].ToString();
                                objTransaction.transactionDate = DateTime.ParseExact(matches[3].ToString(), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                                objTransaction.status = matches[4].ToString();
                                objTransaction.fileType = (int)FileType.CSV;
                                tranList.Add(objTransaction);
                            }
                        }
                    }
                    bool insData = _transactionService.InsertData(tranList);
                    if (insData)
                    {
                        return StatusCode(StatusCodes.Status200OK, new Response { Status = "Success", Message = "Import Success" });
                    }
                    else
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Import Fail" });
                    }
                }
                if (model.fileType == "xml")
                {
                    var myfilename = string.Format(@"{0}", Guid.NewGuid());
                    if (!Directory.Exists(filePath))
                    {
                        Directory.CreateDirectory(filePath);
                    }
                    string filepath = filePath + myfilename + "." + model.fileType;
                    var bytess = Convert.FromBase64String(model.fileUrl);
                    using (var dataFile = new FileStream(filepath, FileMode.Create))
                    {
                        dataFile.Write(bytess, 0, bytess.Length);
                        dataFile.Flush();
                    }
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(filepath);
                    XmlNodeList nodeList = xmlDoc.DocumentElement.SelectNodes("/Transactions/Transaction");
                    string transactionId, status, stDate, amt, currencyCode;
                    Decimal amount;
                    DateTime transactionDate;
                    foreach (XmlNode node in nodeList)
                    {
                        transactionId = node.Attributes["id"].Value;
                        status = node.SelectSingleNode("Status").InnerText;
                        stDate = node.SelectSingleNode("TransactionDate").InnerText;
                        amt = node.SelectSingleNode("PaymentDetails").SelectSingleNode("Amount").InnerText;
                        currencyCode = node.SelectSingleNode("PaymentDetails").SelectSingleNode("CurrencyCode").InnerText;
                        if ((String.IsNullOrEmpty(transactionId) || String.IsNullOrWhiteSpace(transactionId)) ||
                           (String.IsNullOrEmpty(status) || String.IsNullOrWhiteSpace(status)) ||
                           (String.IsNullOrEmpty(stDate) || String.IsNullOrWhiteSpace(stDate)) ||
                           (String.IsNullOrEmpty(currencyCode) || String.IsNullOrWhiteSpace(currencyCode)) ||
                           (String.IsNullOrEmpty(amt) || String.IsNullOrWhiteSpace(amt)))
                        {
                            return StatusCode(StatusCodes.Status406NotAcceptable, new Response { Status = "Error", Message = "Invalid record" });
                        }
                        else
                        {
                            Transaction objTransaction = new Transaction();
                            SimpleDateFormat outputFormat = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss");
                            transactionDate = outputFormat.Parse(stDate);
                            amount = Convert.ToDecimal(amt);
                            objTransaction.transactionId = transactionId;
                            objTransaction.amount = amount;
                            objTransaction.currencyCode = currencyCode;
                            objTransaction.transactionDate = transactionDate;
                            objTransaction.status = status;
                            objTransaction.fileType = (int)FileType.XML;
                            tranList.Add(objTransaction);
                        }
                    }
                    bool insData = _transactionService.InsertData(tranList);
                    if (insData)
                    {
                        return StatusCode(StatusCodes.Status200OK, new Response { Status = "Success", Message = "Import Success" });
                    }
                    else
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Import Fail" });
                    }
                }
            }
            return BadRequest();
        }
        [HttpGet]
        [Route("getByParams")]
        public List<TransactionDTO> getByParams(string currency, string fromDate, string toDate, string status)
        {
            List<TransactionDTO> tranList =  _transactionService.getList(currency, fromDate, toDate, status);
            if(tranList.Count <= 0)
            {
                return null;
            }    
            List<TransactionDTO> rettranList = new List<TransactionDTO>();
            foreach (TransactionDTO objDto in tranList)
            {
                TransactionDTO dto = new TransactionDTO();
                dto.id = objDto.id;
                dto.payment = objDto.payment;
                if (objDto.Status == "Approved")
                {
                    dto.Status = "A";
                }
                else if(objDto.Status == "Failed" || objDto.Status == "Rejected")
                {
                    dto.Status = "R";
                }
                else
                {
                    dto.Status = "D";
                }
                rettranList.Add(dto);
            }
            return rettranList;
        }
    }
}
