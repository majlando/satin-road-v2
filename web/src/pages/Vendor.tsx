import { useEffect, useState } from "react";
import { Link, useParams } from "react-router";
import { Box } from "../Box";
import { api, errorText, type VendorDto } from "../api/client";
import { ListingsTable } from "./Browse";

/** One vendor's shop: their sales and everything they list, sold out or not. */
export function Vendor() {
    const id = Number(useParams().id);
    const [vendor, setVendor] = useState<VendorDto | null>(null);
    const [error, setError] = useState("");

    useEffect(() => {
        setVendor(null);
        setError("");
        api.vendors.vendorsDetail({ id }).then((r) => setVendor(r.data)).catch((e) => setError(errorText(e)));
    }, [id]);

    return (
        <Box title={`Seller: ${vendor?.name ?? "…"}`}>
            <p>
                <Link to="/">&laquo; Back to the market</Link>
            </p>
            {error && <p className="error">{error}</p>}
            {!vendor && !error && <p className="muted">Loading…</p>}
            {vendor && (
                <>
                    {vendor.isSeized ? (
                        <p className="error">This shop was shut down by the FBI.</p>
                    ) : (
                        <p>
                            {vendor.isFeatured && <><span className="tag">[Top seller]</span>{" "}</>}
                            {vendor.sales} {vendor.sales === 1 ? "sale" : "sales"}.
                        </p>
                    )}
                    {!vendor.isSeized && vendor.listings.length === 0 && (
                        <p className="muted">This seller has nothing for sale.</p>
                    )}
                    {vendor.listings.length > 0 && <ListingsTable listings={vendor.listings} />}
                </>
            )}
        </Box>
    );
}
