import { createContext, useContext, useEffect, useState, type FormEvent, type ReactNode } from "react";
import { api, errorText, getActingAs, setActingAs, type UserDto } from "./api/client";

/**
 * "Acting as": which user the browser pretends to be. Any component can read
 * it with useActingAs(), and the header dropdown changes it.
 */
type ActingAs = {
  user: UserDto | null;
  users: UserDto[];
  choose: (userId: number | null) => void;
  reloadUsers: () => Promise<void>;
};

const ActingAsContext = createContext<ActingAs | null>(null);

export function ActingAsProvider({ children }: { children: ReactNode }) {
  const [users, setUsers] = useState<UserDto[]>([]);
  const [userId, setUserId] = useState<number | null>(getActingAs());

  async function reloadUsers() {
    try {
      setUsers((await api.users.usersList()).data);
    } catch {
      setUsers([]); // API not reachable: the dropdown is simply empty
    }
  }

  useEffect(() => {
    reloadUsers();
  }, []);

  function choose(id: number | null) {
    setActingAs(id);
    setUserId(id);
  }

  const user = users.find((u) => u.id === userId) ?? null;

  return (
      <ActingAsContext.Provider value={{ user, users, choose, reloadUsers }}>
        {children}
      </ActingAsContext.Provider>
  );
}

export function useActingAs(): ActingAs {
  const value = useContext(ActingAsContext);
  if (!value) throw new Error("useActingAs must be used inside <ActingAsProvider>");
  return value;
}

/** The dropdown in the header, with a "+ New user" option. */
export function ActingAsPicker() {
  const { user, users, choose, reloadUsers } = useActingAs();
  const [adding, setAdding] = useState(false);
  const [name, setName] = useState("");
  const [error, setError] = useState("");

  async function create(event: FormEvent) {
    event.preventDefault();
    try {
      const created = (await api.users.usersCreate({ username: name })).data;
      await reloadUsers();
      choose(created.id);
      setAdding(false);
      setName("");
      setError("");
    } catch (e) {
      setError(errorText(e));
    }
  }

  if (adding) {
    return (
        <form className="acting-as" onSubmit={create}>
          <input value={name} onChange={(e) => setName(e.target.value)} placeholder="New username" autoFocus />
          <button type="submit">Create</button>
          <button type="button" onClick={() => setAdding(false)}>Cancel</button>
          {error && <span className="error">{error}</span>}
        </form>
    );
  }

  return (
      <label className="acting-as">
        Acting as{" "}
        <select
            value={user?.id ?? ""}
            onChange={(e) => {
              if (e.target.value === "new") setAdding(true);
              else choose(e.target.value ? Number(e.target.value) : null);
            }}
        >
          <option value="">nobody</option>
          {users.map((u) => (
              <option key={u.id} value={u.id}>
                {u.username}
                {u.role === "Admin" ? " (admin)" : ""}
                {u.isSeized ? " (seized)" : ""}
              </option>
          ))}
          <option value="new">+ New user</option>
        </select>
      </label>
  );
}