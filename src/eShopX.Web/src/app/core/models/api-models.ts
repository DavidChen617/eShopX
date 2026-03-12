export interface GetProductItems {
  productId: string;
  categoryId?: string | null;
  name: string;
  description: string;
  price: number;
  stockQuantity: number;
  isActive: boolean;
  primaryImageUrl?: string | null;
}

export interface GetProductResponse {
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  items: GetProductItems[];
}

export interface CreateProductRequest {
  categoryId?: string | null;
  name: string;
  description?: string | null;
  price: number;
  isActive: boolean;
  stockQuantity: number;
}

export interface UpdateProductRequest {
  categoryId?: string | null;
  name: string;
  description?: string | null;
  price: number;
  isActive: boolean;
  stockQuantity: number;
}

export interface CreateProductResponse {
  productId: string;
  categoryId?: string | null;
  name: string;
  description?: string | null;
  price: number;
  isActive: boolean;
  stockQuantity: number;
  createdAt: string;
  updatedAt: string;
}

export interface ProductImageDto {
  id: string;
  productId: string;
  url: string;
  publicId: string;
  format: string;
  width: number;
  height: number;
  bytes: number;
  isPrimary: boolean;
  sortOrder: number;
  createdAt: string;
}

export interface GetProductDetailResponse {
  productId: string;
  categoryId?: string | null;
  sellerId?: string | null;
  sellerName?: string | null;
  sellerEmail?: string | null;
  sellerPhone?: string | null;
  name: string;
  description?: string | null;
  price: number;
  stockQuantity: number;
  isActive: boolean;
  createdAt: string;
  primaryImageUrl?: string | null;
  images: ProductImageDto[];
}

export interface GetProductImagesResponse {
  images: ProductImageDto[];
}

export interface UploadProductImageResponse {
  image: ProductImageDto;
}

export interface UpdateProductImageResponse {
  image: ProductImageDto;
}

export interface CartItemDto {
  productId: string;
  productName: string;
  unitPrice: number;
  quantity: number;
  subtotal: number;
  stockQuantity: number;
  inStock: boolean;
}

export interface GetCartResponse {
  items: CartItemDto[];
  totalAmount: number;
  totalItems: number;
}

export interface UpdateCartItemResponse {
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  subtotal: number;
}

export interface CheckoutPreviewItemDto {
  productId: string;
  productName: string;
  unitPrice: number;
  quantity: number;
  subtotal: number;
  isAvailable: boolean;
  stockQuantity: number;
}

export interface CheckoutPreviewResponse {
  items: CheckoutPreviewItemDto[];
  totalAmount: number;
  hasUnavailableItems: boolean;
}

export type OrderStatus = 'Paid' | 'Completed' | 'Cancelled';
export type SellerStatus = 'Pending' | 'Approved' | 'Rejected';

export interface QueryUserOrderItem {
  orderId: string;
  orderStatus: OrderStatus;
  totalAmount: number;
  itemCount: number;
  createdAt: string;
  paymentMethod: string;
  paidAt?: string | null;
}

export interface GetUserOrderResponse {
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  items: QueryUserOrderItem[];
}

export interface QueryOrderItem {
  orderItemId: string;
  productId: string;
  productName: string;
  unitPrice: number;
  quantity: number;
  subTotal: number;
}

export interface GetOrderResponse {
  orderId: string;
  userId?: string | null;
  status?: OrderStatus | null;
  totalAmount?: number | null;
  paymentMethod?: string | null;
  paidAt?: string | null;
  shippingName?: string | null;
  shippingAddress?: string | null;
  shippingPhone?: string | null;
  createdAt?: string | null;
  items?: QueryOrderItem[] | null;
}

export interface GetMeResponse {
  userId: string;
  name: string;
  email: string;
  phone: string;
  address?: string | null;
  createdAt: string;
  avatarUrl?: string | null;
  avatarPublicId?: string | null;
  avatarFormat?: string | null;
  avatarWidth?: number | null;
  avatarHeight?: number | null;
  avatarBytes?: number | null;
  isSeller: boolean;
  isAdmin: boolean;
  sellerStatus?: SellerStatus | null;
  sellerAppliedAt?: string | null;
  sellerApprovedAt?: string | null;
  sellerRejectionReason?: string | null;
}

export interface ApplyForSellerResponse {
  userId: string;
  status: SellerStatus;
  appliedAt: string;
}

export interface RegisterUserResponse {
  userId: string;
  email: string;
  createdAt: string;
}

export interface UploadUserAvatarResponse {
  url: string;
  publicId: string;
  format: string;
  width: number;
  height: number;
  bytes: number;
}

export interface PendingSellerItem {
  userId: string;
  userName: string;
  email: string;
  appliedAt: string;
}

export interface GetPendingSellersResponse {
  items: PendingSellerItem[];
}

export interface ApproveSellerResponse {
  userId: string;
  userName: string;
  approvedAt: string;
}

export interface RejectSellerResponse {
  userId: string;
  userName: string;
  rejectedAt: string;
  reason: string;
}

export interface HomepageReviewItem {
  reviewId: string;
  userName?: string | null;
  userAvatar?: string | null;
  productId: string;
  productName: string;
  productImage?: string | null;
  rating: number;
  content?: string | null;
  imageUrls: string[];
  createdAt: string;
}

export interface GetProductReviewsResponse {
  items: ReviewItem[];
  totalCount: number;
  averageRating: number;
}

export interface ReviewItem {
  reviewId: string;
  userName?: string | null;
  rating: number;
  content?: string | null;
  imageUrls: string[];
  createdAt: string;
}

export interface CreateReviewRequest {
  orderItemId: string;
  rating: number;
  content?: string | null;
  isAnonymous?: boolean;
  imageUrls?: string[] | null;
}

export interface CreateReviewResponse {
  reviewId: string;
  productId: string;
  rating: number;
  createdAt: string;
}

export interface UpdateReviewRequest {
  rating: number;
  content?: string | null;
  isAnonymous?: boolean;
  imageUrls?: string[] | null;
}

export interface UpdateReviewResponse {
  reviewId: string;
  rating: number;
  updatedAt: string;
}
