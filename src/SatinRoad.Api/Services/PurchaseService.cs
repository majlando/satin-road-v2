namespace SatinRoad.Api.Services;

/// <summary>
/// Buying one listing. Everything happens inside one transaction, so a purchase
/// either happens completely or not at all.
/// </summary>
public class PurchaseService(AppDb db, RaidPolicy raids, IRaidRoller roller)
{
    public async Task<OrderRecord> BuyAsync(UserRecord buyer, int listingId, int quantity)
    {
        if (quantity < 1)
            throw AppException.BadRequest("Quantity must be at least 1.");

        await using var transaction = await db.BeginTransactionAsync();

        var listing = await db.Listings.FirstOrDefaultAsync(l => l.Id == listingId)
            ?? throw AppException.NotFound("Listing");
        var vendor = await db.Users.FirstAsync(u => u.Id == listing.VendorId);

        // Checked before "removed": a raid removes every listing, and buying
        // from a raided vendor must say why it failed.
        if (vendor.IsSeized)
            throw AppException.Conflict("This vendor was shut down by the FBI.");
        if (listing.IsRemoved)
            throw AppException.NotFound("Listing");
        if (vendor.Id == buyer.Id)
            throw AppException.BadRequest("You cannot buy your own listing.");
        if (quantity > listing.Stock)
            throw AppException.Conflict($"Only {listing.Stock} left in stock.");

        // The loyalty rule counts earlier completed orders with this vendor.
        var earlierOrders = await db.Orders.CountAsync(o =>
            o.BuyerId == buyer.Id && o.VendorId == vendor.Id && o.Status == OrderStatus.Completed);
        var price = PricingRules.Calculate(listing.PriceCents * quantity, earlierOrders);

        var order = new OrderRecord
        {
            BuyerId = buyer.Id,
            VendorId = vendor.Id,
            ListingId = listing.Id,
            Quantity = quantity,
            SubtotalCents = price.SubtotalCents,
            DiscountCents = price.DiscountCents,
            TotalCents = price.TotalCents,
            Status = OrderStatus.Completed,
        };

        if (raids.IsRaid(roller))
        {
            // The buyer was the FBI. No sale happens, so stock stays as it is.
            // The vendor is shut down and every one of their listings removed.
            order.Status = OrderStatus.Seized;
            await db.Users.Where(u => u.Id == vendor.Id)
                .Set(u => u.IsSeized, true)
                .UpdateAsync();
            await db.Listings.Where(l => l.VendorId == vendor.Id)
                .Set(l => l.IsRemoved, true)
                .UpdateAsync();
        }
        else
        {
            // Only take the stock if it is still there: a last line of
            // defence, so the stock can never go below zero.
            var updated = await db.Listings.Where(l => l.Id == listing.Id && l.Stock >= quantity)
                .Set(l => l.Stock, l => l.Stock - quantity)
                .UpdateAsync();
            if (updated == 0)
                throw AppException.Conflict("Not enough stock left.");
        }

        order.Id = await db.InsertWithInt32IdentityAsync(order);
        await transaction.CommitAsync();
        return order;
    }
}