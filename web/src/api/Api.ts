/* eslint-disable */
/* tslint:disable */
// @ts-nocheck
/*
 * ---------------------------------------------------------------
 * ## THIS FILE WAS GENERATED VIA SWAGGER-TYPESCRIPT-API        ##
 * ##                                                           ##
 * ## AUTHOR: acacode                                           ##
 * ## SOURCE: https://github.com/acacode/swagger-typescript-api ##
 * ---------------------------------------------------------------
 */

export interface CategoryDto {
  /** @format int32 */
  id: number;
  name: string;
}

export interface CategoryRequest {
  name: string;
}

export interface CreateUserRequest {
  username: string;
  password: string;
}

export interface FeaturedVendorDto {
  /** @format int32 */
  vendorId: number;
  vendorName: string;
  /** @format int32 */
  sales: number;
}

export interface ListingDto {
  /** @format int32 */
  id: number;
  /** @format int32 */
  vendorId: number;
  vendorName: string;
  vendorFeatured: boolean;
  /** @format int32 */
  categoryId: number;
  categoryName: string;
  title: string;
  description: string;
  /** @format int64 */
  priceCents: number;
  /** @format int32 */
  stock: number;
}

export interface ListingRequest {
  /** @format int32 */
  categoryId: number;
  title: string;
  description: string;
  /** @format int64 */
  priceCents: number;
  /** @format int32 */
  stock: number;
}

export interface LoginRequest {
  username: string;
  password: string;
}

export interface OrderDto {
  /** @format int32 */
  id: number;
  /** @format int32 */
  listingId: number;
  /** @format int32 */
  quantity: number;
  /** @format int64 */
  subtotalCents: number;
  /** @format int64 */
  discountCents: number;
  /** @format int64 */
  totalCents: number;
  status: string;
}

export interface OrderRequest {
  /** @format int32 */
  listingId: number;
  /** @format int32 */
  quantity: number;
}

export interface StockRequest {
  /** @format int32 */
  stock: number;
}

export interface UserDto {
  /** @format int32 */
  id: number;
  username: string;
  role: string;
  isSeized: boolean;
}

export interface VendorDto {
  /** @format int32 */
  id: number;
  name: string;
  isSeized: boolean;
  /** @format int32 */
  sales: number;
  isFeatured: boolean;
  listings: ListingDto[];
}

export interface CategoriesUpdateParams {
  /** @format int32 */
  id: number;
}

export interface CategoriesDeleteParams {
  /** @format int32 */
  id: number;
}

export interface ListingsListParams {
  /** @format int32 */
  categoryId?: number;
}

export interface ListingsDetailParams {
  /** @format int32 */
  id: number;
}

export interface MyListingsUpdateParams {
  /** @format int32 */
  id: number;
}

export interface MyListingsDeleteParams {
  /** @format int32 */
  id: number;
}

export interface MyListingsStockPartialUpdateParams {
  /** @format int32 */
  id: number;
}

export interface VendorsDetailParams {
  /** @format int32 */
  id: number;
}

export type QueryParamsType = Record<string | number, any>;
export type ResponseFormat = keyof Omit<Body, "body" | "bodyUsed">;

export interface FullRequestParams extends Omit<RequestInit, "body"> {
  /** set parameter to `true` for call `securityWorker` for this request */
  secure?: boolean;
  /** request path */
  path: string;
  /** content type of request body */
  type?: ContentType;
  /** query params */
  query?: QueryParamsType;
  /** format of response (i.e. response.json() -> format: "json") */
  format?: ResponseFormat;
  /** request body */
  body?: unknown;
  /** base url */
  baseUrl?: string;
  /** request cancellation token */
  cancelToken?: CancelToken;
}

export type RequestParams = Omit<
  FullRequestParams,
  "body" | "method" | "query" | "path"
>;

export interface ApiConfig<SecurityDataType = unknown> {
  baseUrl?: string;
  baseApiParams?: Omit<RequestParams, "baseUrl" | "cancelToken" | "signal">;
  securityWorker?: (
    securityData: SecurityDataType | null,
  ) => Promise<RequestParams | void> | RequestParams | void;
  customFetch?: typeof fetch;
}

export interface HttpResponse<D extends unknown, E extends unknown = unknown>
  extends Response {
  data: D;
  error: E;
}

type CancelToken = Symbol | string | number;

export enum ContentType {
  Json = "application/json",
  JsonApi = "application/vnd.api+json",
  FormData = "multipart/form-data",
  UrlEncoded = "application/x-www-form-urlencoded",
  Text = "text/plain",
}

export class HttpClient<SecurityDataType = unknown> {
  public baseUrl: string = "http://localhost:5080/";
  private securityData: SecurityDataType | null = null;
  private securityWorker?: ApiConfig<SecurityDataType>["securityWorker"];
  private abortControllers = new Map<CancelToken, AbortController>();
  private customFetch = (...fetchParams: Parameters<typeof fetch>) =>
    fetch(...fetchParams);

  private baseApiParams: RequestParams = {
    credentials: "same-origin",
    headers: {},
    redirect: "follow",
    referrerPolicy: "no-referrer",
  };

  constructor(apiConfig: ApiConfig<SecurityDataType> = {}) {
    Object.assign(this, apiConfig);
  }

  public setSecurityData = (data: SecurityDataType | null) => {
    this.securityData = data;
  };

  protected encodeQueryParam(key: string, value: any) {
    const encodedKey = encodeURIComponent(key);
    return `${encodedKey}=${encodeURIComponent(typeof value === "number" ? value : `${value}`)}`;
  }

  protected addQueryParam(query: QueryParamsType, key: string) {
    return this.encodeQueryParam(key, query[key]);
  }

  protected addArrayQueryParam(query: QueryParamsType, key: string) {
    const value = query[key];
    return value.map((v: any) => this.encodeQueryParam(key, v)).join("&");
  }

  protected toQueryString(rawQuery?: QueryParamsType): string {
    const query = rawQuery || {};
    const keys = Object.keys(query).filter(
      (key) => "undefined" !== typeof query[key],
    );
    return keys
      .map((key) =>
        Array.isArray(query[key])
          ? this.addArrayQueryParam(query, key)
          : this.addQueryParam(query, key),
      )
      .join("&");
  }

  protected addQueryParams(rawQuery?: QueryParamsType): string {
    const queryString = this.toQueryString(rawQuery);
    return queryString ? `?${queryString}` : "";
  }

  private contentFormatters: Record<ContentType, (input: any) => any> = {
    [ContentType.Json]: (input: any) =>
      input !== null && (typeof input === "object" || typeof input === "string")
        ? JSON.stringify(input)
        : input,
    [ContentType.JsonApi]: (input: any) =>
      input !== null && (typeof input === "object" || typeof input === "string")
        ? JSON.stringify(input)
        : input,
    [ContentType.Text]: (input: any) =>
      input !== null && typeof input !== "string"
        ? JSON.stringify(input)
        : input,
    [ContentType.FormData]: (input: any) => {
      if (input instanceof FormData) {
        return input;
      }

      return Object.keys(input || {}).reduce((formData, key) => {
        const property = input[key];
        formData.append(
          key,
          property instanceof Blob
            ? property
            : typeof property === "object" && property !== null
              ? JSON.stringify(property)
              : `${property}`,
        );
        return formData;
      }, new FormData());
    },
    [ContentType.UrlEncoded]: (input: any) => this.toQueryString(input),
  };

  protected mergeRequestParams(
    params1: RequestParams,
    params2?: RequestParams,
  ): RequestParams {
    return {
      ...this.baseApiParams,
      ...params1,
      ...(params2 || {}),
      headers: {
        ...(this.baseApiParams.headers || {}),
        ...(params1.headers || {}),
        ...((params2 && params2.headers) || {}),
      },
    };
  }

  protected createAbortSignal = (
    cancelToken: CancelToken,
  ): AbortSignal | undefined => {
    if (this.abortControllers.has(cancelToken)) {
      const abortController = this.abortControllers.get(cancelToken);
      if (abortController) {
        return abortController.signal;
      }
      return void 0;
    }

    const abortController = new AbortController();
    this.abortControllers.set(cancelToken, abortController);
    return abortController.signal;
  };

  public abortRequest = (cancelToken: CancelToken) => {
    const abortController = this.abortControllers.get(cancelToken);

    if (abortController) {
      abortController.abort();
      this.abortControllers.delete(cancelToken);
    }
  };

  public request = async <T = any, E = any>({
    body,
    secure,
    path,
    type,
    query,
    format,
    baseUrl,
    cancelToken,
    ...params
  }: FullRequestParams): Promise<HttpResponse<T, E>> => {
    const secureParams =
      ((typeof secure === "boolean" ? secure : this.baseApiParams.secure) &&
        this.securityWorker &&
        (await this.securityWorker(this.securityData))) ||
      {};
    const requestParams = this.mergeRequestParams(params, secureParams);
    const queryString = query && this.toQueryString(query);
    const payloadFormatter = this.contentFormatters[type || ContentType.Json];
    const responseFormat = format || requestParams.format;

    return this.customFetch(
      `${baseUrl || this.baseUrl || ""}${path}${queryString ? `?${queryString}` : ""}`,
      {
        ...requestParams,
        headers: {
          ...(requestParams.headers || {}),
          ...(type && type !== ContentType.FormData
            ? { "Content-Type": type }
            : {}),
        },
        signal:
          (cancelToken
            ? this.createAbortSignal(cancelToken)
            : requestParams.signal) || null,
        body:
          typeof body === "undefined" || body === null
            ? null
            : payloadFormatter(body),
      },
    ).then(async (response) => {
      const r = response as HttpResponse<T, E>;
      r.data = null as unknown as T;
      r.error = null as unknown as E;

      const responseToParse = responseFormat ? response.clone() : response;
      const data = !responseFormat
        ? r
        : await responseToParse[responseFormat]()
            .then((data) => {
              if (r.ok) {
                r.data = data;
              } else {
                r.error = data;
              }
              return r;
            })
            .catch((e) => {
              r.error = e;
              return r;
            });

      if (cancelToken) {
        this.abortControllers.delete(cancelToken);
      }

      if (!response.ok) throw data;
      return data;
    });
  };
}

/**
 * @title SatinRoad.Api | v1
 * @version 1.0.0
 * @baseUrl http://localhost:5080/
 */
export class Api<
  SecurityDataType extends unknown,
> extends HttpClient<SecurityDataType> {
  health = {
    /**
     * No description
     *
     * @tags Health
     * @name GetHealth
     * @request GET:/health
     */
    getHealth: (params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/health`,
        method: "GET",
        ...params,
      }),
  };
  auth = {
    /**
     * No description
     *
     * @tags Auth
     * @name AuthLoginCreate
     * @request POST:/api/auth/login
     */
    authLoginCreate: (data: LoginRequest, params: RequestParams = {}) =>
      this.request<UserDto, any>({
        path: `/api/auth/login`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Auth
     * @name AuthLogoutCreate
     * @request POST:/api/auth/logout
     */
    authLogoutCreate: (params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/auth/logout`,
        method: "POST",
        ...params,
      }),
  };
  categories = {
    /**
     * No description
     *
     * @tags Categories
     * @name CategoriesList
     * @request GET:/api/categories
     */
    categoriesList: (params: RequestParams = {}) =>
      this.request<CategoryDto[], any>({
        path: `/api/categories`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Categories
     * @name CategoriesCreate
     * @request POST:/api/categories
     */
    categoriesCreate: (data: CategoryRequest, params: RequestParams = {}) =>
      this.request<CategoryDto, any>({
        path: `/api/categories`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Categories
     * @name CategoriesUpdate
     * @request PUT:/api/categories/{id}
     */
    categoriesUpdate: (
      { id }: CategoriesUpdateParams,
      data: CategoryRequest,
      params: RequestParams = {},
    ) =>
      this.request<CategoryDto, any>({
        path: `/api/categories/${id}`,
        method: "PUT",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Categories
     * @name CategoriesDelete
     * @request DELETE:/api/categories/{id}
     */
    categoriesDelete: (
      { id }: CategoriesDeleteParams,
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/categories/${id}`,
        method: "DELETE",
        ...params,
      }),
  };
  listings = {
    /**
     * No description
     *
     * @tags Listings
     * @name ListingsList
     * @request GET:/api/listings
     */
    listingsList: (
      query: ListingsListParams = {},
      params: RequestParams = {},
    ) =>
      this.request<ListingDto[], any>({
        path: `/api/listings`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Listings
     * @name ListingsDetail
     * @request GET:/api/listings/{id}
     */
    listingsDetail: (
      { id }: ListingsDetailParams,
      params: RequestParams = {},
    ) =>
      this.request<ListingDto, any>({
        path: `/api/listings/${id}`,
        method: "GET",
        format: "json",
        ...params,
      }),
  };
  myListings = {
    /**
     * No description
     *
     * @tags MyListings
     * @name MyListingsList
     * @request GET:/api/my/listings
     */
    myListingsList: (params: RequestParams = {}) =>
      this.request<ListingDto[], any>({
        path: `/api/my/listings`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags MyListings
     * @name MyListingsCreate
     * @request POST:/api/my/listings
     */
    myListingsCreate: (data: ListingRequest, params: RequestParams = {}) =>
      this.request<ListingDto, any>({
        path: `/api/my/listings`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags MyListings
     * @name MyListingsUpdate
     * @request PUT:/api/my/listings/{id}
     */
    myListingsUpdate: (
      { id }: MyListingsUpdateParams,
      data: ListingRequest,
      params: RequestParams = {},
    ) =>
      this.request<ListingDto, any>({
        path: `/api/my/listings/${id}`,
        method: "PUT",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags MyListings
     * @name MyListingsDelete
     * @request DELETE:/api/my/listings/{id}
     */
    myListingsDelete: (
      { id }: MyListingsDeleteParams,
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/my/listings/${id}`,
        method: "DELETE",
        ...params,
      }),

    /**
     * No description
     *
     * @tags MyListings
     * @name MyListingsStockPartialUpdate
     * @request PATCH:/api/my/listings/{id}/stock
     */
    myListingsStockPartialUpdate: (
      { id }: MyListingsStockPartialUpdateParams,
      data: StockRequest,
      params: RequestParams = {},
    ) =>
      this.request<ListingDto, any>({
        path: `/api/my/listings/${id}/stock`,
        method: "PATCH",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),
  };
  orders = {
    /**
     * No description
     *
     * @tags Orders
     * @name OrdersCreate
     * @request POST:/api/orders
     */
    ordersCreate: (data: OrderRequest, params: RequestParams = {}) =>
      this.request<OrderDto, any>({
        path: `/api/orders`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),
  };
  users = {
    /**
     * No description
     *
     * @tags Users
     * @name UsersMeList
     * @request GET:/api/users/me
     */
    usersMeList: (params: RequestParams = {}) =>
      this.request<UserDto, any>({
        path: `/api/users/me`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Users
     * @name UsersCreate
     * @request POST:/api/users
     */
    usersCreate: (data: CreateUserRequest, params: RequestParams = {}) =>
      this.request<UserDto, any>({
        path: `/api/users`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),
  };
  vendors = {
    /**
     * No description
     *
     * @tags Vendors
     * @name VendorsFeaturedList
     * @request GET:/api/vendors/featured
     */
    vendorsFeaturedList: (params: RequestParams = {}) =>
      this.request<FeaturedVendorDto[], any>({
        path: `/api/vendors/featured`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Vendors
     * @name VendorsDetail
     * @request GET:/api/vendors/{id}
     */
    vendorsDetail: ({ id }: VendorsDetailParams, params: RequestParams = {}) =>
      this.request<VendorDto, any>({
        path: `/api/vendors/${id}`,
        method: "GET",
        format: "json",
        ...params,
      }),
  };
}
