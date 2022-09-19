using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BulkyBook.DataAccess.Repository
{
    public class NotesRepository : Repository<Notes>, INotesRepository
    {
        private readonly ApplicationDbContext _db;

        public NotesRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Notes notes)
        {
            var objFromDb = _db.notes.FirstOrDefault(s => s.Id == notes.Id);
            if (objFromDb != null)
            {
                objFromDb.UserNotes = notes.UserNotes;                
            }            
        }
    }
}
