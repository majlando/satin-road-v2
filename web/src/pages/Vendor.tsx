import { useEffect, useState } from "react";
import { Link, useParams } from "react-router";
import { Box } from "../Box";
import { api, errorText, type FeaturedVendorDto, type ListingDto } from "../api/client";
import { ListingsTable } from "./Browse";

/** One vendor's shop. There is no vendor endpoint, so it picks their listings out of all of them. */
export function Vendor() {
    const id = Number(useParams().id);
    const [listings, setListings] = useState<ListingDto[] | null>(null);
    const [featured, setFeatured] = useState<FeaturedVendorDto[]>([]);
    const [error, setError] = useState("");

    useEffect(() => {
        api.listings.listingsList({})
            .then((r) => setListings(r.data.filter((l) => l.vendorId === id)))
            .catch((e) => setError(errorText(e)));
        api.vendors.vendorsFeaturedList().then((r) => setFeatured(r.data)).catch((e) => setError(errorText(e)));
    }, [id]);

    const featuredEntry = featured.find((f) => f.vendorId === id);
    const name = listings?.[0]?.vendorName ?? featuredEntry?.vendorName ?? "Vendor";

    return (
        <Box title={`Vendor: ${name}`}>
            <p>
                <Link to="/">&laquo; Back to browsing</Link>
            </p>
            {featuredEntry && (
                <p>
                    <span className="tag">[Featured]</span> {featuredEntry.sales} sales and counting.
                </p>
            )}
            {error && <p className="error">{error}</p>}
            {!listings && !error && <p className="muted">Loading…</p>}
            {listings?.length === 0 && <p className="muted">This vendor has nothing for sale.</p>}
            {listings && listings.length > 0 && (
                <ListingsTable listings={listings} featuredIds={new Set(featuredEntry ? [id] : [])} />
            )}
        </Box>
    );
}
