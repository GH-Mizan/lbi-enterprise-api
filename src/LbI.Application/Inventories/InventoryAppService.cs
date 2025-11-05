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
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace LbI.Inventories
{
    public class InventoryAppService: LbIAppServiceBase, IInventoryAppService
    {
        private readonly IRepository<Inventory> _inventoryRepo;
        private readonly IRepository<Product> _productRepo;
        private readonly IRepository<VirtualItem> _virtualItemRepo;
        private readonly IRepository<StockPoint> _stockPointRepo;
        private readonly IRepository<ProductTransfer> _productTransfersRepo;
        private readonly IRepository<VirtualInventory> _virtualInventoryRepo;
        public InventoryAppService(
            IRepository<Inventory> inventoryRepo,
            IRepository<StockPoint> stockPointRepo,
            IRepository<ProductTransfer> productTransfersRepo,
            IRepository<Product> productRepo,
            IRepository<VirtualItem> virtualItemRepo,
            IRepository<VirtualInventory> virtualInventoryRepo)
        {
            _inventoryRepo = inventoryRepo;
            _stockPointRepo = stockPointRepo;
            _productTransfersRepo = productTransfersRepo;
            _productRepo = productRepo;
            _virtualItemRepo = virtualItemRepo;
            _virtualInventoryRepo = virtualInventoryRepo;
        }

        public async Task<List<InventoryOutputDto>> GetInventoriesAsync(string damadged)
        {
            var isDamadged = damadged == "Y";
            
            var stockPoints = await _stockPointRepo.GetAllListAsync();
            var inventories = await _inventoryRepo.GetAllListAsync(x=> damadged == null || damadged == "" || x.Damadged == isDamadged);
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
                    var stockQty = thisInventories.Where(f => f.ProductId == product.Id).Sum(s=> s.StockQty);
                    if (product.Size == ProductSize.NinePointEightZero && product.Type == ProductType.MedicalOxygen)
                    {
                        inventory.MedicalOxygen9_8Qty = stockQty;
                    }
                    else if (product.Size == ProductSize.OnePointThreeSix && product.Type == ProductType.MedicalOxygen)
                    {
                        inventory.MedicalOxygen1_36Qty = stockQty;
                    }
                    else if (product.Size == ProductSize.NinePointEightZero && product.Type == ProductType.MedicalAir)
                    {
                        inventory.MedicalAir9_8Qty = stockQty;
                    }
                    else if (product.Size == ProductSize.SevenPointZeroZero && product.Type == ProductType.MedicalAir)
                    {
                        inventory.MedicalAir7Qty = stockQty;
                    }
                    else if (product.Size == ProductSize.ThirtyKG && product.Type == ProductType.Nitrous)
                    {
                        inventory.Nitros30KgQty = stockQty;
                    }
                    else if (product.Size == ProductSize.FiveKG && product.Type == ProductType.Nitrous)
                    {
                        inventory.Nitros5KgQty = stockQty;
                    }
                    else if (product.Size == ProductSize.ThreeKG && product.Type == ProductType.Nitrous)
                    {
                        inventory.Nitros3KgQty = stockQty;
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
            var productIds = (await _inventoryRepo.GetAllAsync()).Select(s => s.ProductId).Distinct().ToList();
            return (await _productRepo.GetAllAsync()).Where(x => productIds.Contains(x.Id)).Select(s => new ComboboxItemDto()
            {
                Value =s.Id.ToString(),
                DisplayText = s.Name
            }).ToList();
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

        [UnitOfWork]
        public async Task MarkAsDamadgedAsync(MarkAsDamadgedInput input)
        {
            var currentInventory = await _inventoryRepo.SingleAsync(x=> x.StockPointId == input.StockPointId && x.ProductId == input.ProductId && !x.Damadged);
            currentInventory.StockQty -= input.DamadgeQty;
            await _inventoryRepo.UpdateAsync(currentInventory);

            var currentDamadgedInventory = await _inventoryRepo.FirstOrDefaultAsync(x => x.StockPointId == input.StockPointId && x.ProductId == input.ProductId && x.Damadged);
            if (currentDamadgedInventory != null) {
                currentDamadgedInventory.StockQty += input.DamadgeQty;
                await _inventoryRepo.UpdateAsync(currentDamadgedInventory);
            }
            else
            {
                var damadgedInventory = new Inventory()
                {
                    ProductId = input.ProductId,
                    StockPointId = input.StockPointId,
                    StockQty = input.DamadgeQty,
                    Damadged = true
                };
                await _inventoryRepo.InsertAsync(damadgedInventory);
            }
                
        }

        [UnitOfWork]
        public async Task EditDamadgeAsync(MarkAsDamadgedInput input)
        {
            var actualInventory = await _inventoryRepo.SingleAsync(x => x.StockPointId == input.StockPointId && x.ProductId == input.ProductId && !x.Damadged);

            var existingDamadgedInventory = await _inventoryRepo.FirstOrDefaultAsync(x => x.StockPointId == input.StockPointId && x.ProductId == input.ProductId && x.Damadged);
            if (existingDamadgedInventory == null) {
                throw new UserFriendlyException("There is no existing damadged inventory");
            }

            var damadgedQty = existingDamadgedInventory.StockQty - input.DamadgeQty;
            actualInventory.StockQty += damadgedQty;
            await _inventoryRepo.UpdateAsync(actualInventory);

            existingDamadgedInventory.StockQty = input.DamadgeQty;
            await _inventoryRepo.UpdateAsync(existingDamadgedInventory);
        }

        public async Task<int> GetProductStockQtyAsync(int stockPointId, int productId, bool damadged)
        {
            return (await _inventoryRepo.FirstOrDefaultAsync(f => f.StockPointId == stockPointId && f.ProductId == productId && f.Damadged == damadged))?.StockQty ?? 0;
        }

        public async Task<List<InventoryCrossCheckDto>> GetInventoriesDifferenceAsync()
        {
            var v_products = await _virtualItemRepo.GetAllListAsync(x => x.ActiveStatus);

            var v_oxygen136Id = v_products.First(f => f.Type == ProductType.MedicalOxygen && f.Name.Contains("1.36")).Id;
            var v_oxygen98Id = v_products.First(f => f.Type == ProductType.MedicalOxygen && f.Name.Contains("9.8")).Id;
            var v_medicalAirId = v_products.First(f => f.Type == ProductType.MedicalAir).Id;
            var v_nitrousId = v_products.First(f => f.Type == ProductType.Nitrous).Id;

            var products = await _productRepo.GetAllListAsync(x => x.ActiveStatus);
            var oxygen136Id = products.First(f => f.Size == ProductSize.OnePointThreeSix && f.Type == ProductType.MedicalOxygen).Id;
            var oxygen98Id = products.First(f => f.Size == ProductSize.NinePointEightZero && f.Type == ProductType.MedicalOxygen).Id;
            var medicalAir98Id = products.First(f => f.Size == ProductSize.NinePointEightZero && f.Type == ProductType.MedicalAir).Id;
            var medicalAir7Id = products.First(f => f.Size == ProductSize.SevenPointZeroZero && f.Type == ProductType.MedicalAir).Id;
            var nitros30KId = products.First(f => f.Size == ProductSize.ThirtyKG && f.Type == ProductType.Nitrous).Id;
            var nitros5KId = products.First(f => f.Size == ProductSize.FiveKG && f.Type == ProductType.Nitrous).Id;
            var nitros3KId = products.First(f => f.Size == ProductSize.ThreeKG && f.Type == ProductType.Nitrous).Id;

            var inventories = await _inventoryRepo.GetAllListAsync();
            var v_inventories = await _virtualInventoryRepo.GetAllListAsync();

            var v_oxygen136Stock = v_inventories.Where(x => x.ProductId == v_oxygen136Id).Sum(s => s.StockQty);
            var v_oxygen98Stock = v_inventories.Where(x => x.ProductId == v_oxygen98Id).Sum(s => s.StockQty);
            var v_oxygenMedicalAirStock = v_inventories.Where(x => x.ProductId == v_medicalAirId).Sum(s => s.StockQty);
            var v_nitrousStock = v_inventories.Where(x => x.ProductId == v_nitrousId).Sum(s => s.StockQty);

            var oxygen136Stock = inventories.Where(x => x.ProductId == oxygen136Id).Sum(s => s.StockQty);
            var oxygen98Stock = inventories.Where(x => x.ProductId == oxygen98Id).Sum(s => s.StockQty);
            var medicalAir98Stock = inventories.Where(x => x.ProductId == medicalAir98Id).Sum(s => s.StockQty);
            var medicalAir7Stock = inventories.Where(x => x.ProductId == medicalAir7Id).Sum(s => s.StockQty);
            var nitros30KStock = inventories.Where(x => x.ProductId == nitros30KId).Sum(s => s.StockQty);
            var nitros5KStock = inventories.Where(x => x.ProductId == nitros5KId).Sum(s => s.StockQty);
            var nitros3KStock = inventories.Where(x => x.ProductId == nitros3KId).Sum(s => s.StockQty);

            var output = new List<InventoryCrossCheckDto>()
            {
                new InventoryCrossCheckDto()
                {
                    ProductName = "Medical Oxygen 1.36",
                    ActualStockQty = oxygen136Stock,
                    VirtualStockQty = v_oxygen136Stock
                },
                new InventoryCrossCheckDto()
                {
                    ProductName = "Medical Oxygen 9.80",
                    ActualStockQty = oxygen98Stock,
                    VirtualStockQty = v_oxygen98Stock
                },
                new InventoryCrossCheckDto()
                {
                    ProductName = "Medical Air",
                    ActualStockQty = medicalAir98Stock + medicalAir7Stock,
                    VirtualStockQty = v_oxygenMedicalAirStock
                },
                new InventoryCrossCheckDto()
                {
                    ProductName = "Nitrous",
                    ActualStockQty = nitros30KStock + nitros5KStock + nitros3KStock,
                    VirtualStockQty = v_nitrousStock
                }
            };

            foreach (var item in output)
            {
                item.Difference = item.ActualStockQty - item.VirtualStockQty;
                item.HasDifference = item.Difference != 0;
            }

            return output;
        }
    }
}
