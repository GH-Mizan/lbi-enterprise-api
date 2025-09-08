using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using LbI.Entities;
using LbI.Enums;
using LbI.Helpers;
using LbI.Products.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LbI.Products
{
    public class ProductAppService : LbIAppServiceBase, IProductAppService
    {
        private readonly IRepository<Product> _productRepo;
        private readonly IRepository<ProductHistory> _productHistoryRepo;
        public ProductAppService(IRepository<Product> productRepo,
            IRepository<ProductHistory> productHistoryRepo)
        {
            _productRepo = productRepo;
            _productHistoryRepo = productHistoryRepo;
        }

        public async Task<PagedResultDto<ProductOutputDto>> GetPaginatedProductsAsync(ProductsFilterDto filter)
        {
            var searchText = string.IsNullOrEmpty(filter.SearchText) ? null : filter.SearchText.ToLower();
            var query = (from p in await _productRepo.GetAllAsync()
                         select new ProductOutputDto()
                         {
                             Id = p.Id,
                             Name = p.Name,
                             Size = p.Size,
                             Type = p.Type,
                             PurchasePrice = p.PurchasePrice,
                             SellPrice  = p.SellPrice,
                             ActiveStatus = p.ActiveStatus
                         }).AsQueryable();

            if (searchText != null)
            {
                query = query.Where(x =>
                x.Name.ToLower().Contains(searchText) ||
                x.Size.ToLower().Contains(searchText));
            }

            var products = query.OrderBy(o => o.Name).Skip(filter.Skip).Take(filter.Take).ToList();
            foreach (var p in products) {
                p.TypeText = p.Type.DisplayName();
            }

            return new PagedResultDto<ProductOutputDto>()
            {
                Items = products,
                TotalCount = query.Count()
            };
        }

        public async Task<List<ProductOutputDto>> GetAllAsync()
        {
            return (await _productRepo.GetAllListAsync(x => x.ActiveStatus)).Select(p => new ProductOutputDto()
            {
                Id = p.Id,
                Name = p.Name,
                Size = p.Size,
                Type = p.Type,
                PurchasePrice = p.PurchasePrice,
                SellPrice = p.SellPrice,
                ActiveStatus = p.ActiveStatus,
                TypeText = p.Type.DisplayName()
            }).ToList();
        }
        public async Task<ProductCreateOrUpdateDto> GetAsync(int id)
        {
            var entity = await _productRepo.GetAsync(id);
            return ObjectMapper.Map<ProductCreateOrUpdateDto>(entity);
        }

        public async Task CreateOrUpdateAsync(ProductCreateOrUpdateDto input)
        {
            if (input.Id.HasValue)
            {
                var product = await _productRepo.GetAsync(input.Id.Value);
                ObjectMapper.Map(input, product);
                await _productRepo.UpdateAsync(product);

                var history = MapProductHistory(input, input.Id.Value);
                await _productHistoryRepo.InsertAsync(history);
            }
            else
            {
                var product = ObjectMapper.Map<Product>(input);
                var productId = await _productRepo.InsertAndGetIdAsync(product);

                var history = MapProductHistory(input, productId);
                await _productHistoryRepo.InsertAsync(history);
            }
        }

        private ProductHistory  MapProductHistory(ProductCreateOrUpdateDto input, int productId)
        {
            return new ProductHistory()
            {
                ProductId = productId,
                Name = input.Name,
                Type = input.Type,
                Size = input.Size,
                PurchasePrice = input.PurchasePrice,
                SellPrice = input.SellPrice,
                ActiveStatus = input.ActiveStatus
            };
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _productRepo.GetAsync(id);
            await _productRepo.DeleteAsync(product);
        }

        public async Task<List<ProductOutputDto>> GetProductHistoriesAsync(int productId)
        {
            return (await _productHistoryRepo.GetAllListAsync(x => x.ProductId == productId)).OrderByDescending(o => o.Id).Select(s => new ProductOutputDto()
            {
                Name = s.Name,
                Type = s.Type,
                TypeText = s.Type.DisplayName(),
                Size = s.Size,
                PurchasePrice = s.PurchasePrice,
                SellPrice = s.SellPrice,
                ActiveStatus = s.ActiveStatus
            }).ToList();
        }

        public List<ComboboxItemDto> GetProductTypeSelectListAsync()
        {
            var output = ((ProductType[])Enum.GetValues(typeof(ProductType))).Select(c => new ComboboxItemDto() { Value = ((int)c).ToString(), DisplayText = c.DisplayName() }).ToList();
            return output;
        }
    }
}
