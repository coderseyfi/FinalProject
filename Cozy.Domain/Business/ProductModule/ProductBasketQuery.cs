﻿using Cozy.Domain.AppCode.Extensions;
using Cozy.Domain.Models.DataContexts;
using Cozy.Domain.Models.Entites;
using MediatR;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Cozy.Domain.Business.ProductModule
{
    public class ProductBasketQuery : IRequest<List<Basket>>
    {
        public class ProductBasketQueryHandler : IRequestHandler<ProductBasketQuery, List<Basket>>
        {
            private readonly CozyDbContext db;
            private readonly IActionContextAccessor ctx;

            public ProductBasketQueryHandler(CozyDbContext db, IActionContextAccessor ctx)
            {
                this.db = db;
                this.ctx = ctx;
            }
            public async Task<List<Basket>> Handle(ProductBasketQuery request, CancellationToken cancellationToken)
            {
                var userId = ctx.GetCurrentUserId();

                var data = await db.Basket
                    .Include(b => b.Product)
                    .ThenInclude(p => p.ProductImages.Where(i => i.IsMain && i.DeletedByUserId == null))

                    .Where(b => b.UserId == userId)
                    .ToListAsync(cancellationToken);

                return data;
            }
        }
    }
}