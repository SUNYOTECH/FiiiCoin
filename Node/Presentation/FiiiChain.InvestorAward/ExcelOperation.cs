using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using OfficeOpenXml;

namespace FiiiChain.InvestorAward
{
    public class ExcelOperation
    {
        public List<Investor> ReadExcelFile()
        {
            var fileDownloadName = "setting.xlsx";
            var fileInfo = new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), fileDownloadName));
            List<Investor> list = new List<Investor>();
            using (ExcelPackage package = new ExcelPackage(fileInfo))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets["Sheet1"];
                var rowCount = worksheet.Dimension?.Rows;
                var colCount = worksheet.Dimension?.Columns;

                if (!rowCount.HasValue || !colCount.HasValue)
                {
                    return null;
                }
                //因为第一行是表头信息，所以从第二行开始
                for (int row = 2; row <= rowCount.Value; row++)
                {
                    Investor investor = new Investor();
                    for (int col = 1; col <= colCount.Value; col++)
                    {
                        investor.Name = worksheet.Cells[row, 1].Value.ToString();
                        investor.Address = worksheet.Cells[row, 2].Value.ToString();
                        investor.Amount = Convert.ToInt32(worksheet.Cells[row, 3].Value);
                        investor.Phone = worksheet.Cells[row, 4].Value.ToString();
                    }
                    list.Add(investor);
                }
            }
            return list;
        }
    }
}
