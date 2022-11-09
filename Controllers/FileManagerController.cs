using Microsoft.AspNetCore.Mvc;
using Syncfusion.EJ2.DocumentEditor;
using Newtonsoft.Json;
using Syncfusion.EJ2.FileManager.PhysicalFileProvider;
using Syncfusion.EJ2.FileManager.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http.Features;
using Syncfusion.EJ2.Spreadsheet;
//using Syncfusion.XlsIO;
using Newtonsoft.Json.Linq;
using System;

namespace ASPNETCore_FileManager_PDF_Word_Excel.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableCors("AllowAllOrigins")]
    public class FileManagerController : ControllerBase
    {
        public PhysicalFileProvider operation;
        public string basePath;
        string root = "wwwroot\\Files";
        public FileManagerController(Microsoft.AspNetCore.Hosting.IHostingEnvironment hostingEnvironment)
        {
            this.basePath = hostingEnvironment.ContentRootPath;
            this.operation = new PhysicalFileProvider();
            this.operation.RootFolder(this.basePath + "\\" + this.root);
        }

        [HttpPost]
        [Route("FileOperations")]
        public object? FileOperations([FromBody] JObject data)
        {
            try
            {
                FileManagerDirectoryContent args = JsonConvert.DeserializeObject<FileManagerDirectoryContent>(data.ToString());

                if (args.Action == "delete" || args.Action == "rename")
                {
                    if ((args.TargetPath == null) && (args.Path == ""))
                    {
                        FileManagerResponse response = new FileManagerResponse();
                        response.Error = new ErrorDetails { Code = "401", Message = "Restricted to modify the root folder." };
                        return this.operation.ToCamelCase(response);
                    }
                }
                switch (args.Action)
                {
                    case "read":
                        // reads the file(s) or folder(s) from the given path.
                        return this.operation.ToCamelCase(this.operation.GetFiles(args.Path, args.ShowHiddenItems));
                    case "delete":
                        // deletes the selected file(s) or folder(s) from the given path.
                        return this.operation.ToCamelCase(this.operation.Delete(args.Path, args.Names));
                    case "copy":
                        // copies the selected file(s) or folder(s) from a path and then pastes them into a given target path.
                        return this.operation.ToCamelCase(this.operation.Copy(args.Path, args.TargetPath, args.Names, args.RenameFiles, args.TargetData));
                    case "move":
                        // cuts the selected file(s) or folder(s) from a path and then pastes them into a given target path.
                        return this.operation.ToCamelCase(this.operation.Move(args.Path, args.TargetPath, args.Names, args.RenameFiles, args.TargetData));
                    case "details":
                        // gets the details of the selected file(s) or folder(s).
                        return this.operation.ToCamelCase(this.operation.Details(args.Path, args.Names, args.Data));
                    case "create":
                        // creates a new folder in a given path.
                        return this.operation.ToCamelCase(this.operation.Create(args.Path, args.Name));
                    case "search":
                        // gets the list of file(s) or folder(s) from a given path based on the searched key string.
                        return this.operation.ToCamelCase(this.operation.Search(args.Path, args.SearchString, args.ShowHiddenItems, args.CaseSensitive));
                    case "rename":
                        // renames a file or folder.
                        return this.operation.ToCamelCase(this.operation.Rename(args.Path, args.Name, args.NewName));
                }
                return null;
            }
            catch (Exception ex)
            {
                return Ok("Failed. " + ex.Message);
            }
        }

        [HttpPost]
        [Route("Upload")]
        public IActionResult Upload(string path, IList<IFormFile> uploadFiles, string action)
        {
            FileManagerResponse uploadResponse;
            foreach (var file in uploadFiles)
            {
                var folders = (file.FileName).Split('/');
                // checking the folder upload
                if (folders.Length > 1)
                {
                    for (var i = 0; i < folders.Length - 1; i++)
                    {
                        string newDirectoryPath = Path.Combine(this.basePath + path, folders[i]);
                        if (!Directory.Exists(newDirectoryPath))
                        {
                            this.operation.ToCamelCase(this.operation.Create(path, folders[i]));
                        }
                        path += folders[i] + "/";
                    }
                }
            }
            uploadResponse = operation.Upload(path, uploadFiles, action, null);
            if (uploadResponse.Error != null)
            {
                Response.Clear();
                Response.ContentType = "application/json; charset=utf-8";
                Response.StatusCode = Convert.ToInt32(uploadResponse.Error.Code);
                Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = uploadResponse.Error.Message;
            }
            return Content("");
        }

        [HttpPost]
        [Route("Download")]
        public IActionResult Download(string downloadInput)
        {
            FileManagerDirectoryContent args = JsonConvert.DeserializeObject<FileManagerDirectoryContent>(downloadInput);
            return operation.Download(args.Path, args.Names, args.Data);
        }

        [HttpPost]
        [Route("GetImage")]
        public IActionResult GetImage(FileManagerDirectoryContent args)
        {
            return this.operation.GetImage(args.Path, args.Id, false, null, null);
        }

        //    [HttpGet]
        //    [Route("GetDocument")]
        //    public string GetDocument([FromBody] CustomParams param)
        //    {
        //        string path = this.basePath + "\\wwwroot\\Files" + (param.FileName).Replace("/", "\\");
        //        if (param.Action == "LoadPDF")
        //        {
        //            //for PDF Files
        //            var docBytes = System.IO.File.ReadAllBytes(path);
        //            //we can convert the document stream to bytes then convert to base64
        //            string docBase64 = "data:application/pdf;base64," + Convert.ToBase64String(docBytes);
        //            return (docBase64);
        //        }
        //        else
        //        {
        //            //for Doc Files
        //            try
        //            {
        //                Stream stream = System.IO.File.Open(path, FileMode.Open, FileAccess.ReadWrite);
        //                int index = param.FileName.LastIndexOf('.');
        //                string type = index > -1 && index < param.FileName.Length - 1 ?
        //                    param.FileName.Substring(index) : ".docx";
        //                WordDocument document = WordDocument.Load(stream, GetFormatType(type.ToLower()));
        //                string json = JsonConvert.SerializeObject(document);
        //                document.Dispose();
        //                stream.Dispose();
        //                return json;
        //            }
        //            catch
        //            {
        //                return "Failure";
        //            }
        //        }
        //    }


        //    internal static FormatType GetFormatType(string format)
        //    {
        //        if (string.IsNullOrEmpty(format))
        //        {
        //            throw new NotSupportedException("EJ2 DocumentEditor does not support this file format.");
        //        }

        //        switch (format.ToLower())
        //        {
        //            case ".dotx":
        //            case ".docx":
        //            case ".docm":
        //            case ".dotm":
        //                return FormatType.Docx;
        //            case ".dot":
        //            case ".doc":
        //                return FormatType.Doc;
        //            case ".rtf":
        //                return FormatType.Rtf;
        //            case ".txt":
        //                return FormatType.Txt;
        //            case ".xml":
        //                return FormatType.WordML;
        //            case ".html":
        //                return FormatType.Html;
        //            default:
        //                throw new NotSupportedException("EJ2 DocumentEditor does not support this file format.");
        //        }
        //    }

        //    //For Excel Files
        //    [HttpGet]
        //    [Route("GetExcel")]
        //    public IActionResult GetExcel(CustomParams param)
        //    {
        //        string fullPath = this.basePath + "\\wwwroot\\Files" + (param.FileName).Replace("/", "\\");
        //        FileStream fileStreamInput = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
        //        FileStreamResult fileStreamResult = new FileStreamResult(fileStreamInput, "APPLICATION/octet-stream");
        //        return fileStreamResult;
        //    }

        //    [HttpGet]
        //    [Route("OpenExcel")]
        //    public IActionResult Open(IFormCollection openRequest)
        //    {
        //        ExcelEngine excelEngine = new ExcelEngine();
        //        Stream memStream = (openRequest.Files[0] as IFormFile).OpenReadStream();
        //        IFormFile formFile = new FormFile(memStream, 0, memStream.Length, "", openRequest.Files[0].FileName); // converting MemoryStream to IFormFile
        //        OpenRequest open = new OpenRequest();
        //        open.File = formFile;
        //        return Content(Workbook.Open(open));
        //    }

        //    [HttpGet]
        //    [Route("SaveExcel")]
        //    public IActionResult Save(SaveSettings saveSettings)
        //    {
        //        return Workbook.Save(saveSettings);
        //    }
        //}

        public class CustomParams
        {
            public string FileName { get; set; }
            public string Action { get; set; }
        }
    }
}
