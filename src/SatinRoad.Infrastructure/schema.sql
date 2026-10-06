-- Satin Road schema. Applied on every startup by Schema.Ensure.
--
-- There are no migrations: to change a table, edit this file and delete the .db
-- (or `docker compose down -v`). For a demo app that is reset before every
-- presentation, that is simpler than a migration tool and loses nothing.
--
-- Money is stored in whole cents as INTEGER, because SQLite has no decimal type
-- and floating-point money invites rounding bugs.

CREATE TABLE IF NOT EXISTS users (
                                     id        INTEGER PRIMARY KEY AUTOINCREMENT,
                                     username  TEXT    NOT NULL UNIQUE COLLATE NOCASE,
                                     role      TEXT    NOT NULL DEFAULT 'User' CHECK (role IN ('User', 'Admin')),
    is_seized INTEGER NOT NULL DEFAULT 0
    );

CREATE TABLE IF NOT EXISTS categories (
                                          id   INTEGER PRIMARY KEY AUTOINCREMENT,
                                          name TEXT    NOT NULL UNIQUE COLLATE NOCASE
);

CREATE TABLE IF NOT EXISTS listings (
                                        id          INTEGER PRIMARY KEY AUTOINCREMENT,
                                        vendor_id   INTEGER NOT NULL REFERENCES users(id),
    category_id INTEGER NOT NULL REFERENCES categories(id),
    title       TEXT    NOT NULL,
    description TEXT    NOT NULL DEFAULT '',
    price_cents INTEGER NOT NULL CHECK (price_cents > 0),
    stock       INTEGER NOT NULL CHECK (stock >= 0),
    is_removed  INTEGER NOT NULL DEFAULT 0
    );

CREATE TABLE IF NOT EXISTS orders (
                                      id              INTEGER PRIMARY KEY AUTOINCREMENT,
                                      buyer_id        INTEGER NOT NULL REFERENCES users(id),
    vendor_id       INTEGER NOT NULL REFERENCES users(id),
    listing_id      INTEGER NOT NULL REFERENCES listings(id),
    quantity        INTEGER NOT NULL CHECK (quantity > 0),
    -- The price charged at the time, not recomputed from the listing: listings
    -- change price, and an order is a record of what happened.
    subtotal_cents  INTEGER NOT NULL CHECK (subtotal_cents >= 0),
    discount_cents  INTEGER NOT NULL CHECK (discount_cents >= 0),
    total_cents     INTEGER NOT NULL CHECK (total_cents >= 0),
    status          TEXT    NOT NULL CHECK (status IN ('Completed', 'Seized'))
    );

-- The loyalty rule counts one buyer's completed orders with one vendor, and the
-- featured rule counts one vendor's completed orders. Both run on the purchase
-- path, so both get an index that answers them without touching the table.
CREATE INDEX IF NOT EXISTS ix_orders_buyer_vendor ON orders (buyer_id, vendor_id, status);
CREATE INDEX IF NOT EXISTS ix_orders_vendor       ON orders (vendor_id, status);

-- Browsing filters on these three columns on every page load.
CREATE INDEX IF NOT EXISTS ix_listings_browse ON listings (is_removed, category_id, vendor_id);