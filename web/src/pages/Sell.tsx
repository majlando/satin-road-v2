import { useEffect, useState, type FormEvent } from "react";
import { LoginLink, useAuth } from "../Auth";
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
            <section className="card">
                <h2>Sell</h2>
                <p className="muted"><LoginLink /> to manage your listings.</p>
            </section>
        );
    }

    if (user.isSeized) {
        return (
            <section className="card">
                <h2>Sell</h2>
                <p className="error">Your shop was shut down by the FBI. Permanently.</p>
            </section>
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
            <section className="card">
                <h2>Your listings</h2>
                {listings.length === 0 && <p className="muted">You are not selling anything yet.</p>}
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
                                    <td>{l.title}</td>
                                    <td>{l.categoryName}</td>
                                    <td className="num">{money(l.priceCents)}</td>
                                    <td>
                                        <input
                                            type="number"
                                            min={0}
                                            defaultValue={l.stock}
                                            aria-label={`Stock for ${l.title}`}
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
                                        <button onClick={() => run(() => api.myListings.myListingsDelete({ id: l.id }))}>Delete</button>
                                    </td>
                                </tr>
                            ))}
                            </tbody>
                        </table>
                    </div>
                )}
            </section>

            <section className="card">
                <h2>{editingId === null ? "Add a listing" : "Edit listing"}</h2>
                <form className="stack" onSubmit={save}>
                    <label>
                        Category
                        <select
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
                    </label>
                    <label>
                        Title
                        <input value={form.title} onChange={(e) => setForm({ ...form, title: e.target.value })} />
                    </label>
                    <label>
                        Description
                        <textarea
                            rows={4}
                            value={form.description}
                            onChange={(e) => setForm({ ...form, description: e.target.value })}
                        />
                    </label>
                    <label>
                        Price (€)
                        <input
                            type="number"
                            step="0.01"
                            min="0.01"
                            value={form.price}
                            onChange={(e) => setForm({ ...form, price: e.target.value })}
                        />
                    </label>
                    <label>
                        Stock
                        <input
                            type="number"
                            min="0"
                            value={form.stock}
                            onChange={(e) => setForm({ ...form, stock: e.target.value })}
                        />
                    </label>
                    <div>
                        <button type="submit">{editingId === null ? "Add listing" : "Save changes"}</button>{" "}
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
                    </div>
                </form>
                {error && <p className="error">{error}</p>}
            </section>
        </>
    );
}