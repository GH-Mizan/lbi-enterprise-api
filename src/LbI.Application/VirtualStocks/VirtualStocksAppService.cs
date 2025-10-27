using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.EntityFrameworkCore.Repositories;
using Abp.UI;
using LbI.Entities;
using LbI.Enums;
using LbI.Purchases.Dto;
using LbI.VirtualItems.Dto;
using LbI.VirtualStocks.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LbI.VirtualStocks
{
    public class VirtualStocksAppService: LbIAppServiceBase, IVirtualStocksAppService
    {
        private readonly IRepository<VirtualStock> _virtualStockRepo;
        private readonly IRepository<VirtualStockDetail> _virtualStockDetailRepo;
        private readonly IRepository<VirtualInventory> _virtualInventoryRepo;
        private readonly IRepository<VirtualItem> _virtualItemRepo;
        private readonly IRepository<StockPoint> _stockPointRepo;
        private readonly IRepository<Customer> _customerRepo;
        private readonly IRepository<Supplier> _supplierRepo;
        public VirtualStocksAppService(
            IRepository<VirtualStock> virtualStockRepo, 
            IRepository<VirtualStockDetail> virtualStockDetailRepo,
            IRepository<VirtualInventory> virtualInventoryRepo,
            IRepository<VirtualItem> virtualItemRepo,
            IRepository<StockPoint> stockPointRepo,
            IRepository<Customer> customerRepo,
            IRepository<Supplier> supplierRepo)
        {
            _virtualStockRepo = virtualStockRepo;
            _virtualStockDetailRepo = virtualStockDetailRepo;
            _virtualInventoryRepo = virtualInventoryRepo;
            _virtualItemRepo = virtualItemRepo;
            _stockPointRepo = stockPointRepo;
            _customerRepo = customerRepo;
            _supplierRepo = supplierRepo;
        }

        public async Task<List<VirtualStockOutputDto>> GetVirtualStocksAsync(int warehouseId, DateTime startDate, DateTime endDate, VirtualStockType type)
        {
            try
            {
                var stocks = (from vs in await _virtualStockRepo.GetAllAsync()
                              join sp in await _stockPointRepo.GetAllAsync() on vs.StockPointId equals sp.Id into stockPoints
                              from sp in stockPoints.DefaultIfEmpty()
                              where vs.ClientId == warehouseId && vs.VirtualStockType == type && vs.Date.Date >= startDate.Date && vs.Date.Date <= endDate.Date
                              select new
                              {
                                  vs.Id,
                                  vs.Date,
                                  vs.StockPointId,
                                  StockPoint = sp.Name,
                                  vs.SupervisorId,
                                  vs.DriverId,
                                  vs.VirtualStockType
                              }).OrderBy(o => o.Date).ToList();
                var stockIds = stocks.Select(x => x.Id).ToList();
                var stockDetails = await _virtualStockDetailRepo.GetAllListAsync(x => stockIds.Contains(x.VirtualStockId));
                var items = await _virtualItemRepo.GetAllListAsync(x => x.ActiveStatus);
                var output = new List<VirtualStockOutputDto>();
                foreach (var stock in stocks)
                {
                    var thisStockDetails = stockDetails.Where(x => x.VirtualStockId == stock.Id).ToList();
                    var vs = new VirtualStockOutputDto()
                    {
                        Id = stock.Id,
                        Date = stock.Date,
                        StockPointId = stock.StockPointId,
                        StockPoint = stock.StockPointId != -1 ? stock.StockPoint : "Reconciliation",
                        SupervisorId = stock.SupervisorId,
                        DriverId = stock.DriverId,
                        VirtualStockType = stock.VirtualStockType
                    };
                    foreach (var item in items)
                    {
                        var stockDetail = thisStockDetails.FirstOrDefault(f => f.ProductId == item.Id);
                        if (stockDetail != null)
                        {
                            if (item.Type == ProductType.MedicalAir)
                            {
                                vs.MedicalAirIn = stockDetail.In;
                                vs.MedicalAirOut = stockDetail.Out;
                                vs.MedicalAirStock = stockDetail.StockQty;
                            }
                            else if (item.Type == ProductType.Nitrous)
                            {
                                vs.NitrousIn = stockDetail.In;
                                vs.NitrousOut = stockDetail.Out;
                                vs.NitrousStock = stockDetail.StockQty;
                            }
                            else if (item.Name.Contains("1.36"))
                            {
                                vs.Oxygen136In = stockDetail.In;
                                vs.Oxygen136Out = stockDetail.Out;
                                vs.Oxygen136Stock = stockDetail.StockQty;
                            }
                            else
                            {
                                vs.Oxygen98In = stockDetail.In;
                                vs.Oxygen98Out = stockDetail.Out;
                                vs.Oxygen98Stock = stockDetail.StockQty;
                            }
                        }

                    }
                    output.Add(vs);
                }
                return output.OrderByDescending(o=> o.Id).ToList();
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<bool> CheckVirtualStockExistenceAsync(DateTime date, int warehouseId, VirtualStockType type)
        {
            return (await _virtualStockRepo.GetAllAsync()).Any(x => x.ClientId == warehouseId && x.VirtualStockType == type && x.Date.Date == date.Date);
        }

        //public async Task<VirtualStockEntryInput> GetPreviousVirtualStockAsync(DateTime date, int warehouseId)
        //{
        //    var stock = await _virtualStockRepo.FirstOrDefaultAsync(f => f.ClientId == warehouseId && f.Date.Date == date.Date);
            
        //    if(stock != null)
        //    {
        //        var stockDetails = (from vs in await _virtualStockDetailRepo.GetAllAsync()
        //                            join p in await _virtualItemRepo.GetAllAsync() on vs.ProductId equals p.Id
        //                            where vs.VirtualStockId == stock.Id
        //                            select new VirtualStockDetailEntryDto()
        //                            {
        //                                Id = vs.Id,
        //                                VirtualStockId = vs.VirtualStockId,
        //                                ProductId = vs.ProductId,
        //                                ProductName = p.Name,
        //                                In = vs.In,
        //                                Out = vs.Out,
        //                                StockQty = vs.StockQty,
        //                                InitialStockQty = vs.StockQty - vs.In + vs.Out,
        //                                Selected = vs.In > 0 || vs.Out > 0
        //                            }).ToList();
        //        return new VirtualStockEntryInput()
        //        {
        //            Stock = new VirtualStockEntryDto()
        //            {
        //                Id = stock.Id,
        //                Date = stock.Date,
        //                StockPointId = stock.StockPointId,
        //                ClientId = stock.ClientId,
        //                SupervisorId = stock.SupervisorId,
        //                DriverId = stock.DriverId
        //            },
        //            StockDetails = stockDetails
        //        };
        //    }
        //    else return null;
        //}

        [UnitOfWork]
        public async Task CreateVirtualStockAsync(VirtualStockEntryInput input)
        { 
            var virtualStock = ObjectMapper.Map<VirtualStock>(input.Stock);
            var id = await _virtualStockRepo.InsertAndGetIdAsync(virtualStock);

            var newInventories = new List<VirtualInventory>();
            var productIds = input.StockDetails.Select(s => s.ProductId).ToList();
            var inventories = await _virtualInventoryRepo.GetAllListAsync(x => x.WarehouseId == input.Stock.ClientId && productIds.Contains(x.ProductId) && x.VirtualStockType == virtualStock.VirtualStockType);
            foreach (var sd in input.StockDetails)
            {
                sd.VirtualStockId = id;

                var currentInventory = inventories.FirstOrDefault(f => f.ProductId == sd.ProductId);
                if(currentInventory == null)
                {
                    newInventories.Add(new VirtualInventory()
                    {
                        ProductId = sd.ProductId,
                        WarehouseId = input.Stock.ClientId,
                        StockQty = sd.StockQty,
                        VirtualStockType = virtualStock.VirtualStockType,
                    });
                }
                else
                {
                    currentInventory.StockQty = sd.StockQty;
                    await _virtualInventoryRepo.UpdateAsync(currentInventory);
                }
            }

            await _virtualInventoryRepo.InsertRangeAsync(newInventories);

            var details = ObjectMapper.Map<List<VirtualStockDetail>>(input.StockDetails);
            await _virtualStockDetailRepo.InsertRangeAsync(details);
        }

        //[UnitOfWork]
        //public async Task UpdateVirtualStockAsync(VirtualStockEntryInput input)
        //{
        //    await VirtualStockRemoveAsync(input.Stock, false);

        //    var newInventories = new List<VirtualInventory>();
        //    var productIds = input.StockDetails.Select(s => s.ProductId).ToList();
        //    var inventories = await _virtualInventoryRepo.GetAllListAsync(x => x.WarehouseId == input.Stock.ClientId && productIds.Contains(x.ProductId));

        //    foreach (var sd in input.StockDetails)
        //    {
        //        sd.Id = null;
        //        sd.VirtualStockId = input.Stock.Id.Value;

        //        var currentInventory = inventories.FirstOrDefault(f => f.ProductId == sd.ProductId);
        //        if (currentInventory == null)
        //        {
        //            newInventories.Add(new VirtualInventory()
        //            {
        //                ProductId = sd.ProductId,
        //                WarehouseId = input.Stock.ClientId,
        //                StockQty = sd.StockQty
        //            });
        //        }
        //        else
        //        {
        //            currentInventory.StockQty = sd.StockQty;
        //            await _virtualInventoryRepo.UpdateAsync(currentInventory);
        //        }
        //    }

        //    await _virtualInventoryRepo.InsertRangeAsync(newInventories);
        //    var details = ObjectMapper.Map<List<VirtualStockDetail>>(input.StockDetails);
        //    await _virtualStockDetailRepo.InsertRangeAsync(details);
        //}

        //[UnitOfWork]

        public async Task VirtualStockRemoveAsync(int id, VirtualStockType type)
        {
            var stock = await _virtualStockRepo.SingleAsync(x => x.Id == id);
            var stockDetails = await _virtualStockDetailRepo.GetAllListAsync(x => x.VirtualStockId == id);
            var productIds = stockDetails.Select(s => s.ProductId).ToList();

            var inventories = await _virtualInventoryRepo.GetAllListAsync(x => x.WarehouseId == stock.ClientId && productIds.Contains(x.ProductId) && x.VirtualStockType == stock.VirtualStockType);
            foreach (var sd in stockDetails)
            {
                var inventory = inventories.FirstOrDefault(f => f.ProductId == sd.ProductId);
                if (inventory != null)
                {
                    inventory.StockQty = inventory.StockQty - (sd.In - sd.Out);
                    await _virtualInventoryRepo.UpdateAsync(inventory);
                }
            }
            await _virtualStockDetailRepo.BatchDeleteAsync(x => x.VirtualStockId == id);
            await _virtualStockRepo.DeleteAsync(x => x.Id == id);

        }
        public async Task<List<VirtualInventoryDto>> GetVirtualInventoryInfoAsync(int warehouseId, VirtualStockType type)
        {
            var output = (await _virtualInventoryRepo.GetAllAsync()).Where(x=> x.WarehouseId == warehouseId && x.VirtualStockType == type)
                .Select(s=> new VirtualInventoryDto() { ProductId = s.ProductId, StockQty = s.StockQty}).ToList();

            return output;
        }

        public async Task<GeneralStockOutputDto> GetGeneralStocksReportAsync(DateTime date)
        {
            var clients = await _customerRepo.GetAllListAsync(x => x.ActiveStatus);
            var suppliers = await _supplierRepo.GetAllListAsync(x => x.ActiveStatus);

            var virtualStocks = await _virtualStockRepo.GetAllListAsync(x => x.Date.Date == date.Date);
            var virtualStockIds = virtualStocks.Select(s=>s.Id).ToList();
            var virtualStockDetails = await _virtualStockDetailRepo.GetAllListAsync(x => virtualStockIds.Contains(x.VirtualStockId));
            var products = await _virtualItemRepo.GetAllListAsync(x => x.ActiveStatus);

            var oxygen136Id = products.First(f => f.Type == ProductType.MedicalOxygen && f.Name.Contains("1.36")).Id;
            var oxygen98Id = products.First(f => f.Type == ProductType.MedicalOxygen && f.Name.Contains("9.8")).Id;
            var medicalAirId = products.First(f => f.Type == ProductType.MedicalAir).Id;
            var nitrousId = products.First(f => f.Type == ProductType.Nitrous).Id;

            var output = new GeneralStockOutputDto();
            var outputDetails = new List<GeneralStockDetailsDto>();

            foreach (var client in clients) 
            {
                var virtualStockId = virtualStocks.FirstOrDefault(f=> f.ClientId == client.Id && f.VirtualStockType == VirtualStockType.ClientWarehouse)?.Id;
                var details = virtualStockDetails.Where(x => x.VirtualStockId == virtualStockId).ToList();
                var stock = new GeneralStockDetailsDto()
                {
                    WarehouseId = client.Id,
                    WarehouseName = client.Name,
                    VirtualStockType = VirtualStockType.ClientWarehouse
                };
                if(virtualStockId != null)
                {
                    stock.Oxygen136 = details.FirstOrDefault(f => f.ProductId == oxygen136Id)?.StockQty ?? 0;
                    stock.Oxygen98 = details.FirstOrDefault(f => f.ProductId == oxygen98Id)?.StockQty ?? 0;
                    stock.MedicalAir = details.FirstOrDefault(f => f.ProductId == medicalAirId)?.StockQty ?? 0;
                    stock.NitrousOxide = details.FirstOrDefault(f => f.ProductId == nitrousId)?.StockQty ?? 0;
                    stock.Total = stock.Oxygen136 + stock.Oxygen98 + stock.MedicalAir + stock.NitrousOxide;
                }
                outputDetails.Add(stock);
            }

            foreach (var supplier in suppliers)
            {
                var virtualStockId = virtualStocks.FirstOrDefault(f => f.ClientId == supplier.Id && f.VirtualStockType == VirtualStockType.SupplierWarehouse)?.Id;
                var details = virtualStockDetails.Where(x => x.VirtualStockId == virtualStockId).ToList();
                var stock = new GeneralStockDetailsDto()
                {
                    WarehouseId = supplier.Id,
                    WarehouseName = supplier.Name,
                    VirtualStockType = VirtualStockType.SupplierWarehouse
                };
                if (virtualStockId != null)
                {
                    stock.Oxygen136 = details.FirstOrDefault(f => f.ProductId == oxygen136Id)?.StockQty ?? 0;
                    stock.Oxygen98 = details.FirstOrDefault(f => f.ProductId == oxygen98Id)?.StockQty ?? 0;
                    stock.MedicalAir = details.FirstOrDefault(f => f.ProductId == medicalAirId)?.StockQty ?? 0;
                    stock.NitrousOxide = details.FirstOrDefault(f => f.ProductId == nitrousId)?.StockQty ?? 0;
                    stock.Total = stock.Oxygen136 + stock.Oxygen98 + stock.MedicalAir + stock.NitrousOxide;
                }
                outputDetails.Add(stock);
            }

            output = outputDetails.GroupBy(t => 1).Select(g => new GeneralStockOutputDto()
            {
                Oxygen136Total = g.Sum(s => s.Oxygen136),
                Oxygen98Total = g.Sum(s => s.Oxygen98),
                MedicalAirTotal = g.Sum(s => s.MedicalAir),
                NitrousOxideTotal = g.Sum(s => s.NitrousOxide)
            }).First();
            output.GrandTotal = output.Oxygen136Total + output.Oxygen98Total + output.MedicalAirTotal + output.NitrousOxideTotal;
            output.Details = outputDetails;
            return output;
        }

        public async Task<List<OverallVirtualInventoriesOutput>> GetActualVirtualInventoriesAsync()
        {
            try
            {
                var output = (from i in await _virtualInventoryRepo.GetAllAsync()
                            join p in await _virtualItemRepo.GetAllAsync() on i.ProductId equals p.Id
                            select new OverallVirtualInventoriesOutput()
                            {
                                ProductId = i.ProductId,
                                ProductName = p.Name,
                                WarehouseId = i.WarehouseId,
                                VirtualStockType = i.VirtualStockType,
                                StockQty = i.StockQty
                            }).ToList();
                var customers = await _customerRepo.GetAllListAsync();
                var suppliers = await _supplierRepo.GetAllListAsync();

                foreach (var item in output)
                {
                    if(item.VirtualStockType == VirtualStockType.ClientWarehouse)
                    {
                        item.WarehouseName = customers.First(f => f.Id == item.WarehouseId).Name;
                    }
                    else
                    {
                         item.WarehouseName = suppliers.First(f => f.Id == item.WarehouseId).Name;
                    }
                }

                return output;
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException(ex.Message);
            }
        }
    }
}
