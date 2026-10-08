import { Link, NavLink, Route, Routes } from "react-router";
import { AccountMenu, AuthProvider } from "./Auth";
import { Browse } from "./pages/Browse";
import { ListingPage } from "./pages/ListingPage";
import { Admin } from "./pages/Admin";
import { Sell } from "./pages/Sell";
import { Login } from "./pages/Login";

/** The frame around every page: header, the current page, disclaimer. */
export function App() {
    return (
        <AuthProvider>
            <div className="shell">
                <header className="masthead">
                    <h1>
                        <Link to="/" className="home">Satin Road</Link>
                    </h1>
                    <nav>
                        <NavLink to="/" end>Browse</NavLink>
                        <NavLink to="/sell">Sell</NavLink>
                        <NavLink to="/admin">Admin</NavLink>
                    </nav>
                    <AccountMenu />
                </header>

                <main>
                    <Routes>
                        <Route path="/" element={<Browse />} />
                        <Route path="/listings/:id" element={<ListingPage />} />
                        <Route path="/sell" element={<Sell />} />
                        <Route path="/admin" element={<Admin />} />
                        <Route path="/login" element={<Login />} />
                        <Route path="*" element={<NotFound />} />
                    </Routes>
                </main>

                <p className="disclaimer">
                    Satire. Every vendor, product and order in here is made up for a school assignment.
                </p>
            </div>
        </AuthProvider>
    );
}

function NotFound() {
    return (
        <section className="card">
            <h2>Nothing here</h2>
            <p>
                <Link to="/">Back to browsing</Link>
            </p>
        </section>
    );
}