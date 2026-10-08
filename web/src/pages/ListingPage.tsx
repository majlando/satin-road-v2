import { useEffect, useState } from "react";
import { Link, useParams } from "react-router";
import { LoginLink, useAuth } from "../Auth";
import { Box } from "../Box";
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
        <>
            <Box title={listing?.title ?? "Listing"}>
                <p>
                    <Link to="/">&laquo; Back to browsing</Link>
                </p>

                {!listing && !error && <p className="muted">Loading…</p>}

                {listing && (
                    <>
                        <table className="details">
                            <tbody>
                            <tr>
                                <th>Price</th>
                                <td className="price">{money(listing.priceCents)}</td>
                            </tr>
                            <tr>
                                <th>Seller</th>
                                <td>
                                    <Link to={`/vendors/${listing.vendorId}`}>{listing.vendorName}</Link>
                                </td>
                            </tr>
                            <tr>
                                <th>Category</th>
                                <td>
                                    <Link to={`/?category=${listing.categoryId}`}>{listing.categoryName}</Link>
                                </td>
                            </tr>
                            <tr>
                                <th>In stock</th>
                                <td>{listing.stock > 0 ? listing.stock : "Sold out"}</td>
                            </tr>
                            </tbody>
                        </table>
                        <h3>Description</h3>
                        <p>{listing.description}</p>
                    </>
                )}
                {error && !listing && <p className="error">{error}</p>}
            </Box>

            {listing && (
                <Box title="Buy">
                    {!user && <p><LoginLink /> to buy.</p>}
                    {user && listing.stock === 0 && <p className="muted">Sold out.</p>}
                    {user && listing.stock > 0 && (
                        <p>
                            <label>
                                Quantity:{" "}
                                <input
                                    type="number"
                                    min={1}
                                    max={listing.stock}
                                    value={quantity}
                                    onChange={(e) => setQuantity(Number(e.target.value))}
                                    size={4}
                                />
                            </label>{" "}
                            <button onClick={buy}>Buy now</button>
                        </p>
                    )}
                    {order && (
                        <p className="success">
                            Bought {order.quantity} for {money(order.totalCents)}.
                            {order.discountCents > 0 && ` Loyalty discount: ${money(order.discountCents)} off!`}
                        </p>
                    )}
                    {error && <p className="error">{error}</p>}
                </Box>
            )}
        </>
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
