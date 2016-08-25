using GilderRoseStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace GilderRoseStore.Controllers
{
   // [Authorize]
    public class StoreController : ApiController
    {
        public IEnumerable<Item> Get()
        {
            var imi = new InMemoryInventory();
            return imi.Items;
        }

        public bool Get(Guid id)
        {
            var imi = new InMemoryInventory();
            return imi.BuyItem(id);
        }

    }
}
