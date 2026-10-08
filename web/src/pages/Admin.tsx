import { useEffect, useState, type FormEvent } from "react";
import { LoginLink, useAuth } from "../Auth";
import { Box } from "../Box";
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
            <Box title="Admin: categories">
                <p>
                    Only an admin can manage categories.{" "}
                    {user ? "You are not one." : <><LoginLink /> as an admin.</>}
                </p>
            </Box>
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
        <Box title="Admin: categories">
            <div className="table-wrap">
                <table>
                    <thead>
                    <tr>
                        <th>Name</th>
                        <th></th>
                    </tr>
                    </thead>
                    <tbody>
                    {categories.map((c) => (
                        <tr key={c.id}>
                            <td>
                                <input
                                    defaultValue={c.name}
                                    aria-label={`Name of ${c.name}`}
                                    onBlur={async (e) => {
                                        const input = e.target;
                                        if (input.value === c.name) return;
                                        const renamed = await run(() => api.categories.categoriesUpdate({ id: c.id }, { name: input.value }));
                                        if (!renamed) input.value = c.name; // refused: show the saved name again
                                    }}
                                />
                            </td>
                            <td>
                                <button onClick={() => run(() => api.categories.categoriesDelete({ id: c.id }))}>Delete</button>
                            </td>
                        </tr>
                    ))}
                    </tbody>
                </table>
            </div>
            <p className="muted">Edit a name and click elsewhere to save it.</p>

            <form onSubmit={add}>
                <p>
                    <label>
                        New category:{" "}
                        <input value={newName} onChange={(e) => setNewName(e.target.value)} />
                    </label>{" "}
                    <button type="submit">Add</button>
                </p>
            </form>

            {error && <p className="error">{error}</p>}
        </Box>
    );
}
