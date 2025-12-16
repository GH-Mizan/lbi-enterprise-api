using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.EntityFrameworkCore.Repositories;
using Abp.UI;
using LbI.Entities;
using LbI.Enums;
using LbI.Helpers;
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
                        var sds = thisStockDetails.Where(f => f.ProductId == item.Id).ToList();
                        var inQty = sds.Sum(s => s.In);
                        var outQty = sds.Sum(s => s.Out);
                        var stockQty = sds.Sum(s => s.StockQty);
                        if (item.Type == ProductType.MedicalAir)
                        {
                            vs.MedicalAirIn = inQty;
                            vs.MedicalAirOut = outQty;
                            vs.MedicalAirStock = stockQty;
                        }
                        else if (item.Type == ProductType.Nitrous)
                        {
                            vs.NitrousIn = inQty;
                            vs.NitrousOut = outQty;
                            vs.NitrousStock = stockQty;
                        }
                        else if (item.Name.Contains("1.36"))
                        {
                            vs.Oxygen136In = inQty;
                            vs.Oxygen136Out = outQty;
                            vs.Oxygen136Stock = stockQty;
                        }
                        else
                        {
                            vs.Oxygen98In = inQty;
                            vs.Oxygen98Out = outQty;
                            vs.Oxygen98Stock = stockQty;
                        }
                    }
                    output.Add(vs);
                }
                return output.OrderBy(o=> o.Date).ToList();
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
        public async Task<VirtualInventoryInfoDto> GetVirtualInventoryInfoAsync(int warehouseId, VirtualStockType type)
        {
            return new VirtualInventoryInfoDto()
            {
                LastDate  = (await _virtualStockRepo.GetAllAsync()).Where(x=> x.ClientId == warehouseId && x.VirtualStockType == type).OrderByDescending(o => o.Date).FirstOrDefault()?.Date,
                Inventories = (await _virtualInventoryRepo.GetAllAsync()).Where(x => x.WarehouseId == warehouseId && x.VirtualStockType == type)
                .Select(s => new VirtualInventoryDto() { ProductId = s.ProductId, StockQty = s.StockQty }).ToList()
            };
        }

        public async Task<GeneralStockOutputDto> GetGeneralStocksReportAsync(DateTime date, VirtualStockType? type)
        {
            var clients = type == null || type ==VirtualStockType.ClientWarehouse ? await _customerRepo.GetAllListAsync(x => x.ActiveStatus) : new List<Customer>();
            var suppliers = type == null || type == VirtualStockType.SupplierWarehouse ? await _supplierRepo.GetAllListAsync(x => x.ActiveStatus) : new List<Supplier>();
            var stockPointWarehouses = type == null || type == VirtualStockType.StockPointWarehouse ? await _stockPointRepo.GetAllListAsync(x => x.ActiveStatus && x.StockPointType == StockPointType.Warehouse) : new List<StockPoint>();

            var virtualStocks = new List<VirtualStock>();
            virtualStocks = await _virtualStockRepo.GetAllListAsync(x => x.Date.Date == date.Date && (type == null || x.VirtualStockType == type));
            //if (!virtualStocks.Any())
            //{
            //    var lastDate = (await _virtualStockRepo.GetAllAsync()).Where(x=> x.Date.Date < date.Date && (type == null || x.VirtualStockType == type)).OrderByDescending(x => x.Date).FirstOrDefault()?.Date;
            //    if(lastDate != null)
            //    {
            //        virtualStocks = await _virtualStockRepo.GetAllListAsync(x => x.Date.Date == lastDate.Value.Date && (type == null || x.VirtualStockType == type));
            //    }
            //}

            var virtualStockIds = virtualStocks.Select(s=>s.Id).ToList();
            var virtualStockDetails = await _virtualStockDetailRepo.GetAllListAsync(x => virtualStockIds.Contains(x.VirtualStockId));
            var products = await _virtualItemRepo.GetAllListAsync(x => x.ActiveStatus);

            var oxygen136Id = products.First(f => f.Type == ProductType.MedicalOxygen && f.Name.Contains("1.36")).Id;
            var oxygen98Id = products.First(f => f.Type == ProductType.MedicalOxygen && f.Name.Contains("9.8")).Id;
            var medicalAirId = products.First(f => f.Type == ProductType.MedicalAir).Id;
            var nitrousId = products.First(f => f.Type == ProductType.Nitrous).Id;

            var output = new GeneralStockOutputDto();
            var outputDetails = new List<GeneralStockDetailsDto>();

            var overallStocks = (from vs in await _virtualStockRepo.GetAllAsync()
                                 join vsd in await _virtualStockDetailRepo.GetAllAsync() on vs.Id equals vsd.VirtualStockId
                                 where vs.Date.Date < date.Date
                                 select new
                                 {
                                     vs.Date,
                                     WarehouseId = vs.ClientId,
                                     vsd.VirtualStockId,
                                     vs.VirtualStockType,
                                     vsd.ProductId,
                                     vsd.StockQty
                                 }).ToList();

            foreach (var client in clients) 
            {
                var virtualStockId = virtualStocks.OrderByDescending(o=> o.Date).FirstOrDefault(f=> f.ClientId == client.Id && f.VirtualStockType == VirtualStockType.ClientWarehouse)?.Id;
                var details = virtualStockDetails.Where(x => x.VirtualStockId == virtualStockId).ToList();
                var stock = new GeneralStockDetailsDto()
                {
                    WarehouseId = client.Id,
                    WarehouseName = client.Name,
                    VirtualStockType = VirtualStockType.ClientWarehouse
                };
               
                var thisOverallStocks = overallStocks.Where(x => x.WarehouseId == client.Id && x.VirtualStockType == VirtualStockType.ClientWarehouse && x.Date.Date < date.Date && x.StockQty > 0).ToList();
                stock.Oxygen136 = details.FirstOrDefault(f => f.ProductId == oxygen136Id)?.StockQty ?? 0;
                if (stock.Oxygen136 == 0)
                    stock.Oxygen136 = thisOverallStocks.OrderByDescending(o => o.Date).FirstOrDefault(x => x.ProductId == oxygen136Id)?.StockQty ?? 0;

                stock.Oxygen98 = details.FirstOrDefault(f => f.ProductId == oxygen98Id)?.StockQty ?? 0;
                if (stock.Oxygen98 == 0)
                    stock.Oxygen98 = thisOverallStocks.OrderByDescending(o => o.Date).FirstOrDefault(x => x.ProductId == oxygen98Id)?.StockQty ?? 0;

                stock.MedicalAir = details.FirstOrDefault(f => f.ProductId == medicalAirId)?.StockQty ?? 0;
                if (stock.MedicalAir == 0)
                    stock.MedicalAir = thisOverallStocks.OrderByDescending(o => o.Date).FirstOrDefault(x => x.ProductId == medicalAirId)?.StockQty ?? 0;

                stock.NitrousOxide = details.FirstOrDefault(f => f.ProductId == nitrousId)?.StockQty ?? 0;
                if (stock.NitrousOxide == 0)
                    stock.NitrousOxide = thisOverallStocks.OrderByDescending(o => o.Date).FirstOrDefault(x => x.ProductId == nitrousId)?.StockQty ?? 0;

                stock.Total = stock.Oxygen136 + stock.Oxygen98 + stock.MedicalAir + stock.NitrousOxide;
                outputDetails.Add(stock);
            }

            foreach (var supplier in suppliers)
            {
                var virtualStockId = virtualStocks.OrderByDescending(o => o.Date).FirstOrDefault(f => f.ClientId == supplier.Id && f.VirtualStockType == VirtualStockType.SupplierWarehouse)?.Id;
                var details = virtualStockDetails.Where(x => x.VirtualStockId == virtualStockId).ToList();
                var stock = new GeneralStockDetailsDto()
                {
                    WarehouseId = supplier.Id,
                    WarehouseName = supplier.Name,
                    VirtualStockType = VirtualStockType.SupplierWarehouse
                };

                var thisOverallStocks = overallStocks.Where(x => x.WarehouseId == supplier.Id && x.VirtualStockType == VirtualStockType.SupplierWarehouse && x.Date.Date < date.Date && x.StockQty > 0).ToList();

                stock.Oxygen136 = details.FirstOrDefault(f => f.ProductId == oxygen136Id)?.StockQty ?? 0;
                if (stock.Oxygen136 == 0)
                    stock.Oxygen136 = thisOverallStocks.OrderByDescending(o => o.Date).FirstOrDefault(x => x.ProductId == oxygen136Id)?.StockQty ?? 0;

                stock.Oxygen98 = details.FirstOrDefault(f => f.ProductId == oxygen98Id)?.StockQty ?? 0;
                if (stock.Oxygen98 == 0)
                    stock.Oxygen98 = thisOverallStocks.OrderByDescending(o => o.Date).FirstOrDefault(x => x.ProductId == oxygen98Id)?.StockQty ?? 0;

                stock.MedicalAir = details.FirstOrDefault(f => f.ProductId == medicalAirId)?.StockQty ?? 0;
                if (stock.MedicalAir == 0)
                    stock.MedicalAir = thisOverallStocks.OrderByDescending(o => o.Date).FirstOrDefault(x => x.ProductId == medicalAirId)?.StockQty ?? 0;

                stock.NitrousOxide = details.FirstOrDefault(f => f.ProductId == nitrousId)?.StockQty ?? 0;
                if (stock.NitrousOxide == 0)
                    stock.NitrousOxide = thisOverallStocks.OrderByDescending(o => o.Date).FirstOrDefault(x => x.ProductId == nitrousId)?.StockQty ?? 0;

                stock.Total = stock.Oxygen136 + stock.Oxygen98 + stock.MedicalAir + stock.NitrousOxide;
                outputDetails.Add(stock);
            }

            foreach (var sp in stockPointWarehouses)
            {
                var virtualStockId = virtualStocks.OrderByDescending(o => o.Date).FirstOrDefault(f => f.ClientId == sp.Id && f.VirtualStockType == VirtualStockType.StockPointWarehouse)?.Id;
                var details = virtualStockDetails.Where(x => x.VirtualStockId == virtualStockId).ToList();
                var stock = new GeneralStockDetailsDto()
                {
                    WarehouseId = sp.Id,
                    WarehouseName = sp.Name,
                    VirtualStockType = VirtualStockType.StockPointWarehouse
                };

                var thisOverallStocks = overallStocks.Where(x => x.WarehouseId == sp.Id && x.VirtualStockType == VirtualStockType.StockPointWarehouse && x.Date.Date < date.Date && x.StockQty > 0).ToList();

                stock.Oxygen136 = details.FirstOrDefault(f => f.ProductId == oxygen136Id)?.StockQty ?? 0;
                if (stock.Oxygen136 == 0)
                    stock.Oxygen136 = thisOverallStocks.OrderByDescending(o => o.Date).FirstOrDefault(x => x.ProductId == oxygen136Id)?.StockQty ?? 0;

                stock.Oxygen98 = details.FirstOrDefault(f => f.ProductId == oxygen98Id)?.StockQty ?? 0;
                if (stock.Oxygen98 == 0)
                    stock.Oxygen98 = thisOverallStocks.OrderByDescending(o => o.Date).FirstOrDefault(x => x.ProductId == oxygen98Id)?.StockQty ?? 0;

                stock.MedicalAir = details.FirstOrDefault(f => f.ProductId == medicalAirId)?.StockQty ?? 0;
                if (stock.MedicalAir == 0)
                    stock.MedicalAir = thisOverallStocks.OrderByDescending(o => o.Date).FirstOrDefault(x => x.ProductId == medicalAirId)?.StockQty ?? 0;

                stock.NitrousOxide = details.FirstOrDefault(f => f.ProductId == nitrousId)?.StockQty ?? 0;
                if (stock.NitrousOxide == 0)
                    stock.NitrousOxide = thisOverallStocks.OrderByDescending(o => o.Date).FirstOrDefault(x => x.ProductId == nitrousId)?.StockQty ?? 0;

                stock.Total = stock.Oxygen136 + stock.Oxygen98 + stock.MedicalAir + stock.NitrousOxide;
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

        public async Task<List<OverallVirtualInventoriesOutput>> GetActualVirtualInventoriesAsync(VirtualStockType type, int? warehouseId, int? itemId)
        {
            try
            {
                var output = (from i in await _virtualInventoryRepo.GetAllAsync()
                            join p in await _virtualItemRepo.GetAllAsync() on i.ProductId equals p.Id
                            where i.VirtualStockType == type 
                            && (warehouseId == null || i.WarehouseId == warehouseId) 
                            && (itemId == null || i.ProductId == itemId)
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

        public List<ComboboxItemDto> GetStockTypesSelectListAsync()
        {
            return ((VirtualStockType[])Enum.GetValues(typeof(VirtualStockType))).Select(c => new ComboboxItemDto() { Value = ((int)c).ToString(), DisplayText = c.DisplayName() }).ToList();
        }

        public async Task<List<PlantWarehouseSelectListDto>> GetPlantWarehouseAsync()
        {
            var output = (await _supplierRepo.GetAllListAsync(x => x.ActiveStatus)).Select(s => new PlantWarehouseSelectListDto()
            {
                Id = s.Id,
                DisplayText = s.Name,
                VirtualStockType = VirtualStockType.SupplierWarehouse
            }).ToList();

            output.AddRange((await _stockPointRepo.GetAllListAsync(x => x.ActiveStatus && x.StockPointType == StockPointType.Warehouse)).Select(s => new PlantWarehouseSelectListDto()
            {
                Id = s.Id,
                DisplayText = s.Name,
                VirtualStockType = VirtualStockType.StockPointWarehouse
            }).ToList());

            return output.Select((item, index) => new PlantWarehouseSelectListDto
            {
                Uid = index + 1,
                Id = item.Id,
                DisplayText = item.DisplayText,
                VirtualStockType = item.VirtualStockType
            }).ToList();
        }
    }
}
