/**
 * 通用 API 回傳格式
 */
export interface ApiResponse<T> {
  isSuccess: boolean;
  code: string;
  data: T;
  problem?: {
    status: number;
    title: string;
    detail: string;
  };
}

/**
 * Auth 相關介面
 */
export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  userId: string;
  name: string;
  expiresAt: string;
}

export interface RegisterRequest {
  name: string;
  email: string;
  password: string;
  otp: string;
}

export interface RegisterResponse {
  userId: string;
  email: string;
  createdAt: string;
}

export interface SendOtpRequest {
  email: string;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export interface RefreshTokenResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
}

export interface LogoutRequest {
  refreshToken: string;
}

export interface GetMeResponse {
  id: string;
  name: string;
  email: string;
  avatarUrl?: string | null;
  roles: string[];
  isLocalUser: boolean;
}

export interface GoogleAuthRequest {
  code: string;
  codeVerifier: string;
  state: string;
}

export interface GoogleAuthResponse extends LoginResponse {
  googleSub: string;
  email: string;
  picture?: string | null;
  avatarUrl?: string | null;
}

export interface LineAuthRequest {
  code: string;
  codeVerifier?: string;
  nonce?: string;
}

export interface LineAuthResponse extends LoginResponse {
  lineSub: string;
  email: string;
  picture?: string | null;
  avatarUrl?: string | null;
}

/**
 * 商品相關介面
 */
export type Audience = 'Men' | 'Women';

export interface SKU {
  id: string;
  sizeId: string;
  price: number;
  stock: number;
}

export interface ProductImage {
  id: string;
  url: string;
  publicId: string;
  isPrimary: boolean;
  sortOrder: number;
}

export interface ProductVariant {
  id: string;
  color: string;
  skus: SKU[];
  images: ProductImage[];
}

export interface Product {
  id: string;
  name: string;
  description: string;
  audience: Audience;
  isActive: boolean;
  categoryId: string;
  tagIds: string[];
  variants: ProductVariant[];
  createdAt: string;
  updatedAt: string;
}

export interface ProductUpsertPayload {
  name: string;
  description: string;
  audience: Audience;
  categoryId: string;
  tagIds: string[];
  variants: Array<{
    variantId?: string;
    color: string;
    skus: Array<{
      skuId?: string;
      sizeId: string;
      price: number;
      stock: number;
    }>;
    images: Array<{
      url: string;
      publicId: string;
      isPrimary: boolean;
      sortOrder: number;
    }>;
  }>;
}

export interface ProductListItemResponse {
  id: string;
  name: string;
  audience: Audience | null;
  isActive: boolean;
  categoryId: string;
  updatedAt?: string;
  primaryImageUrl: string;
  startingPrice: number;
  colors: string[];
}

export interface GetProductsResponse {
  items: ProductListItemResponse[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface CreateProductResponse {
  productId: string;
  name: string;
  isActive: boolean;
  createdAt: string;
}

export interface UploadedProductImage {
  fileName: string;
  url: string;
  publicId: string;
}

/**
 * 搜尋結果商品項 (扁平化)
 */
export interface ProductSummary {
  productId: string;
  categoryId: string;
  name: string;
  description: string;
  price: number;
  stockQuantity: number;
  isActive: boolean;
  primaryImageUrl: string;
}

export interface ProductSearchResponse {
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  items: ProductSummary[];
}

/**
 * 分類相關介面
 */
export interface Category {
  id: string;
  name: string;
  createdAt: string;
}

export interface Size {
  id: string;
  name: string;
  type: string;
  createdAt: string;
}

export interface Tag {
  id: string;
  name: string;
  type: 'Season' | 'Style' | 'Feature';
  createdAt: string;
}

/**
 * 購物車相關介面
 */
export interface CartItem {
  itemId: string;
  skuId: string;
  quantity: number;
  productName: string;
  color: string;
  sizeName: string;
  unitPrice: number;
  primaryImageUrl: string;
  selected: boolean;
}

export interface Cart {
  cartId: string;
  userId: string;
  items: CartItem[];
}

/**
 * 訂單相關介面
 */
export type PaymentMethod = 'LinePay' | 'PayPal';
export type OrderStatus = 'PendingPayment' | 'Paid' | 'Shipped' | 'Completed';

export interface StartLogisticsRequest {
  receiverName: string;
  receiverPhone: string;
  goodsAmount: number;
}

export interface LogisticsStatusResponse {
  isReady: boolean;
  logisticsSubType?: string;
  storeName?: string;
  address?: string;
}

export interface CreateOrderRequest {
  paymentMethod: PaymentMethod;
  receiverName: string;
  receiverPhone: string;
}

export interface CreateOrderResponse {
  orderId: string;
  totalAmount: number;
  paymentUrl: string;
  createdAt: string;
}

export interface OrderListItem {
  orderId: string;
  userId: string;
  status: OrderStatus;
  totalAmount: number;
  createdAt: string;
}

export interface GetOrdersResponse {
  items: OrderListItem[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface OrderItem {
  itemId: string;
  skuId: string;
  productName: string;
  color: string;
  size: string;
  unitPrice: number;
  quantity: number;
  totalPrice: number;
}

export interface OrderResponse {
  orderId: string;
  userId: string;
  status: OrderStatus;
  totalAmount: number;
  createdAt: string;
  items: OrderItem[];
}
