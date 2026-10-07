import index from "./index.html";
import { serve } from "bun";

/**
 * Dev server. Serves the React app and forwards API calls to the .NET API,
 * so the browser only ever talks to one origin and there is no CORS to configure.
 * In Docker this job is nginx's instead (see web/nginx.conf).
 */
const API = process.env.API_URL ?? "http://localhost:5080";

function proxy(req: Request): Promise<Response> {
  const url = new URL(req.url);
  const headers = new Headers(req.headers);
  headers.delete("host"); // let fetch set it for the upstream

  return fetch(API + url.pathname + url.search, {
    method: req.method,
    headers,
    body: req.body,
    duplex: "half",
  } as RequestInit);
}

const server = serve({
  port: Number(process.env.PORT ?? 3000),
  routes: {
    "/api/*": proxy,
    "/openapi/*": proxy,
    "/health": proxy,
    "/*": index,
  },
  development: {
    hmr: true,
    console: true,
  },
});

console.log(`Satin Road web  → ${server.url}`);
console.log(`proxying /api   → ${API}`);