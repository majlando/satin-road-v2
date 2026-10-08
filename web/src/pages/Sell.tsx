import { useEffect, useState, type FormEvent } from "react";
import { Link } from "react-router";
import { LoginLink, useAuth } from "../Auth";
import { Box } from "../Box";
import { api, errorText, money, type CategoryDto, type ListingDto, type ListingRequest } from "../api/client";

const EMPTY_FORM = { categoryId: 0, title: "", description: "", price: "", stock: "1" };

/** The logged-in user's own listings: add, edit, change stock, delete. */
export function Sell() {
    const { user } = useAuth();
    const [listings, setListings] = useState<ListingDto[]>([]);
    const [categories, setCategories] = useState<CategoryDto[]>([]);
    const [form, setForm] = useState(EMPTY_FORM);
    const [editingId, setEditingId] = useState<number | null>(null);
    const [error, setError] = useState("");

    async function reload() {
        try {
            setListings((await api.myListings.myListingsList()).data);
        } catch (e) {
            setError(errorText(e));
        }
    }

    // Reload whenever a different user logs in.
    useEffect(() => {
        if (!user) return;
        reload();
        api.categories.categoriesList().then((r) => setCategories(r.data)).catch((e) => setError(errorText(e)));
    }, [user?.id]);

    if (!user) {
        return (
            <Box title="Sell">
                <p><LoginLink /> to manage your products.</p>
            </Box>
        );
    }

    if (user.isSeized) {
        return (
            <Box title="Sell">
                <p className="error">Your shop was shut down by the FBI. Permanently.</p>
            </Box>
        );
    }

    // Run an API call, then reload the table. Shows any error, and says whether it worked.
    async function run(action: () => Promise<unknown>): Promise<boolean> {
        setError("");
        try {
            await action();
            await reload();
            return true;
        } catch (e) {
            setError(errorText(e));
            return false;
        }
    }

    async function save(event: FormEvent) {
        event.preventDefault();
        // The form holds "9.99"; the API wants 999 cents.
        const input: ListingRequest = {
            categoryId: Number(form.categoryId),
            title: form.title,
            description: form.description,
            priceCents: Math.round(Number(form.price) * 100),
            stock: Number(form.stock),
        };
        const saved = await run(() =>
            editingId === null
                ? api.myListings.myListingsCreate(input)
                : api.myListings.myListingsUpdate({ id: editingId }, input),
        );
        // Keep what was typed if the API said no, so it can be corrected.
        if (saved) {
            setForm(EMPTY_FORM);
            setEditingId(null);
        }
    }

    function edit(l: ListingDto) {
        setEditingId(l.id);
        setForm({
            categoryId: l.categoryId,
            title: l.title,
            description: l.description,
            price: (l.priceCents / 100).toFixed(2),
            stock: String(l.stock),
        });
    }

    return (
        <>
            <Box title="Your products">
                {listings.length === 0 && <p className="muted">Nothing for sale yet. Time to smuggle something in.</p>}
                {listings.length > 0 && (
                    <div className="table-wrap">
                        <table>
                            <thead>
                            <tr>
                                <th>Title</th>
                                <th>Category</th>
                                <th className="num">Price</th>
                                <th>Stock</th>
                                <th></th>
                            </tr>
                            </thead>
                            <tbody>
                            {listings.map((l) => (
                                <tr key={l.id}>
                                    <td>
                                        <Link to={`/listings/${l.id}`}>{l.title}</Link>
                                    </td>
                                    <td>{l.categoryName}</td>
                                    <td className="num">{money(l.priceCents)}</td>
                                    <td>
                                        <input
                                            // Keyed on the stock, so a reload shows the saved number.
                                            key={l.stock}
                                            type="number"
                                            min={0}
                                            max={1_000_000}
                                            defaultValue={l.stock}
                                            aria-label={`Stock for ${l.title}`}
                                            size={4}
                                            onBlur={async (e) => {
                                                const input = e.target;
                                                const stock = Number(input.value);
                                                if (stock === l.stock) return;
                                                const saved = await run(() => api.myListings.myListingsStockPartialUpdate({ id: l.id }, { stock }));
                                                if (!saved) input.value = String(l.stock); // refused: show the saved stock again
                                            }}
                                        />
                                    </td>
                                    <td>
                                        <button onClick={() => edit(l)}>Edit</button>{" "}
                                        <button
                                            onClick={async () => {
                                                const deleted = await run(() => api.myListings.myListingsDelete({ id: l.id }));
                                                if (deleted && editingId === l.id) {
                                                    setEditingId(null); // nothing left to edit
                                                    setForm(EMPTY_FORM);
                                                }
                                            }}
                                        >
                                            Delete
                                        </button>
                                    </td>
                                </tr>
                            ))}
                            </tbody>
                        </table>
                    </div>
                )}
                <p className="muted">Change a stock number and click elsewhere to save it.</p>
            </Box>

            <Box title={editingId === null ? "Add a product" : "Edit product"}>
                <form onSubmit={save}>
                    <table className="form-table">
                        <tbody>
                        <tr>
                            <th><label htmlFor="category">Category:</label></th>
                            <td>
                                <select
                                    id="category"
                                    value={form.categoryId}
                                    onChange={(e) => setForm({ ...form, categoryId: Number(e.target.value) })}
                                >
                                    <option value={0}>Choose…</option>
                                    {categories.map((c) => (
                                        <option key={c.id} value={c.id}>
                                            {c.name}
                                        </option>
                                    ))}
                                </select>
                            </td>
                        </tr>
                        <tr>
                            <th><label htmlFor="title">Title:</label></th>
                            <td>
                                <input
                                    id="title"
                                    size={40}
                                    maxLength={100}
                                    required
                                    value={form.title}
                                    onChange={(e) => setForm({ ...form, title: e.target.value })}
                                />
                            </td>
                        </tr>
                        <tr>
                            <th><label htmlFor="description">Description:</label></th>
                            <td>
                                <textarea
                                    id="description"
                                    rows={4}
                                    cols={40}
                                    maxLength={2000}
                                    value={form.description}
                                    onChange={(e) => setForm({ ...form, description: e.target.value })}
                                />
                            </td>
                        </tr>
                        <tr>
                            <th><label htmlFor="price">Price ($):</label></th>
                            <td>
                                <input
                                    id="price"
                                    type="number"
                                    step="0.01"
                                    min="0.01"
                                    max="1000000"
                                    required
                                    size={8}
                                    value={form.price}
                                    onChange={(e) => setForm({ ...form, price: e.target.value })}
                                />
                            </td>
                        </tr>
                        <tr>
                            <th><label htmlFor="stock">Stock:</label></th>
                            <td>
                                <input
                                    id="stock"
                                    type="number"
                                    min="0"
                                    max="1000000"
                                    required
                                    size={4}
                                    value={form.stock}
                                    onChange={(e) => setForm({ ...form, stock: e.target.value })}
                                />
                            </td>
                        </tr>
                        <tr>
                            <th></th>
                            <td>
                                <button type="submit">{editingId === null ? "Add product" : "Save changes"}</button>{" "}
                                {editingId !== null && (
                                    <button
                                        type="button"
                                        onClick={() => {
                                            setEditingId(null);
                                            setForm(EMPTY_FORM);
                                        }}
                                    >
                                        Cancel
                                    </button>
                                )}
                            </td>
                        </tr>
                        </tbody>
                    </table>
                </form>
                {error && <p className="error">{error}</p>}
            </Box>
        </>
    );
}