using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.UI;
using LbI.Entities;
using LbI.Enums;
using LbI.Inventories.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LbI.Inventories
{
    public class InventoryAppService: LbIAppServiceBase, IInventoryAppService
    {
        private readonly IRepository<Inventory> _inventoryRepo;
        private readonly IRepository<Product> _productRepo;
        private readonly IRepository<StockPoint> _stockPointRepo;
        private readonly IRepository<ProductTransfer> _productTransfersRepo;
        public InventoryAppService(
            IRepository<Inventory> inventoryRepo,
            IRepository<StockPoint> stockPointRepo,
            IRepository<ProductTransfer> productTransfersRepo,
            IRepository<Product> productRepo)
        {
            _inventoryRepo = inventoryRepo;
            _stockPointRepo = stockPointRepo;
            _productTransfersRepo = productTransfersRepo;
            _productRepo = productRepo;

        }

        public async Task<List<InventoryOutputDto>> GetInventoriesAsync()
        {
            var stockPoints = await _stockPointRepo.GetAllListAsync();
            var inventories = await _inventoryRepo.GetAllListAsync();
            var products = await _productRepo.GetAllListAsync();

            var output = new List<InventoryOutputDto>();

            foreach (var sp in stockPoints)
            {
                var inventory = new InventoryOutputDto()
                {
                    StockPointId = sp.Id,
                    StockPointName = sp.Name
                };

                var thisInventories = inventories.Where(x => x.StockPointId == sp.Id).ToList();
                foreach (var product in products)
                {
                    var thisProduct = thisInventories.FirstOrDefault(f => f.ProductId == product.Id);
                    if (thisProduct != null)
                    {
                        if (product.Size == ProductSize.NinePointEightZero && product.Type == ProductType.MedicalOxygen)
                        {
                            inventory.MedicalOxygen9_8Qty = thisProduct.StockQty;
                        }
                        else if (product.Size == ProductSize.OnePointThreeSix && product.Type == ProductType.MedicalOxygen)
                        {
                            inventory.MedicalOxygen1_36Qty = thisProduct.StockQty;
                        }
                        else if (product.Size == ProductSize.NinePointEightZero && product.Type == ProductType.MedicalAir)
                        {
                            inventory.MedicalAir9_8Qty = thisProduct.StockQty;
                        }
                        else if (product.Size == ProductSize.SevenPointZeroZero && product.Type == ProductType.MedicalAir)
                        {
                            inventory.MedicalAir7Qty = thisProduct.StockQty;
                        }
                        else if (product.Size == ProductSize.ThirtyKG && product.Type == ProductType.Nitrous)
                        {
                            inventory.Nitros30KgQty = thisProduct.StockQty;
                        }
                        else if (product.Size == ProductSize.FiveKG && product.Type == ProductType.Nitrous)
                        {
                            inventory.Nitros5KgQty = thisProduct.StockQty;
                        }
                        else if (product.Size == ProductSize.ThreeKG && product.Type == ProductType.Nitrous)
                        {
                            inventory.Nitros3KgQty = thisProduct.StockQty;
                        }
                    }
                }

                inventory.Total = inventory.MedicalOxygen9_8Qty + inventory.MedicalOxygen1_36Qty + inventory.MedicalAir9_8Qty + inventory.MedicalAir7Qty + inventory.Nitros30KgQty + inventory.Nitros5KgQty + inventory.Nitros3KgQty;

                output.Add(inventory);
            }

            return output;
        }

        [UnitOfWork]
        public async Task TransferProductsAsync(ProductTransferEntryDto input)
        {
            var from = await _inventoryRepo.FirstOrDefaultAsync(f=> f.ProductId == input.ProductId && f.StockPointId == input.FromStockPointId);
            if(from == null || from.StockQty < input.TransferQuantity)
            {
                throw new UserFriendlyException("Insufficient transfer quantity found.");
            }
            else
            {
                from.StockQty -= input.TransferQuantity;
                await _inventoryRepo.UpdateAsync(from);
                var to = await _inventoryRepo.FirstOrDefaultAsync(f => f.ProductId == input.ProductId && f.StockPointId == input.ToStockPointId);
                if(to == null)
                {
                    var inventory = new Inventory()
                    {
                        ProductId = input.ProductId,
                        ProductName = input.ProductName,
                        StockPointId = input.ToStockPointId,
                        StockQty = input.TransferQuantity,
                    };
                    await _inventoryRepo.InsertAsync(inventory);
                }
                else
                {
                    to.StockQty += input.TransferQuantity;
                    await _inventoryRepo.UpdateAsync(from);
                }

                var pt = new ProductTransfer()
                {
                    TransferDate = input.TransferDate,
                    ProductId = input.ProductId,
                    FromStockPointId = input.FromStockPointId,
                    ToStockPointId = input.ToStockPointId,
                    TransferQuantity = input.TransferQuantity
                };
                await _productTransfersRepo.InsertAsync(pt);
            }
        }

        [UnitOfWork]
        public async Task ProductTransferRemoveAsync(int id)
        {
            var pt = await _productTransfersRepo.SingleAsync(s => s.Id == id);
            var thisProductInventory = await _inventoryRepo.GetAllListAsync(s => s.ProductId == pt.ProductId && (s.StockPointId == pt.ToStockPointId || s.StockPointId == pt.FromStockPointId));
            var fromStockInventory = thisProductInventory.Single(s => s.StockPointId == pt.FromStockPointId);
            var toStockInventory = thisProductInventory.Single(s => s.StockPointId == pt.ToStockPointId);
                       
            var toStockNewQty = toStockInventory.StockQty - pt.TransferQuantity; 
            if(toStockNewQty < 0 )
            {
                throw new UserFriendlyException("There is not enough quantity to remove this Transfer History");
            }
            var fromStockNewQty = fromStockInventory.StockQty + pt.TransferQuantity;

            fromStockInventory.StockQty = fromStockNewQty;
            toStockInventory.StockQty = toStockNewQty;

            await _inventoryRepo.UpdateAsync(fromStockInventory);
            await _inventoryRepo.UpdateAsync(toStockInventory);

            await _productTransfersRepo.DeleteAsync(x=> x.Id == id);
        }

        public async Task<List<ComboboxItemDto>> GetInventoryProductsAsync()
        {
            return ((await _inventoryRepo.GetAllAsync()).Select(s => new ComboboxItemDto()
            {
                Value = s.ProductId.ToString(),
                DisplayText = s.ProductName
            }).Distinct().ToList());
        }

        public async Task<List<ProductTransferDto>> GetProductTransferHistoriesAsync(int productId, DateTime date)
        {
            var output = (from pt in await _productTransfersRepo.GetAllAsync()
                          join p in await _productRepo.GetAllAsync() on pt.ProductId equals p.Id
                          join fs in await _stockPointRepo.GetAllAsync() on pt.FromStockPointId equals fs.Id
                          join ts in await _stockPointRepo.GetAllAsync() on pt.ToStockPointId equals ts.Id
                          where pt.ProductId == productId && pt.TransferDate.Date == date.Date
                          select new ProductTransferDto()
                          {
                              Id = pt.Id,
                              TransferDate = pt.TransferDate,
                              ProductId = pt.ProductId,
                              ProductName = p.Name,
                              FromStockPointId = pt.FromStockPointId,
                              FromStockPointName = fs.Name,
                              ToStockPointId = pt.ToStockPointId,
                              ToStockPointName = ts.Name,
                              TransferQuantity = pt.TransferQuantity,
                              CreationTime = pt.CreationTime
                          }).OrderBy(o=>o.CreationTime).ToList();
            return output;
        }

        public async Task<ProductTransferEntryDto> GetProductTransferAsync(int id)
        {
            var entity = await _productTransfersRepo.SingleAsync(s=> s.Id == id);
            return new ProductTransferEntryDto()
            {
                Id = entity.Id,
                TransferDate = entity.TransferDate,
                ProductId = entity.Id,
                FromStockPointId = entity.FromStockPointId,
                ToStockPointId = entity.ToStockPointId,
                TransferQuantity = entity.TransferQuantity
            };
        }
    }
}
