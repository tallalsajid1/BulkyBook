using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Stripe;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BulkyBook.Areas.Admin.Controllers
{
    [Area("Admin")]    
    public class FileUploadController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;

        public FileUploadController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Upsert(int? id)
        {
            FileUpload fileUpload = new FileUpload();
            if (id == null)
            {
                //this is for create
                return View(fileUpload);
            }
            //this is for edit
            fileUpload = _unitOfWork.FileUpload.Get(id.GetValueOrDefault());
            if (fileUpload == null)
            {
                return NotFound();
            }
            return View(fileUpload);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestFormLimits(MultipartBodyLengthLimit = 314572800)]
        public IActionResult Upsert(FileUpload fileUpload)
        {
            if (ModelState.IsValid)
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);
                fileUpload.ApplicationUserId = claim.Value;

                string webRootPath = _hostEnvironment.WebRootPath;
                var files = HttpContext.Request.Form.Files;
                if (files.Count > 0)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(webRootPath, @"images\fileUpload");
                    var extenstion = Path.GetExtension(files[0].FileName);

                    if (fileUpload.ImageUrl != null)
                    {
                        //this is an edit and we need to remove old image
                        var imagePath = Path.Combine(webRootPath, fileUpload.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }
                    using (var filesStreams = new FileStream(Path.Combine(uploads, fileName + extenstion), FileMode.Create))
                    {
                        files[0].CopyTo(filesStreams);
                    }
                    fileUpload.ImageUrl = @"\images\fileUpload\" + fileName + extenstion;
                }
                else
                {
                    //update when they do not change the image
                    if (fileUpload.Id != 0)
                    {
                        FileUpload objFromDb = _unitOfWork.FileUpload.Get(fileUpload.Id);
                        fileUpload.ImageUrl = objFromDb.ImageUrl;
                    }
                }


                if (fileUpload.Id == 0)
                {
                    _unitOfWork.FileUpload.Add(fileUpload);
                    //TempData["AlertMessage"] = "File Uploaded Successfully";

                }
                else
                {
                    _unitOfWork.FileUpload.Update(fileUpload);
                    //TempData["AlertMessage"] = "File Updated Successfully";
                }
                _unitOfWork.Save();
                TempData["AlertMessage"] = "File Uploaded Successfully";
                return RedirectToAction(nameof(Index));
                
            }
            
            return View(fileUpload);
        }


        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var allObj = _unitOfWork.FileUpload.GetAll(c => c.ApplicationUserId == claim.Value).ToList();
            return Json(new { data = allObj });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var objFromDb = _unitOfWork.FileUpload.Get(id);
            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            string webRootPath = _hostEnvironment.WebRootPath;
            var imagePath = Path.Combine(webRootPath, objFromDb.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }
            _unitOfWork.FileUpload.Remove(objFromDb);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete Successful" });

        }

        #endregion
    }
}