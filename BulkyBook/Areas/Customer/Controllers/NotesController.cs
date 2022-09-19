using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BulkyBook.Areas.Admin.Controllers
{
    [Area("Customer")]
    public class NotesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public NotesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            
            if (claim != null)
            {
                var user = _unitOfWork.Notes
                    .GetAll(c => c.ApplicationUserId == claim.Value)
                    .ToList();
            }

            return View();
        }
        public IActionResult Upsert(int? id)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            Notes notes = new Notes();
            if (id == null)
            {
                //this is for create
                return View(notes);
            }
            //this is for edit
            notes = _unitOfWork.Notes.Get(id.GetValueOrDefault());
            if (notes == null)
            {
                return NotFound();
            }
            return View(notes);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Upsert(Notes notes)
        {
            //notes.Id = 0;
            if (ModelState.IsValid)
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);
                notes.ApplicationUserId = claim.Value;
                if (notes.Id == 0)
                {
                    _unitOfWork.Notes.Add(notes);

                }
                else
                {
                    _unitOfWork.Notes.Update(notes);
                }
                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            return View(notes);
        }


        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var allObj = _unitOfWork.Notes.GetAll(c => c.ApplicationUserId == claim.Value).ToList();
            return Json(new { data = allObj });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var objFromDb = _unitOfWork.Notes.Get(id);
            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            _unitOfWork.Notes.Remove(objFromDb);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete Successful" });

        }

        #endregion
    }
}