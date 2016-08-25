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
        public IEnumerable<Item> Items
        {
            get
            {
                return items;
            }
        }

        public bool BuyItem(Guid itemId)
        {
            var item = items.FirstOrDefault(itm => itm.Id.Equals(itemId));
            //thread safe checking an item quantity is greater than zero
            //the item is checked than removed from the list to insure is not being altered by concurrent threads
            //and only than is safely added back to the concurrent bag
            if (item != null && item.Quantity > 0 && items.TryTake(out item))
            {
                if (item.Quantity == 0)
                {
                    items.Add(item);
                    return false;
                }
                item.Quantity--;
                items.Add(item);
                return true;
            }
            return false;

        }
    }
}