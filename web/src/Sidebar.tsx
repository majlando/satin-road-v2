import { useEffect, useState } from "react";
import { Link, useLocation } from "react-router";
import { AccountMenu } from "./Auth";
import { Box } from "./Box";
import { api, type CategoryDto, type FeaturedVendorDto } from "./api/client";

/** The left column on every page: account, categories, featured vendors. */
export function Sidebar() {
    const location = useLocation();
    const [categories, setCategories] = useState<CategoryDto[]>([]);
    const [featured, setFeatured] = useState<FeaturedVendorDto[]>([]);

    // Load again on every page change, so an admin's new category shows up straight away.
    useEffect(() => {
        api.categories.categoriesList().then((r) => setCategories(r.data)).catch(() => setCategories([]));
        api.vendors.vendorsFeaturedList().then((r) => setFeatured(r.data)).catch(() => setFeatured([]));
    }, [location.pathname]);

    return (
        <aside className="sidebar">
            <Box title="Account">
                <AccountMenu />
            </Box>

            <Box title="Categories">
                <ul className="plain">
                    <li><Link to="/">All products</Link></li>
                    {categories.map((c) => (
                        <li key={c.id}>
                            <Link to={`/?category=${c.id}`}>{c.name}</Link>
                        </li>
                    ))}
                </ul>
            </Box>

            <Box title="Top sellers">
                {featured.length === 0 && <p className="muted">None yet.</p>}
                <ul className="plain">
                    {featured.map((f) => (
                        <li key={f.vendorId}>
                            <Link to={`/vendors/${f.vendorId}`}>{f.vendorName}</Link>{" "}
                            <span className="muted">({f.sales} sales)</span>
                        </li>
                    ))}
                </ul>
            </Box>
        </aside>
    );
}
