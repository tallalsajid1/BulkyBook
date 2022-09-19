﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BulkyBook.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork : IDisposable
    {
        ICategoryRepository Category { get; }
        ICoverTypeRepository CoverType { get; }
        IProductRepository Product { get; }
        ICompanyRepository Company { get; }
        IApplicationUserRepository ApplicationUser { get; }
        ISP_Call SP_Call { get; }
        IOrderDetailsRepository OrderDetails { get; }
        IOrderHeaderRepository OrderHeader { get; }       
        IShoppingCartRepository ShoppingCart { get; }
        INotesRepository Notes { get; }
        IFileUploadRepository FileUpload { get; }

        void Save();
    }
}
