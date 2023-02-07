﻿using Cozy.Domain.AppCode.Extensions;
using Cozy.Domain.Business.BasketModule;
using Cozy.Domain.Business.ProductModule;
using Cozy.Domain.Models.DataContexts;
using Cozy.Domain.Models.Entites;
using Cozy.Domain.Models.FormModels;
using Cozy.Domain.Models.ViewModels.ProductViewModel;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cozy.WebUI.Controllers
{

    
    public class ShopController : Controller
    {
        private readonly CozyDbContext db;
        private readonly IMediator mediator;

        public ShopController(CozyDbContext db, IMediator mediator)
        {
            this.db = db;
            this.mediator = mediator;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var brands = await db.Brands.Where(b => b.DeletedDate == null).ToListAsync();

            var categories = await db.Categories
                .Include(c => c.Children)
                .ThenInclude(c => c.Children)
                .Where(b => b.DeletedDate == null && b.ParentId == null)
                .ToListAsync();

            var colors = await db.Colors.Where(c => c.DeletedDate == null).ToListAsync();
            var materials = await db.Materials.Where(c => c.DeletedDate == null).ToListAsync();

            var products = await db.Products
                .Include(p=>p.ProductImages.Where(i=>i.IsMain == true))
                .Include(c => c.Brand)
                .Where(b => b.DeletedDate == null)
                .ToListAsync();


            var vm = new ProductViewModel()
            {
                Brands = brands,
                Categories = categories,
                Colors = colors,
                Materials= materials,
                Products = products
            };

            return View(vm);
        }



        [HttpPost]
        [AllowAnonymous]
        public IActionResult Filter(ShopFilterFormModel model)
        {
            var query = db.Products
                .Include(p => p.ProductImages.Where(i => i.IsMain == true))
                //.Include(c=>c.Category)
                .Include(p => p.Brand)
                .Where(p => p.DeletedDate == null)
                .AsQueryable();

            if (model?.Brands?.Count() > 0)
            {
                query = query.Where(p => model.Brands.Contains(p.BrandId));
            }

            if (model?.Colors?.Count() > 0)
            {
                query = query.Where(p => model.Colors.Contains(p.BrandId));
            }


            return PartialView("_ProductsContainer", query.ToList());
        }


        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            
           var product = await db.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id && p.DeletedDate == null);


            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }


        [AllowAnonymous]
        [Route("/wishlist")]
        public async Task<IActionResult> Wishlist(WishlistQuery query)
        {
            var favs = await mediator.Send(query);

            if (Request.IsAjaxRequest())
            {
                return PartialView("_WishlistBody", favs);
            }

            return View(favs);
        }



        #region BasketRegion



        [Route("/basket")]
        public async Task<IActionResult> Basket(ProductBasketQuery query)
        {
            var response = await mediator.Send(query);

            return View(response);
        }




        [HttpPost]
        [Route("/basket")]
        public async Task<IActionResult> Basket(AddToBasketCommand command)
        {
            var response = await mediator.Send(command);

            return Json(response);
        }



        [HttpPost]
        public async Task<IActionResult> RemoveFromBasket(RemoveFromBasketCommand command)
        {
            var response = await mediator.Send(command);

            return Json(response);
        }



        [HttpPost]
        public async Task<IActionResult> ChangeBasketQuantity(ChangeBasketQuantityCommand command)
        {
            var response = await mediator.Send(command);

            return Json(response);
        }

        #endregion


        [Route("/checkout")]
        public IActionResult Checkout()
        {
            return View();
        }


    }
}
