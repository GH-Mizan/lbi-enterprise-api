using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.UI;
using LbI.Entities;
using LbI.Inventories.Dto;
using System.Collections;
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

        public async Task<List<StockQuantityOutputDto>> GetInventoriesAsync()
        {
            var output = (from i in (await _inventoryRepo.GetAllAsync()).GroupBy(t => t.StockPointId).Select(g => new
                    {
                        StockPointId = g.Key,
                        Stock = g.Sum(s => s.StockQty)
                    })
                     join sp in await _stockPointRepo.GetAllAsync() on i.StockPointId equals sp.Id
                     select new StockQuantityOutputDto()
                     {
                         StockPointId = i.StockPointId,
                         StockPointName = sp.Name,
                         Stock = i.Stock
                     }).ToList();
            return output;
        }

        public async Task<List<StockWiseInventoryOutputDto>> GetInventoriesBreakpointAsync(int stockPointId)
        {
            return (from i in await _inventoryRepo.GetAllAsync()
                    join p in await _productRepo.GetAllAsync() on i.ProductId equals p.Id
                    where i.StockPointId == stockPointId
                    select new StockWiseInventoryOutputDto()
                    {
                        ProductName = p.Name,
                        Stock = i.StockQty
                    }).ToList();
        }

        [UnitOfWork]
        public async Task TransferProductsAsync(ProductTransferDto input)
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
                    ProductId = input.ProductId,
                    FromStockPointId = input.FromStockPointId,
                    ToStockPointId = input.ToStockPointId,
                    TransferQuantity = input.TransferQuantity
                };
                await _productTransfersRepo.InsertAsync(pt);
            }
        }

        public async Task<List<ComboboxItemDto>> GetInventoryProductsAsync()
        {
            return ((await _inventoryRepo.GetAllAsync()).Select(s => new ComboboxItemDto()
            {
                Value = s.ProductId.ToString(),
                DisplayText = s.ProductName
            }).Distinct().ToList());
        }

        public async Task<List<ProductTransferDto>> GetProductTransferHistoriesAsync(int productId)
        {
            var output = (from pt in await _productTransfersRepo.GetAllAsync()
                          join p in await _productRepo.GetAllAsync() on pt.ProductId equals p.Id
                          join fs in await _stockPointRepo.GetAllAsync() on pt.FromStockPointId equals fs.Id
                          join ts in await _stockPointRepo.GetAllAsync() on pt.ToStockPointId equals ts.Id
                          select new ProductTransferDto()
                          {
                              Id = pt.Id,
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
    }
}
