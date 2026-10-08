import { useEffect, useState } from "react";
import { Link } from "react-router";
import { api, errorText, money, type CategoryDto, type FeaturedVendorDto, type ListingDto } from "../api/client";

/** The landing page: category sidebar, featured vendors, and the listings table. */
export function Browse() {
    const [categories, setCategories] = useState<CategoryDto[]>([]);
    const [featured, setFeatured] = useState<FeaturedVendorDto[]>([]);
    const [listings, setListings] = useState<ListingDto[]>([]);
    const [categoryId, setCategoryId] = useState<number | undefined>(undefined);
    const [error, setError] = useState("");

    // Categories and featured vendors load once.
    useEffect(() => {
        api.categories.categoriesList().then((r) => setCategories(r.data)).catch((e) => setError(errorText(e)));
        api.vendors.vendorsFeaturedList().then((r) => setFeatured(r.data)).catch((e) => setError(errorText(e)));
    }, []);

    // Listings load again whenever the chosen category changes.
    useEffect(() => {
        api.listings.listingsList({ categoryId }).then((r) => setListings(r.data)).catch((e) => setError(errorText(e)));
    }, [categoryId]);

    // Featured vendors' listings go first. sort() keeps the rest in title order.
    const featuredIds = new Set(featured.map((f) => f.vendorId));
    const sorted = [...listings].sort(
        (a, b) => Number(featuredIds.has(b.vendorId)) - Number(featuredIds.has(a.vendorId)),
    );

    return (
        <div className="layout">
            <aside className="sidebar">
                <h2>Categories</h2>
                <ul>
                    <li>
                        <button
                            className="linklike"
                            aria-pressed={categoryId === undefined}
                            onClick={() => setCategoryId(undefined)}
                        >
                            All
                        </button>
                    </li>
                    {categories.map((c) => (
                        <li key={c.id}>
                            <button
                                className="linklike"
                                aria-pressed={categoryId === c.id}
                                onClick={() => setCategoryId(c.id)}
                            >
                                {c.name}
                            </button>
                        </li>
                    ))}
                </ul>
            </aside>

            <section>
                {featured.length > 0 && (
                    <p className="featured">
                        <strong>Featured vendors:</strong>{" "}
                        {featured.map((f, i) => (
                            <span key={f.vendorId}>
                                {i > 0 && ", "}
                                {f.vendorName} <span className="muted">({f.sales} sales)</span>
                            </span>
                        ))}
                    </p>
                )}

                <h2>Listings</h2>
                {error && <p className="error">{error}</p>}
                {sorted.length === 0 && !error && <p className="muted">Nothing for sale here.</p>}

                {sorted.length > 0 && (
                    <div className="table-wrap">
                        <table>
                            <thead>
                            <tr>
                                <th>Title</th>
                                <th>Vendor</th>
                                <th>Category</th>
                                <th className="num">Price</th>
                            </tr>
                            </thead>
                            <tbody>
                            {sorted.map((l) => (
                                <tr key={l.id}>
                                    <td>
                                        <Link to={`/listings/${l.id}`}>{l.title}</Link>
                                        {featuredIds.has(l.vendorId) && <span className="tag">Featured</span>}
                                    </td>
                                    <td>{l.vendorName}</td>
                                    <td>{l.categoryName}</td>
                                    <td className="num">{money(l.priceCents)}</td>
                                </tr>
                            ))}
                            </tbody>
                        </table>
                    </div>
                )}
            </section>
        </div>
    );
}