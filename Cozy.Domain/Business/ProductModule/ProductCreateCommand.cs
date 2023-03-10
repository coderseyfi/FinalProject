using Cozy.Domain.Models.DataContexts;
using Cozy.Domain.Models.Entites;
using Cozy.Domain.AppCode.Extensions;
using MediatR;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System;
using System.ComponentModel.DataAnnotations;

namespace Cozy.Domain.Business.ProductModule
{
    public class ProductCreateCommand : IRequest<Product>
    {

        [Required]
        public string Name { get; set; }

        [Required]

        public string StockKeepingUnit { get; set; }

        [Required]

        public decimal Price { get; set; }

        [Required]

        public string ShortDescription { get; set; }

        [Required]

        public string Description { get; set; }


        public int BrandId { get; set; }


        public int CategoryId { get; set; }

        //public int[] ItemIds { get; set; }
        [Required]

        public int ColorId { get; set; }
        [Required]

        public int MaterialId { get; set; }
        [Required]
        public ImageItem[] Images { get; set; }


        public class ProductCreateCommandHandler : IRequestHandler<ProductCreateCommand, Product>
        {
            private readonly CozyDbContext db;
            private readonly IHostEnvironment env;

            public ProductCreateCommandHandler(CozyDbContext db, IHostEnvironment env)
            {
                this.db = db;
                this.env = env;
            }

            public async Task<Product> Handle(ProductCreateCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var model = new Product();
                    model.ProductCatalog = new List<ProductCatalogItem>();
                    model.Name = request.Name;
                    model.StockKeepingUnit = request.StockKeepingUnit;
                    model.Price = request.Price;
                    model.ShortDescription = request.ShortDescription;
                    model.Description = request.Description;
                    model.BrandId = request.BrandId;
                    model.CategoryId = request.CategoryId;

                    if (request.Images != null && request.Images.Where(i => i.File != null).Count() > 0)
                    {
                        model.ProductImages = new List<ProductImage>();


                        foreach (var item in request.Images.Where(i => i.File != null))
                        {
                            var image = new ProductImage();
                            image.IsMain = item.IsMain;

                            string extension = Path.GetExtension(item.File.FileName);//.jpg

                            image.Name = $"product-{Guid.NewGuid().ToString().ToLower()}{extension}";

                            string fullName = env.GetImagePhysicalPath(image.Name);

                            using (var fs = new FileStream(fullName, FileMode.Create, FileAccess.Write))
                            {
                                await item.File.CopyToAsync(fs, cancellationToken);
                            }

                            model.ProductImages.Add(image);
                        }
                    }

                   

                    var itemIn = new ProductCatalogItem();
                    itemIn.ColorId = request.ColorId;
                    itemIn.MaterialId = request.MaterialId;

                    model.ProductCatalog.Add(itemIn);

                    await db.Products.AddAsync(model, cancellationToken);
                    await db.SaveChangesAsync(cancellationToken);


                    return model;

                }
                catch (System.Exception)
                {
                    return null;
                }

            }
        }
    }
}
