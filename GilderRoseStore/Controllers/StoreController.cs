using GilderRoseStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace GilderRoseStore.Controllers
{
    [Authorize]
    public class StoreController : ApiController
    {
        private readonly IInventory inventory;

        public StoreController(IInventory inventory)
        {
            this.inventory = inventory;
        }

        public IEnumerable<Item> Get()
        {            
            return inventory.Items;
        }

        public bool Get(Guid id)
        {
            return inventory.BuyItem(id);
        }

    }
}
