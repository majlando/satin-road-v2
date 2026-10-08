/**
 * The one place the web app talks to the API from. Api.ts next to this file is
 * generated from the API's OpenAPI document by swagger-typescript-api (run
 * `bun run gen:api`); this file only adds what the generator cannot know.
 */
import { Api } from "./Api";

export type { CategoryDto, FeaturedVendorDto, ListingDto, ListingRequest, OrderDto, UserDto, VendorDto } from "./Api";

// ---- the client -------------------------------------------------------------

export const api = new Api({
  // The generated client points at http://localhost:5080. An empty base URL
  // sends every request to the page's own address instead, where the dev
  // server (or nginx in Docker) passes it on to the API.
  // Same origin also means the browser sends the login cookie by itself.
  baseUrl: "",
});

// ---- small helpers ----------------------------------------------------------

const dollars = new Intl.NumberFormat("en-US", { style: "currency", currency: "USD" });

/** 2500 cents → "$25.00", 100000000 → "$1,000,000.00". Money is always whole cents until it is shown. */
export function money(cents: number): string {
  return dollars.format(cents / 100);
}

/**
 * The text of an error. When the API refuses something, the generated client
 * throws the response, with the API's explanation ({ status, title }) in `error`.
 */
export function errorText(error: unknown): string {
  const problem = (error as { error?: { title?: string } } | null)?.error;
  if (problem?.title) return problem.title;
  if (error instanceof Response) return `Request failed (${error.status})`;
  return error instanceof Error ? error.message : String(error);
}