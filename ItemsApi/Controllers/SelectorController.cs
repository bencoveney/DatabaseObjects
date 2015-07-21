﻿namespace ItemsApi.Controllers
{
    using Items;
    using ItemsApi;
    using ItemsApi.Models;
    using System.Collections.Generic;
    using System.Web.Http;
    using System.Linq;

    public class ItemSelectorController : ApiController
    {
        public IEnumerable<ItemSelector> GetAllItemSelectors()
        {
            foreach(Item item in WebApiApplication.Model.Items)
            {
                ItemSelector selector = new ItemSelector() { ItemName = item.Name, Relationships = new List<PotentialRelationship>() };
                foreach(Relationship relationship in item.GetReferenceRelationships(WebApiApplication.Model))
                {
                    RelationshipLink sourceLink = relationship.Links.Single(link => link.Thing == item);
                    RelationshipLink targetLink = relationship.Links.Single(link => link.Thing != item);

                    selector.Relationships.Add(new PotentialRelationship()
                    {
                        RelationshipName = relationship.Name,
                        ThingTarget = targetLink.Thing.Name,
                        SourceNumberLower = sourceLink.AmountLower,
                        SourceNumberUpper = sourceLink.AmountUpper,
                        TargetNumberLower = targetLink.AmountLower,
                        TargetNumberUpper = targetLink.AmountUpper,
                    });
                }

                yield return selector;
            }
        }

        public IHttpActionResult GetItemSelector(string name)
        {
            if (string.IsNullOrEmpty(name))
                return BadRequest("Name must be provided");

            ItemSelector selector = GetAllItemSelectors().Single(itemSelector => itemSelector.ItemName == name);

            if (selector == null)
                return NotFound();

            return Ok(selector);
        }
    }
}
