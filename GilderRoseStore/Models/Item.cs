using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GilderRoseStore.Models
{
    /// <summary>
    /// the item for sale
    /// </summary>
    public class Item
    {
        public Item()
        {
            this.Id = Guid.NewGuid();
        }
        public Guid Id { set; get; }
        public int Quantity { set; get; }        
        //maybe name should be immutable?
        public string Name { set; get; }
        public string Description { set; get; }
        public int Price { get; set; }
    }    
   
}