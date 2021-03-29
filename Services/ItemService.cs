using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using doggo.Models;
using doggo.Helpers;
using doggo.Data;

namespace doggo.Services
{
    public interface IItemService
    {
        IEnumerable<StockSummaryDTO> StockSummary();
        Task<IEnumerable<ItemDTO>> GetItems();
    }

    public class ItemService : IItemService
    {
        private readonly AppSettings _appSettings;
        private readonly DBContext db;

        public ItemService(IOptions<AppSettings> appSettings, DBContext context)
        {
            _appSettings = appSettings.Value;
            db = context;
        }
        
        public IEnumerable<StockSummaryDTO> StockSummary(){
            var res = (
                from i in db.Item
                join s in (from rs in db.ItemStock group rs by rs.ItemId into ss
                select new {
                    ItemId=ss.Key,
                    Current=ss.Sum(d => (Int32)(d.Type=="Increment"?d.Amount:(d.Amount * -1))),
                    Increment=ss.Sum(d => (Int32)(d.Type=="Increment"?d.Amount:0)),
                    Decrement=ss.Sum(d => (Int32)(d.Type=="Decrement"?d.Amount:0))
                }) on i.Id equals s.ItemId into gis
                from s in gis.DefaultIfEmpty()
                select new StockSummaryDTO {
                    Id=i.Id,
                    Name=i.Name,
                    Location=i.Location,
                    Current=s!=null?s.Current:0,
                    Increment=s!=null?s.Increment:0,
                    Decrement=s!=null?s.Decrement:0
                }
            );

            return res;
        }

        public async Task<IEnumerable<ItemDTO>> GetItems(){
            return await db.Item.ToListAsync();
        }
    }
}