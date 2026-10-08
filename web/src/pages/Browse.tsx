import { useEffect, useState } from "react";
import { Link, useSearchParams } from "react-router";
import { Box } from "../Box";
import { api, errorText, money, type CategoryDto, type ListingDto } from "../api/client";

/**
 * The landing page: every listing, or one category's (?category=ID).
 * The API puts featured vendors' listings first.
 */
export function Browse() {
    const [params] = useSearchParams();
    const raw = Number(params.get("category"));
    const categoryId = Number.isInteger(raw) && raw > 0 ? raw : undefined; // ignore a missing or garbled id
    const [categories, setCategories] = useState<CategoryDto[]>([]);
    const [listings, setListings] = useState<ListingDto[] | null>(null);
    const [error, setError] = useState("");

    // Categories (for the title) load once.
    useEffect(() => {
        api.categories.categoriesList().then((r) => setCategories(r.data)).catch((e) => setError(errorText(e)));
    }, []);

    // Listings load again whenever the chosen category changes.
    useEffect(() => {
        setError("");
        api.listings.listingsList({ categoryId }).then((r) => setListings(r.data)).catch((e) => setError(errorText(e)));
    }, [categoryId]);

    const categoryName = categories.find((c) => c.id === categoryId)?.name;

    return (
        <Box title={categoryName ? `Listings: ${categoryName}` : "All listings"}>
            {error && <p className="error">{error}</p>}
            {!listings && !error && <p className="muted">Loading…</p>}
            {listings?.length === 0 && <p className="muted">Nothing for sale here.</p>}
            {listings && listings.length > 0 && <ListingsTable listings={listings} />}
        </Box>
    );
}

/** The table of listings used by Browse and the vendor page. */
export function ListingsTable({ listings }: { listings: ListingDto[] }) {
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
                            {l.vendorFeatured && <span className="tag">[Featured]</span>}
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
