import { Link } from "react-router";
import { Box } from "../Box";

/** What the site is and how its rules work. */
export function About() {
    return (
        <>
            <Box title="About Satin Road">
                <p>
                    Satin Road is a <strong>fictional</strong> marketplace, made as satire for a school
                    assignment. Nothing here is real: not the vendors, not the products and not the orders.
                </p>
            </Box>

            <Box title="Frequently asked questions">
                <dl className="faq">
                    <dt>How do I buy something?</dt>
                    <dd>
                        <Link to="/login">Log in</Link>, open a listing, choose a quantity and press
                        &quot;Buy now&quot;.
                    </dd>

                    <dt>How do I sell something?</dt>
                    <dd>
                        Go to <Link to="/sell">Sell</Link>. There you can add listings, change them and keep
                        your stock up to date.
                    </dd>

                    <dt>Is there a loyalty discount?</dt>
                    <dd>Yes. Once you have placed more than 10 orders with one vendor, every order after that from them is 20% off.</dd>

                    <dt>What are featured vendors?</dt>
                    <dd>Vendors with more than 100 sales. Their listings are shown first.</dd>

                    <dt>Is it safe?</dt>
                    <dd>
                        Not quite. Every purchase has a 1% chance that the buyer is the FBI. If so, the vendor is
                        shut down for good and all of their listings disappear.
                    </dd>

                    <dt>Can I try it out?</dt>
                    <dd>
                        Log in as <code>admin</code> with the password <code>password</code>. The demo users
                        (<code>shadypete</code>, <code>grandmasoap</code>, <code>loyalbuyer</code>, ...) have the
                        same password. You can also create your own account.
                    </dd>
                </dl>
            </Box>
        </>
    );
}
