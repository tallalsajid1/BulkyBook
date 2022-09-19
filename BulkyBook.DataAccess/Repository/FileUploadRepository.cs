using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BulkyBook.DataAccess.Repository
{
    public class FileUploadRepository : Repository<FileUpload>, IFileUploadRepository
    {
        private readonly ApplicationDbContext _db;

        public FileUploadRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(FileUpload fileUpload)
        {
            var objFromDb = _db.FileUploads.FirstOrDefault(s => s.Id == fileUpload.Id);
            if (objFromDb != null)
            {
                objFromDb.Title = fileUpload.Title;                
            }            
        }
    }
}
