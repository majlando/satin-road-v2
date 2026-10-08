import { Link, NavLink, Route, Routes } from "react-router";
import { AuthProvider } from "./Auth";
import { Box } from "./Box";
import { Sidebar } from "./Sidebar";
import { Browse } from "./pages/Browse";
import { ListingPage } from "./pages/ListingPage";
import { Vendor } from "./pages/Vendor";
import { Admin } from "./pages/Admin";
import { Sell } from "./pages/Sell";
import { Login } from "./pages/Login";
import { About } from "./pages/About";

/** The frame around every page: header, nav bar, sidebar, the current page, footer. */
export function App() {
    return (
        <AuthProvider>
            <div className="page">
                <header className="header">
                    <h1>
                        <Link to="/">Satin Road a.k.a. Golden Route</Link>
                    </h1>
                    <p>The smoothest trade route on the web</p>
                </header>

                <nav className="navbar">
                    <NavLink to="/" end>Market</NavLink>
                    <NavLink to="/sell">Sell</NavLink>
                    <NavLink to="/admin">Admin</NavLink>
                    <NavLink to="/about">About</NavLink>
                </nav>

                <div className="columns">
                    <Sidebar />
                    <main>
                        <Routes>
                            <Route path="/" element={<Browse />} />
                            <Route path="/listings/:id" element={<ListingPage />} />
                            <Route path="/vendors/:id" element={<Vendor />} />
                            <Route path="/sell" element={<Sell />} />
                            <Route path="/admin" element={<Admin />} />
                            <Route path="/login" element={<Login />} />
                            <Route path="/about" element={<About />} />
                            <Route path="*" element={<NotFound />} />
                        </Routes>
                    </main>
                </div>

                <footer className="footer">
                    <p>
                        <Link to="/">Market</Link> | <Link to="/sell">Sell</Link> |{" "}
                        <Link to="/about">About</Link>
                    </p>
                    <p>This is a school assignment.</p>
                    <p>&copy; Satin Road</p>
                </footer>
            </div>
        </AuthProvider>
    );
}

function NotFound() {
    return (
        <Box title="404 - Page not found">
            <p>There is nothing at this address.</p>
            <p>
                <Link to="/">Back to the market</Link>
            </p>
        </Box>
    );
}
