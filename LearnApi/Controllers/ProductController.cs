using LearnApi.Helper;
using LearnApi.Repos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices.Marshalling;
using System.Threading.RateLimiting;

namespace LearnApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ApplicationDbContext _context;
        public ProductController(IWebHostEnvironment webHostEnvironment, ApplicationDbContext context)
        {
            _webHostEnvironment = webHostEnvironment;
            _context = context;
        }

        [HttpPost("UploadImage")]
        public async Task<IActionResult> UploadImage(IFormFile formFile, string productCode)
        {
            APIResponse response = new APIResponse();
            try
            {
                string filePath = _webHostEnvironment.WebRootPath + "\\Upload\\Product\\" + productCode;
                if (!System.IO.Directory.Exists(filePath))
                {
                    System.IO.Directory.CreateDirectory(filePath);
                }

                string imagePath = filePath + "\\" + productCode + formFile.FileName;
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }

                using (FileStream stream = System.IO.File.Create(imagePath))
                {
                    await formFile.CopyToAsync(stream);
                    response.ResponseCode = 200;
                    response.Result = "OK";
                }
            }
            catch (Exception ex)
            {
                response.ResponseCode = 400;
                response.ErrorMessage = ex.Message;
                response.Result = "Fail";
            }

            return Ok(response);
        }

        [HttpPost("MultipleUploadImage")]
        public async Task<IActionResult> MultipleUploadImage(IFormFileCollection formFile, string productCode)
        {
            APIResponse response = new APIResponse();
            int passCount = 0; int errorCount = 0;
            try
            {
                string filePath = _webHostEnvironment.WebRootPath + "\\Upload\\Product\\" + productCode;
                if (!System.IO.Directory.Exists(filePath))
                {
                    System.IO.Directory.CreateDirectory(filePath);
                }

                foreach (var file in formFile)
                {
                    string imagePath = filePath + "\\" + productCode + file.FileName;
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }

                    using (FileStream stream = System.IO.File.Create(imagePath))
                    {
                        await file.CopyToAsync(stream);
                        passCount++;
                    }
                }

            }
            catch (Exception ex)
            {
                response.ErrorMessage = ex.Message;
                errorCount++;
            }

            response.ResponseCode = 200;
            response.Result = passCount + " files uploaded & " + errorCount + " files failed";
            return Ok(response);
        }

        [HttpGet("GetImage")]
        public async Task<IActionResult> GetImage(string productCode)
        {
            string imageUrl = "";
            string hostUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            try
            {
                var filePath = GetFilePath(productCode);
                var imagePath = filePath + "\\" + productCode + ".png";
                if (System.IO.File.Exists(imagePath))
                {
                    imageUrl = hostUrl + "/Upload/Product/" + productCode + "/" + productCode + ".png";
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
            return Ok(imageUrl);
        }

        [HttpGet("GetMultipleImage")]
        public async Task<IActionResult> GetMultipleImage(string productCode)
        {
            List<string> imageUrl = new List<string>();
            string hostUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            try
            {
                var filePath = GetFilePath(productCode);
                if (System.IO.Directory.Exists(filePath))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(filePath);
                    var files = directoryInfo.GetFiles();
                    if (files != null && files.Count() > 0)
                    {
                        foreach (var file in files)
                        {
                            var imagePath = filePath + "\\" + file.Name;
                            if (System.IO.File.Exists(imagePath))
                            {
                                var imgUrl = hostUrl + "/Upload/Product/" + productCode + "/" + file.Name;
                                imageUrl.Add(imgUrl);
                            }
                        }
                            return Ok(imageUrl);
                    }
                    else
                    {
                        return NotFound();
                    }
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        [HttpGet("DowloadImage")]
        public async Task<IActionResult> DowloadImage(string productCode)
        {
            try
            {
                var filePath = GetFilePath(productCode);
                var imagePath = filePath + "\\" + productCode + ".png";
                if (System.IO.File.Exists(imagePath))
                {
                    MemoryStream memoryStream = new MemoryStream();
                    using(FileStream fileStream = new FileStream(imagePath, FileMode.Open))
                    {
                        await memoryStream.CopyToAsync(fileStream);
                    }
                    memoryStream.Position = 0;
                    return File(memoryStream, "image/png", productCode + ".png");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }

        [HttpGet("RemoveImage")]
        public async Task<IActionResult> RemoveImage(string productCode)
        {
            try
            {
                var filePath = GetFilePath(productCode);
                var imagePath = filePath + "\\" + productCode + ".png";
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                    return Ok("File Deleted Successfully..");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }

        [HttpPost("UploadMultipleImageInDB")]
        public async Task<IActionResult> UploadMultipleImageInDB(IFormFileCollection formFile, string productCode)
        {
            APIResponse response = new APIResponse();
            int passCount = 0; int errorCount = 0;
            try
            {
                foreach (var file in formFile)
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        await file.CopyToAsync(stream);
                        await _context.ProductImages.AddAsync(new Repos.Models.ProductImage()
                        {
                            ProductCode = int.Parse(productCode),
                            ProductImage1 = stream.ToArray()
                        });
                        await _context.SaveChangesAsync();
                        passCount++;
                    }
                }

            }
            catch (Exception ex)
            {
                response.ErrorMessage = ex.Message;
                errorCount++;
            }

            response.ResponseCode = 200;
            response.Result = passCount + " files uploaded & " + errorCount + " files failed";
            return Ok(response);
        }

        [HttpGet("GetMultipleImagesFromDB")]
        public async Task<IActionResult> GetMultipleImagesFromDB(int productCode)
        {
            List<string> imageUrl = new List<string>();
            try
            {
                var checkProductImages = _context.ProductImages.Where(x => x.ProductCode == productCode).ToList();
                if (checkProductImages != null && checkProductImages.Count > 0)
                {
                    checkProductImages.ForEach(item =>
                    {
                        imageUrl.Add("data:image/jpeg;base64,"+Convert.ToBase64String(item.ProductImage1));
                    });
                    return Ok(imageUrl);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }


        [NonAction]
        private string GetFilePath(string productCode)
        {
            return _webHostEnvironment.WebRootPath + "\\Upload\\Product\\" + productCode;
        }
    }
}
