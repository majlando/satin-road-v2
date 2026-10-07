import { Link, Route, Routes } from "react-router";
import { ActingAsPicker, ActingAsProvider } from "./ActingAs";

/** The frame around every page: header, the current page, disclaimer. */
export function App() {
    return (
        <ActingAsProvider>
            <div className="shell">
                <header className="masthead">
                    <h1>
                        Satin Road <span className="muted">— a fictional marketplace</span>
                    </h1>
                    <nav>
                        {/* Parts 15 and 17 add a link here for each page. */}
                    </nav>
                    <ActingAsPicker />
                </header>

                <main>
                    <Routes>
                        {/* Parts 15 to 17 add a route here for each page. */}
                        <Route path="*" element={<NotFound />} />
                    </Routes>
                </main>

                <p className="disclaimer">
                    Satire. Every vendor, product and order in here is made up for a school assignment.
                </p>
            </div>
        </ActingAsProvider>
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