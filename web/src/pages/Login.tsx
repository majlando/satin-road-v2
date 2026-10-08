import { useState, type FormEvent } from "react";
import { useLocation, useNavigate } from "react-router";
import { useAuth } from "../Auth";
import { Box } from "../Box";
import { errorText } from "../api/client";

/** Log in, or create an account. Afterwards, back to the page that sent you here. */
export function Login() {
    const { login, register } = useAuth();
    const navigate = useNavigate();
    const from = (useLocation().state as { from?: string } | null)?.from ?? "/";
    const [registering, setRegistering] = useState(false);
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");
    const [error, setError] = useState("");

    async function submit(event: FormEvent) {
        event.preventDefault();
        setError("");
        try {
            if (registering) await register(username, password);
            else await login(username, password);
            navigate(from === "/login" ? "/" : from, { replace: true });
        } catch (e) {
            setError(errorText(e));
        }
    }

    return (
        <Box title={registering ? "Create an account" : "Log in"}>
            <form onSubmit={submit}>
                <table className="form-table">
                    <tbody>
                    <tr>
                        <th><label htmlFor="username">Username:</label></th>
                        <td>
                            <input
                                id="username"
                                value={username}
                                onChange={(e) => setUsername(e.target.value)}
                                autoComplete="username"
                                maxLength={32}
                                autoFocus
                                required
                            />
                        </td>
                    </tr>
                    <tr>
                        <th><label htmlFor="password">Password:</label></th>
                        <td>
                            <input
                                id="password"
                                type="password"
                                value={password}
                                onChange={(e) => setPassword(e.target.value)}
                                autoComplete={registering ? "new-password" : "current-password"}
                                minLength={registering ? 8 : undefined}
                                maxLength={128}
                                required
                            />
                        </td>
                    </tr>
                    <tr>
                        <th></th>
                        <td>
                            <button type="submit">{registering ? "Create account" : "Log in"}</button>
                        </td>
                    </tr>
                    </tbody>
                </table>
            </form>
            {error && <p className="error">{error}</p>}
            <p>
                {registering ? "Already have an account? " : "New here? "}
                <button
                    type="button"
                    className="linklike"
                    onClick={() => {
                        setRegistering(!registering);
                        setError("");
                    }}
                >
                    {registering ? "Log in" : "Create an account"}
                </button>
            </p>
        </Box>
    );
}
