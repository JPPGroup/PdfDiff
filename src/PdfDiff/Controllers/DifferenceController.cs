using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PdfDiff.Model;

namespace PdfDiff.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DifferenceController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Post(List<IFormFile> files)
        {
            if (files.Count != 2)
                return BadRequest("Invalid number of files");

            string file1 = Path.GetTempFileName();
            string file2 = Path.GetTempFileName();

            using(FileStream fs1 = System.IO.File.Create(file1))
            {
                await files[0].CopyToAsync(fs1);
            }

            using(FileStream fs2 = System.IO.File.Create(file2))
            {
                await files[1].CopyToAsync(fs2);
            }
            
            DifferenceModel model = new DifferenceModel(file1, file2);
            Stream bmpData = model.Compare();

            return File(bmpData, "application/octet-stream");
        }
    }
}