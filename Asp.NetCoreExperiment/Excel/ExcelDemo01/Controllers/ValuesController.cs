﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace ExcelDemo01.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            string filePath = $"{Directory.GetCurrentDirectory()}/test.xlsx";
            var file = new FileInfo(filePath);
            using (var package = new ExcelPackage(file))
            {
                var sheet = package.Workbook.Worksheets[1];
            }

            return new string[] { "value1", "value2" };
        }

        #region
        private static ExcelPackage CreateExcelPackage<T>(List<T> datas, Dictionary<string, string> columnNames, List<string> outOfColumns, string sheetName = "Sheet1", string title = "", int isProtected = 0)
        {
            var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add(sheetName);
            if (isProtected == 1)
            {
                worksheet.Protection.IsProtected = true;//设置是否进行锁定
                worksheet.Protection.SetPassword("xiangzhidaomimama");//设置密码
                worksheet.Protection.AllowAutoFilter = false;//下面是一些锁定时权限的设置
                worksheet.Protection.AllowDeleteColumns = false;
                worksheet.Protection.AllowDeleteRows = false;
                worksheet.Protection.AllowEditScenarios = false;
                worksheet.Protection.AllowEditObject = false;
                worksheet.Protection.AllowFormatCells = false;
                worksheet.Protection.AllowFormatColumns = false;
                worksheet.Protection.AllowFormatRows = false;
                worksheet.Protection.AllowInsertColumns = false;
                worksheet.Protection.AllowInsertHyperlinks = false;
                worksheet.Protection.AllowInsertRows = false;
                worksheet.Protection.AllowPivotTables = false;
                worksheet.Protection.AllowSelectLockedCells = false;
                worksheet.Protection.AllowSelectUnlockedCells = false;
                worksheet.Protection.AllowSort = false;
            }

            var titleRow = 0;
            if (!string.IsNullOrWhiteSpace(title))
            {
                titleRow = 1;
                worksheet.Cells[1, 1, 1, columnNames.Count()].Merge = true;//合并单元格
                worksheet.Cells[1, 1].Value = title;
                worksheet.Cells.Style.WrapText = true;
                worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;//水平居中
                worksheet.Cells[1, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;//垂直居中
                worksheet.Row(1).Height = 30;//设置行高
                worksheet.Cells.Style.ShrinkToFit = true;//单元格自动适应大小
            }

            //获取要反射的属性,加载首行
            Type myType = typeof(T);
            List<PropertyInfo> myPro = new List<PropertyInfo>();
            int i = 1;
            foreach (string key in columnNames.Keys)
            {
                PropertyInfo p = myType.GetProperty(key);
                myPro.Add(p);

                worksheet.Cells[1 + titleRow, i].Value = columnNames[key];
                i++;
            }

            int row = 2 + titleRow;
            foreach (T data in datas)
            {
                int column = 1;
                foreach (PropertyInfo p in myPro.Where(info => !outOfColumns.Contains(info.Name)))
                {
                    worksheet.Cells[row, column].Value = p == null ? "" : Convert.ToString(p.GetValue(data, null));
                    column++;
                }
                row++;
            }
            return package;
        }

        public static Byte[] GetByteToExportExcel<T>(List<T> datas, Dictionary<string, string> columnNames, List<string> outOfColumn, string sheetName = "Sheet1", string title = "", int isProtected = 0)
        {
            using (var fs = new MemoryStream())
            {
                using (var package = CreateExcelPackage(datas, columnNames, outOfColumn, sheetName, title, isProtected))
                {
                    package.SaveAs(fs);
                    return fs.ToArray();
                }
            }
        }

        public async Task<IActionResult> ToExcel(int isProtected = 0)
        {
            var result = new List<dynamic>();
            var columns = new Dictionary<string, string>() {
                { "Id","序号"},
                { "UserName","用户名"},
                { "Remark","备注"}
            };
            var fs = GetByteToExportExcel(result, columns, new List<string>(), "Sheet1", "", isProtected);
            return File(fs, "application/vnd.android.package-archive", $"ExcelDemo.xlsx");
        }

        #endregion
    }
}
