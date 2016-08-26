using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;


namespace GilderRoseStore.Models
{

    /// <summary>
    /// Simple implementation of the Inventory
    /// </summary>
    public class InMemoryInventory : IInventory
    {
        //using a concurrent bag to store the inventory items
        //the fact is "concurrent" allows the list to be traversed while safely other threads can add item to the list
        ConcurrentBag<Item> items = new ConcurrentBag<Item>();
        private object _sync = new object();

        /// <summary>
        /// The constructor creates 4 items in the inventory
        /// </summary>
        public InMemoryInventory()
        {
            this.items.Add(new Models.Item { Name = "item1", Description = "finest item1", Price = 10, Quantity = 2 });
            this.items.Add(new Models.Item { Name = "item2", Description = "finest item2", Price = 5, Quantity = 3 });
            this.items.Add(new Models.Item { Name = "item3", Description = "finest item3", Price = 4, Quantity = 5 });
            this.items.Add(new Models.Item { Name = "item4", Description = "finest item4", Price = 3, Quantity = 15 });
        }
        
        public IEnumerable<Item> Items
        {
            get
            {
                return items;
            }
        }

        public bool BuyItem(Guid itemId, out Item item)
        {            
            item = items.FirstOrDefault(itm => itm.Id == itemId);
            //syncronizing the access the item when decreasing the quantity 
            //prevents multiple threads from modifying the chosed item properties.
            //in this case its only use to decrease the quantity when a user purcase an item
            //however until this operation executes the stock can indicate two clients that an item is available
            //for both despite having just one in stock
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