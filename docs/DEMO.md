## How to demo Satin Road

A walk-through that shows every feature in about ten minutes.

### Before you start

1. Start the app with Docker Compose (see the README) and open <http://localhost:8080>.
2. If you have run it before, reset the data first so the demo users and orders are fresh.
3. Every user's password is `password`.

The demo data is set up so each rule can be shown straight away:

| User | Why they matter |
|---|---|
| `admin` | Can manage categories |
| `vitocorleone` | Has 101 sales, so he is a featured top seller |
| `tonymontana` | Has 11 orders at `walterwhite`, so he gets the loyal customer discount there |
| `jacksparrow` | A fresh user, for the FBI raid |

### 1. Look around the market

- Show the **Market** page: every product, with featured sellers' products at the top.
- Click a category in the sidebar to filter the products.
- Point out **Top sellers** in the sidebar: `vitocorleone` is there with 101 sales.
- Open a product, then click the seller's name to see their shop.
- Open **About** for the rules of the site.

### 2. Manage categories (admin)

1. Log in as `admin`.
2. Go to **Admin**.
3. Add a new category. It shows up in the sidebar straight away.
4. Rename a category by editing its name and clicking elsewhere.
5. Delete the new category.
6. Log out, log in as any other user and open **Admin**: they are not allowed in.

### 3. Sell a product

1. Log in as any user, or create a new account from the login page.
2. Go to **Sell**.
3. Add a product: pick a category, give it a title, price and stock.
4. Change the stock in the table and click elsewhere to save it.
5. Edit the product, then show it on the **Market** page.

### 4. Buy a product and get the discount

1. Log in as `tonymontana`.
2. Open a product sold by `walterwhite`, such as the blue rock candy.
3. Choose a quantity and press **Buy now**.
4. The order confirmation shows the 20% loyal customer discount.
5. Buy something from another seller to show there is no discount there.

### 5. The FBI raid

Each purchase has a 1% chance of being an FBI raid, which is too rare to show live. For the demo:

1. Set the FBI raid chance to 1 (that is, 100%) in the Docker Compose settings and restart the app.
2. Log in as `jacksparrow` and buy any product.
3. The FBI notice appears: the seller has been shut down.
4. Show that all of that seller's products are gone from the market.
5. Log in as the raided seller and open **Sell**: their shop is closed for good.
6. Set the raid chance back to 0.01 (1%) afterwards.

### 6. Wrap up

- Show the API documentation at <http://localhost:8080/swagger>.
- Show the Lighthouse scores in the README.
- Show the tests passing.
- Reset the data if you want to run the demo again.
