import { createContext, useContext, useEffect, useState, type ReactNode } from "react";
import { Link, useLocation } from "react-router";
import { api, type UserDto } from "./api/client";

/**
 * Who is logged in. The API keeps that in an HttpOnly cookie the page cannot
 * read, so on load we ask /api/users/me. Any component can read it with useAuth().
 */
type Auth = {
  user: UserDto | null;
  loading: boolean;
  login: (username: string, password: string) => Promise<void>;
  register: (username: string, password: string) => Promise<void>;
  logout: () => Promise<void>;
  refresh: () => Promise<void>;
};

const AuthContext = createContext<Auth | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<UserDto | null>(null);
  const [loading, setLoading] = useState(true);

  async function refresh() {
    try {
      setUser((await api.users.usersMeList()).data);
    } catch {
      setUser(null); // 401: nobody is logged in (or the API is not reachable)
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    refresh();
  }, []);

  async function login(username: string, password: string) {
    setUser((await api.auth.authLoginCreate({ username, password })).data);
  }

  async function register(username: string, password: string) {
    setUser((await api.users.usersCreate({ username, password })).data);
  }

  async function logout() {
    await api.auth.authLogoutCreate();
    setUser(null);
  }

  return (
      <AuthContext.Provider value={{ user, loading, login, register, logout, refresh }}>
        {children}
      </AuthContext.Provider>
  );
}

export function useAuth(): Auth {
  const value = useContext(AuthContext);
  if (!value) throw new Error("useAuth must be used inside <AuthProvider>");
  return value;
}

/** A "Log in" link that brings you back to the page you were on. */
export function LoginLink({ children = "Log in" }: { children?: ReactNode }) {
  const location = useLocation();
  return (
      <Link to="/login" state={{ from: location.pathname }}>
        {children}
      </Link>
  );
}

/** The sidebar's account box: who you are and Log out, or a Log in link. */
export function AccountMenu() {
  const { user, loading, logout } = useAuth();

  if (loading) return <p className="muted">Loading…</p>;

  if (!user) {
    return (
        <p>
          Welcome, stranger. 
          <br />
          <LoginLink>Log in or register</LoginLink>
        </p>
    );
  }

  return (
      <p>
        Logged in as <strong>{user.username}</strong>
        {user.role === "Admin" && <span className="tag">[admin]</span>}
        {user.isSeized && <span className="tag">[seized]</span>}
        <br />
        <button type="button" className="linklike" onClick={logout}>
          Log out
        </button>
      </p>
  );
}
