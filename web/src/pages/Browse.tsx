import { useEffect, useState } from "react";
import { Link, useSearchParams } from "react-router";
import { Box } from "../Box";
import { api, errorText, money, type CategoryDto, type ListingDto } from "../api/client";

/** The landing page: every listing, or one category's (?category=ID), featured vendors first. */
export function Browse() {
    const [params] = useSearchParams();
    const categoryId = params.get("category") ? Number(params.get("category")) : undefined;
    const [categories, setCategories] = useState<CategoryDto[]>([]);
    const [featuredIds, setFeaturedIds] = useState<Set<number>>(new Set());
    const [listings, setListings] = useState<ListingDto[]>([]);
    const [error, setError] = useState("");

    // Categories (for the title) and featured vendors (for the order) load once.
    useEffect(() => {
        api.categories.categoriesList().then((r) => setCategories(r.data)).catch((e) => setError(errorText(e)));
        api.vendors.vendorsFeaturedList()
            .then((r) => setFeaturedIds(new Set(r.data.map((f) => f.vendorId))))
            .catch((e) => setError(errorText(e)));
    }, []);

    // Listings load again whenever the chosen category changes.
    useEffect(() => {
        api.listings.listingsList({ categoryId }).then((r) => setListings(r.data)).catch((e) => setError(errorText(e)));
    }, [categoryId]);

    // Featured vendors' listings go first. sort() keeps the rest in title order.
    const sorted = [...listings].sort(
        (a, b) => Number(featuredIds.has(b.vendorId)) - Number(featuredIds.has(a.vendorId)),
    );
    const categoryName = categories.find((c) => c.id === categoryId)?.name;

    return (
        <Box title={categoryName ? `Listings: ${categoryName}` : "All listings"}>
            {error && <p className="error">{error}</p>}
            {sorted.length === 0 && !error && <p className="muted">Nothing for sale here.</p>}
            {sorted.length > 0 && <ListingsTable listings={sorted} featuredIds={featuredIds} />}
        </Box>
    );
}

/** The table of listings used by Browse and the vendor page. */
export function ListingsTable({ listings, featuredIds }: { listings: ListingDto[]; featuredIds: Set<number> }) {
    return (
        <div className="table-wrap">
            <table>
                <thead>
                <tr>
                    <th>Title</th>
                    <th>Vendor</th>
                    <th>Category</th>
                    <th className="num">Price</th>
                    <th className="num">Stock</th>
                </tr>
                </thead>
                <tbody>
                {listings.map((l) => (
                    <tr key={l.id}>
                        <td>
                            <Link to={`/listings/${l.id}`}>{l.title}</Link>
                            {featuredIds.has(l.vendorId) && <span className="tag">[Featured]</span>}
                        </td>
                        <td>
                            <Link to={`/vendors/${l.vendorId}`}>{l.vendorName}</Link>
                        </td>
                        <td>{l.categoryName}</td>
                        <td className="num">{money(l.priceCents)}</td>
                        <td className="num">{l.stock > 0 ? l.stock : "Sold out"}</td>
                    </tr>
                ))}
                </tbody>
            </table>
        </div>
    );
}
