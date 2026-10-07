import { useEffect, useState } from "react";
import { Link } from "react-router";
import { api, errorText, money, type CategoryDto, type FeaturedVendorDto, type ListingDto } from "../api/client";

/** The landing page: featured vendors, category filter, and the listing grid. */
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
        <>
            {featured.length > 0 && (
                <section className="card">
                    <h2>Featured vendors</h2>
                    <ul className="featured-list">
                        {featured.map((f) => (
                            <li key={f.vendorId}>
                                <strong>{f.vendorName}</strong> <span className="muted">{f.sales} sales</span>
                            </li>
                        ))}
                    </ul>
                </section>
            )}

            <section>
                <h2>Listings</h2>
                <div className="filters">
                    <button className={categoryId === undefined ? "active" : ""} onClick={() => setCategoryId(undefined)}>
                        All
                    </button>
                    {categories.map((c) => (
                        <button key={c.id} className={categoryId === c.id ? "active" : ""} onClick={() => setCategoryId(c.id)}>
                            {c.name}
                        </button>
                    ))}
                </div>

                {error && <p className="error">{error}</p>}
                {sorted.length === 0 && !error && <p className="muted">Nothing for sale here.</p>}

                <div className="grid">
                    {sorted.map((l) => (
                        <Link key={l.id} to={`/listings/${l.id}`} className="card listing">
                            {featuredIds.has(l.vendorId) && <span className="badge">Featured</span>}
                            <h3>{l.title}</h3>
                            <p className="muted">
                                {l.vendorName} · {l.categoryName}
                            </p>
                            <p className="price">{money(l.priceCents)}</p>
                        </Link>
                    ))}
                </div>
            </section>
        </>
    );
}