import { useEffect, useState } from "react";
import { Link, useParams } from "react-router";
import { LoginLink, useAuth } from "../Auth";
import { api, errorText, money, type ListingDto, type OrderDto } from "../api/client";

/** One listing, with a quantity field and a Buy button. */
export function ListingPage() {
    const id = Number(useParams().id);
    const { user } = useAuth();
    const [listing, setListing] = useState<ListingDto | null>(null);
    const [quantity, setQuantity] = useState(1);
    const [order, setOrder] = useState<OrderDto | null>(null);
    const [error, setError] = useState("");

    useEffect(() => {
        api.listings.listingsDetail({ id }).then((r) => setListing(r.data)).catch((e) => setError(errorText(e)));
    }, [id]);

    async function buy() {
        setError("");
        setOrder(null);
        try {
            const result = (await api.orders.ordersCreate({ listingId: id, quantity })).data;
            setOrder(result);
            if (result.status !== "Seized") {
                setListing((await api.listings.listingsDetail({ id })).data); // show the new stock
            }
        } catch (e) {
            setError(errorText(e));
        }
    }

    if (order?.status === "Seized") return <SeizedScreen />;

    return (
        <section className="card">
            <p>
                <Link to="/">← Back to browsing</Link>
            </p>

            {!listing && !error && <p className="muted">Loading…</p>}

            {listing && (
                <>
                    <h2>{listing.title}</h2>
                    <p className="muted">
                        Sold by {listing.vendorName} · {listing.categoryName}
                    </p>
                    <p>{listing.description}</p>
                    <p className="price">{money(listing.priceCents)}</p>
                    <p className="muted">{listing.stock > 0 ? `${listing.stock} in stock` : "Sold out"}</p>

                    {!user && <p className="muted"><LoginLink /> to buy.</p>}

                    {user && listing.stock > 0 && (
                        <div className="buy">
                            <label>
                                Quantity{" "}
                                <input
                                    type="number"
                                    min={1}
                                    max={listing.stock}
                                    value={quantity}
                                    onChange={(e) => setQuantity(Number(e.target.value))}
                                />
                            </label>
                            <button onClick={buy}>Buy</button>
                        </div>
                    )}
                </>
            )}

            {order && (
                <p className="success">
                    Bought {order.quantity} for {money(order.totalCents)}.
                    {order.discountCents > 0 && ` Loyalty discount: ${money(order.discountCents)} off!`}
                </p>
            )}
            {error && <p className="error">{error}</p>}
        </section>
    );
}

/** Shown after a raided purchase. The seal is invented, not the real FBI logo. */
function SeizedScreen() {
    return (
        <div className="seized" role="alert">
            <div className="seal" aria-hidden="true">
                ★<br />
                FEDERAL
                <br />
                SATIRE
                <br />
                BUREAU
            </div>
            <h2>This marketplace listing has been seized</h2>
            <p>
                The buyer was the FBI. The vendor has been shut down for good and all of their products
                removed.
            </p>
            <Link to="/">Back to browsing</Link>
        </div>
    );
}