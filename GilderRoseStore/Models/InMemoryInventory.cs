using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;


namespace GilderRoseStore.Models
{
    public class InMemoryInventory : IInventory
    {
        public InMemoryInventory()
        {
            this.items.Add(new Models.Item { Name = "item1", Description = "finest item1", Price = 10, Quantity = 2 });
            this.items.Add(new Models.Item { Name = "item2", Description = "finest item2", Price = 5, Quantity = 3 });
            this.items.Add(new Models.Item { Name = "item3", Description = "finest item3", Price = 4, Quantity = 5 });
            this.items.Add(new Models.Item { Name = "item4", Description = "finest item4", Price = 3, Quantity = 15 });
        }
        
        ConcurrentBag<Item> items = new ConcurrentBag<Item>();
        private object _sync = new object();
        public IEnumerable<Item> Items
        {
            get
            {
                return items;
            }
        }

        public bool BuyItem(Guid itemId)
        {            
            var item = items.FirstOrDefault(itm => itm.Id == itemId);
            lock (_sync)
            {
                if (item != null && item.Quantity > 0)
                {
                    item.Quantity--;                
                    return true;
                }
                return false;
            }
        }
    }
}