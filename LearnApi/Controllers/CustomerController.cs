using AutoMapper;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Vml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using LearnApi.Model;
using LearnApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.VisualBasic.FileIO;
using System.Data;

namespace LearnApi.Controllers
{
    [Authorize]
    [EnableRateLimiting("fixedwindow")]
    // if we apply cors only on specific controller then we will add this attribute
    [EnableCors("corPolicy")]
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public CustomerController(ICustomerService customerService, IWebHostEnvironment webHostEnvironment)
        {
            _customerService = customerService;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet("GetAllStudents")]
        public async Task<IActionResult> GetAllStudents()
        {
            var data = await _customerService.GetAll();
            if (data.Count == 0)
            {
                return NotFound();
            }
            return Ok(data);
        }

        [HttpGet("GetStudentById")]
        public async Task<IActionResult> GetStudentById(int id)
        {
            var data = await _customerService.GetStudentById(id);
            if (data == null)
            {
                return NotFound();
            }
            return Ok(data);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create(CustomerModel model)
        {
            var data = await _customerService.Create(model);
            return Ok(data);
        }

        [HttpPut("Update")]
        public async Task<IActionResult> Update(CustomerModel model, int id)
        {
            var data = await _customerService.Update(model,id);
            return Ok(data);
        }

        [HttpDelete("Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _customerService.Remove(id);
            return Ok(data);
        }

        [AllowAnonymous]
        [HttpGet("ExportExcel")]
        public async Task<IActionResult> ExportExcel()
        {
            try
            {
                DataTable dataTable = new DataTable();
                dataTable.Columns.Add("Id",typeof(int));
                dataTable.Columns.Add("Name", typeof(string));
                dataTable.Columns.Add("Age", typeof(int));
                dataTable.Columns.Add("RollNumber", typeof(string));
                var getAllCutomers = await _customerService.GetAll();
                if (getAllCutomers != null && getAllCutomers.Count > 0)
                {
                    getAllCutomers.ForEach(item =>
                    {
                        dataTable.Rows.Add(item.Id,item.Name,item.Age,item.RollNumber);
                    });
                }
                else
                {
                    return NotFound();
                }
                using (XLWorkbook workBook = new XLWorkbook())
                {
                    workBook.AddWorksheet(dataTable, "Customer");
                    using (MemoryStream stream = new MemoryStream())
                    {
                        workBook.SaveAs(stream);
                        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Customer.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        [AllowAnonymous]
        [HttpGet("SaveExportExcelOnServer")]
        public async Task<IActionResult> SaveExportExcelOnServer()
        {
            try
            {
                DataTable dataTable = new DataTable();
                dataTable.Columns.Add("Id", typeof(int));
                dataTable.Columns.Add("Name", typeof(string));
                dataTable.Columns.Add("Age", typeof(int));
                dataTable.Columns.Add("RollNumber", typeof(string));
                var getAllCutomers = await _customerService.GetAll();
                if (getAllCutomers != null && getAllCutomers.Count > 0)
                {
                    getAllCutomers.ForEach(item =>
                    {
                        dataTable.Rows.Add(item.Id, item.Name, item.Age, item.RollNumber);
                    });
                }
                else
                {
                    return NotFound();
                }
                using (XLWorkbook workBook = new XLWorkbook())
                {
                    workBook.AddWorksheet(dataTable, "Customer");
                    var filePath = _webHostEnvironment.WebRootPath + "\\Export\\Customer.xlsx";
                    using (FileStream stream = System.IO.File.Create(filePath))
                    {
                        workBook.SaveAs(stream);
                        //if (System.IO.File.Exists(filePath))
                        //{
                        //    System.IO.File.Delete(filePath);
                        //}
                        return Ok("File saved to "+ filePath);
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }
    }
}
