using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.EntityFrameworkCore.Repositories;
using LbI.Entities;
using LbI.Enums;
using LbI.Helpers;
using LbI.Purchases.Dto;
using LbI.Sales.Dto;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace LbI.Purchases
{
    public class PurchaseAppService : LbIAppServiceBase, IPurchaseAppService
    {
        private readonly IRepository<Purchase> _purchaseRepo;
        private readonly IRepository<PurchaseDetail> _purchaseDetailsRepo;
        private readonly IRepository<DuePaymentHistory> _duePaymentHistoryRepo;
        private readonly IRepository<Inventory> _inventoryRepo;
        private readonly IRepository<Product> _productRepo;
        private readonly IRepository<StockPoint> _vehicleRepo;
        private readonly IRepository<LbiSetting> _lbiSettingsRepo;
        public PurchaseAppService(
            IRepository<Purchase> purchaseRepo,
            IRepository<PurchaseDetail> purchaseDetailsRepo,
            IRepository<Inventory> inventoryRepo,
            IRepository<Product> productRepo,
            IRepository<StockPoint> vehicleRepo,
            IRepository<LbiSetting> lbiSettingsRepo,
            IRepository<DuePaymentHistory> duePaymentHistoryRepo
            )
        {
            _purchaseRepo = purchaseRepo;
            _purchaseDetailsRepo = purchaseDetailsRepo;
            _inventoryRepo = inventoryRepo;
            _productRepo = productRepo;
            _vehicleRepo = vehicleRepo;
            _lbiSettingsRepo = lbiSettingsRepo;
            _duePaymentHistoryRepo = duePaymentHistoryRepo;
        }

        public async Task<PagedResultDto<PurchaseOutputDto>> GetPaginatedPurchasesAsync(PurchasesFilterDto filter)
        {
            var searchText = string.IsNullOrEmpty(filter.SearchText) ? null : filter.SearchText.ToLower();
            var query = (from p in await _purchaseRepo.GetAllAsync()
                         join v in await _vehicleRepo.GetAllAsync() on p.StockPointId equals v.Id
                         select new PurchaseOutputDto()
                         {
                             Id = p.Id,
                             Date = p.Date,
                             InvoiceNumber = p.InvoiceNumber,
                             SupplierId = p.SupplierId,
                             SupplierName = p.SupplierName,
                             TotalAmount = p.TotalAmount,
                             Discount = p.Discount,
                             NetAmount = p.NetAmount,
                             PaidAmount = p.PaidAmount,
                             DueAmount = p.DueAmount,
                             PaymentStatus = p.PaymentStatus,
                             StockPointId = p.StockPointId,
                             StockPointName = v.Name,
                             Remarks = p.Remarks,
                             Locked = p.Locked
                         }).AsQueryable();

            if (searchText != null)
            {
                query = query.Where(x =>
                x.InvoiceNumber.ToLower().Contains(searchText));
            }

            var items = query.OrderByDescending(o => o.Id).Skip(filter.Skip).Take(filter.Take).ToList();
            foreach (var p in items)
            {
                p.PaymentStatusText = p.PaymentStatus.DisplayName();
            }

            return new PagedResultDto<PurchaseOutputDto>()
            {
                Items = items,
                TotalCount = query.Count()
            };
        }

        public async Task<List<PurchaseProductDto>> GetAllProductsAsync(int? stockPointId)
        {
            var output = new List<PurchaseProductDto>();
            output = (from p in await _productRepo.GetAllAsync()
                     //join i in await _inventoryRepo.GetAllAsync() on p.Id equals i.ProductId into inventory
                     //from i in inventory.DefaultIfEmpty()
                     where p.ActiveStatus
                     select new PurchaseProductDto()
                     {
                         ProductId = p.Id,
                         Name = p.Name,
                         Size = p.Size,
                         Type = p.Type,
                         PurchasePrice = p.PurchasePrice,
                         PurchasePriceDisabled = true,
                         QtyDisabled = true,
                         Quantity = 0,
                         TotalPrice = 0,
                         //Stock = i.StockQty,
                         TypeText = p.Type.DisplayName(),
                         SizeText = p.Size.DisplayName()
                     }).ToList();
            if (stockPointId != null) 
            { 
                var stocks = await _inventoryRepo.GetAllListAsync(x=> x.StockPointId == stockPointId);
                foreach (var item in output) 
                {
                    var stock = stocks.Where(x => x.ProductId == item.ProductId).FirstOrDefault()?.StockQty;
                    item.Stock = stock != null ? stock : 0; 
                }
            }

            return output;
        }

        public async Task<PurchaseEntryInput> GetAsync(int id)
        {
            var entity = await _purchaseRepo.GetAsync(id);
            var details = await _purchaseDetailsRepo.GetAllListAsync(x=> x.PurchaseId == id);

            return new PurchaseEntryInput()
            {
                Purchase = ObjectMapper.Map<PurchaseEntryDto>(entity),
                PurchaseDetails = ObjectMapper.Map<List<PurchaseDetailsEntryDto>>(details)
            };
        }

        public async Task<string> GetLastInvoiceNumberAsync()
        {
            var settings = await _lbiSettingsRepo.FirstOrDefaultAsync(x => x.Key == InitialSetupKey.LastPurchaseInvoiceNumber);
            if (settings != null)
            {
                return settings.Value;
            } 
            else
            {
                await _lbiSettingsRepo.InsertAsync(new LbiSetting()
                {
                    Key = InitialSetupKey.LastPurchaseInvoiceNumber,
                    Value = "1001"
                });
                return "1001";
            }
        }

        [UnitOfWork]
        public async Task CreateOrUpdateAsync(PurchaseEntryInput input)
        {
            var id = input.Purchase.Id;
            if (id.HasValue)
            {
                var purchase = await _purchaseRepo.GetAsync(id.Value);
                ObjectMapper.Map(input.Purchase, purchase);
                await _purchaseRepo.UpdateAsync(purchase);

                var prevPurchaseDetails = await _purchaseDetailsRepo.GetAllListAsync(x => x.PurchaseId == id);
                foreach (var pd in prevPurchaseDetails)
                {
                    var inventory = await _inventoryRepo.FirstOrDefaultAsync(f => f.ProductId == pd.ProductId && f.StockPointId == input.Purchase.StockPointId);
                    if (inventory != null)
                    {
                        inventory.StockQty -= pd.Quantity;
                        await _inventoryRepo.UpdateAsync(inventory);
                    }
                }
                await _purchaseDetailsRepo.BatchDeleteAsync(x => x.PurchaseId == id);
                await InsertPurchaseDetails(input.PurchaseDetails, id.Value, input.Purchase.StockPointId);
            }
            else
            {
                var purchase = ObjectMapper.Map<Purchase>(input.Purchase);
                id = await _purchaseRepo.InsertAndGetIdAsync(purchase);

                await InsertPurchaseDetails(input.PurchaseDetails, id.Value, input.Purchase.StockPointId);
                input.DuePayment.PurchaseId = id.Value;
                input.DuePayment.Default = true;
                await InsertDuePaymentAsync(input.DuePayment);

                var invoiceSettings = await _lbiSettingsRepo.SingleAsync(x => x.Key == InitialSetupKey.LastPurchaseInvoiceNumber);
                invoiceSettings.Value = (Convert.ToInt32(invoiceSettings.Value) + 1).ToString();
                await _lbiSettingsRepo.UpdateAsync(invoiceSettings);

            }
        }

        [UnitOfWork]
        public async Task DuePaymentEntryAsync(DuePaymentEntryDto input)
        {
            var purchase = await _purchaseRepo.SingleAsync(s => s.Id == input.PurchaseId);
            purchase.Discount += input.Discount;
            purchase.NetAmount = input.NetTotal;
            purchase.PaidAmount += input.TotalPaid;
            purchase.DueAmount = input.Due;
            purchase.Remarks = input.Remarks;
            purchase.PaymentStatus = purchase.DueAmount == 0 ? PaymentStatus.Paid : purchase.TotalAmount > purchase.DueAmount ? PaymentStatus.Partialpaid : PaymentStatus.Due;
            await _purchaseRepo.UpdateAsync(purchase);
            await InsertDuePaymentAsync(input.DuePayment);
        }

        public async Task<List<DuePaymentHistoryDto>> GetDuePaymentHistoriesAsync(int purchaseId)
        {
            var histories = (await _duePaymentHistoryRepo.GetAllListAsync(x => x.PurchaseId == purchaseId)).OrderBy(o=>o.CreationTime).ToList();
            return ObjectMapper.Map<List<DuePaymentHistoryDto>>(histories);
        }

        private async Task InsertPurchaseDetails(List<PurchaseDetailsEntryDto> purchaseDetailsInput, int purchaseId, int vehicleId)
        {
            var purchaseDetails = new List<PurchaseDetail>();
            foreach (var pd in purchaseDetailsInput)
            {
                var inventory = await _inventoryRepo.FirstOrDefaultAsync(f => f.ProductId == pd.ProductId && f.StockPointId == vehicleId);
                if (inventory != null)
                {
                    inventory.StockQty += pd.Quantity;
                    await _inventoryRepo.UpdateAsync(inventory);
                }
                else
                {
                    inventory = new Inventory()
                    {
                        ProductId = pd.ProductId,
                        StockPointId = vehicleId,
                        ProductName = pd.ProductName,
                        StockQty = pd.Quantity,
                    };
                    await _inventoryRepo.InsertAsync(inventory);
                }

                var detail = ObjectMapper.Map<PurchaseDetail>(pd);
                detail.PurchaseId = purchaseId;
                purchaseDetails.Add(detail);
            }
            await _purchaseDetailsRepo.InsertRangeAsync(purchaseDetails);
        }

        public List<ComboboxItemDto> GetPaymentStatusSelectListAsync()
        {
            var output = ((PaymentStatus[])Enum.GetValues(typeof(PaymentStatus))).Select(c => new ComboboxItemDto() { Value = ((int)c).ToString(), DisplayText = c.DisplayName() }).ToList();
            return output;
        }

        private async Task InsertDuePaymentAsync(DuePaymentHistoryDto input)
        {
            var dp = ObjectMapper.Map<DuePaymentHistory>(input);
            await _duePaymentHistoryRepo.InsertAsync(dp);
        }
    }
}
