import { useEffect, useState, type FormEvent } from "react";
import { LoginLink, useAuth } from "../Auth";
import { api, errorText, type CategoryDto } from "../api/client";

/** Category management. Only shown to admins; the API checks as well. */
export function Admin() {
    const { user } = useAuth();
    const [categories, setCategories] = useState<CategoryDto[]>([]);
    const [newName, setNewName] = useState("");
    const [error, setError] = useState("");

    async function reload() {
        try {
            setCategories((await api.categories.categoriesList()).data);
        } catch (e) {
            setError(errorText(e));
        }
    }

    useEffect(() => {
        reload();
    }, []);

    if (user?.role !== "Admin") {
        return (
            <section className="card">
                <h2>Admin: categories</h2>
                <p className="muted">
                    Only an admin can manage categories.{" "}
                    {user ? "You are not one." : <><LoginLink /> as an admin.</>}
                </p>
            </section>
        );
    }

    // Run an API call, then reload the list. Shows any error, and says whether it worked.
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

    async function add(event: FormEvent) {
        event.preventDefault();
        if (await run(() => api.categories.categoriesCreate({ name: newName }))) setNewName("");
    }

    return (
        <section className="card">
            <h2>Admin: categories</h2>

            <ul className="category-list">
                {categories.map((c) => (
                    <li key={c.id}>
                        <input
                            defaultValue={c.name}
                            aria-label={`Name of ${c.name}`}
                            onBlur={async (e) => {
                                const input = e.target;
                                if (input.value === c.name) return;
                                const renamed = await run(() => api.categories.categoriesUpdate({ id: c.id }, { name: input.value }));
                                if (!renamed) input.value = c.name; // refused: show the saved name again
                            }}
                        />{" "}
                        <button onClick={() => run(() => api.categories.categoriesDelete({ id: c.id }))}>Delete</button>
                    </li>
                ))}
            </ul>

            <form onSubmit={add}>
                <input value={newName} onChange={(e) => setNewName(e.target.value)} placeholder="New category" />{" "}
                <button type="submit">Add</button>
            </form>

            {error && <p className="error">{error}</p>}
        </section>
    );
}