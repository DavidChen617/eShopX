import {
  GetCartResponse,
  GetMeResponse,
  GetOrderResponse,
  GetProductDetailResponse,
  GetProductImagesResponse,
  GetProductResponse,
  GetUserOrderResponse,
} from '../models/api-models';
import { BannerSlide, CategoryEntry, RecommendProduct } from '../models/home-models';
import { FlashSaleData } from '../models/flash-sale-models';

export const mockProducts: GetProductResponse = {
  page: 1,
  pageSize: 12,
  totalCount: 4,
  totalPages: 1,
  items: [
    {
      productId: '9f8b6e36-8d34-4c14-8e5b-6d7c2b46f001',
      name: 'Everyday Tee',
      description: 'Soft, breathable cotton for daily wear.',
      price: 48,
      stockQuantity: 120,
      isActive: true,
      primaryImageUrl: 'https://placehold.co/640x800',
    },
    {
      productId: '9f8b6e36-8d34-4c14-8e5b-6d7c2b46f002',
      name: 'Soft Knit Pullover',
      description: 'Light warmth with a relaxed drape.',
      price: 92,
      stockQuantity: 64,
      isActive: true,
      primaryImageUrl: 'https://placehold.co/640x800',
    },
    {
      productId: '9f8b6e36-8d34-4c14-8e5b-6d7c2b46f003',
      name: 'Cloud Jogger',
      description: 'Comfort-first fit with clean lines.',
      price: 78,
      stockQuantity: 80,
      isActive: true,
      primaryImageUrl: 'https://placehold.co/640x800',
    },
    {
      productId: '9f8b6e36-8d34-4c14-8e5b-6d7c2b46f004',
      name: 'Relaxed Shirt',
      description: 'Easy layer for work or weekend.',
      price: 64,
      stockQuantity: 55,
      isActive: true,
      primaryImageUrl: 'https://placehold.co/640x800',
    },
  ],
};

export const mockProductDetail: GetProductDetailResponse = {
  productId: '9f8b6e36-8d34-4c14-8e5b-6d7c2b46f002',
  name: 'Soft Knit Pullover',
  description: 'Light warmth with a relaxed drape.',
  price: 92,
  stockQuantity: 64,
  isActive: true,
  createdAt: '2025-12-10T08:30:00Z',
  primaryImageUrl: null,
  images: [
    {
      id: 'b5dd69f7-4e0a-4429-9f55-3e2e69bb86c1',
      productId: '9f8b6e36-8d34-4c14-8e5b-6d7c2b46f002',
      url: 'https://placehold.co/640x800',
      publicId: 'mock-public-1',
      format: 'jpg',
      width: 640,
      height: 800,
      bytes: 120000,
      isPrimary: true,
      sortOrder: 1,
      createdAt: '2025-12-10T08:30:00Z',
    },
    {
      id: 'b5dd69f7-4e0a-4429-9f55-3e2e69bb86c2',
      productId: '9f8b6e36-8d34-4c14-8e5b-6d7c2b46f002',
      url: 'https://placehold.co/640x800',
      publicId: 'mock-public-2',
      format: 'jpg',
      width: 640,
      height: 800,
      bytes: 118000,
      isPrimary: false,
      sortOrder: 2,
      createdAt: '2025-12-10T08:30:00Z',
    },
  ],
};

export const mockProductImages: GetProductImagesResponse = {
  images: mockProductDetail.images,
};

export const mockCart: GetCartResponse = {
  items: [
    {
      productId: '9f8b6e36-8d34-4c14-8e5b-6d7c2b46f001',
      productName: 'Everyday Tee',
      unitPrice: 48,
      quantity: 2,
      subtotal: 96,
      stockQuantity: 120,
      inStock: true,
    },
    {
      productId: '9f8b6e36-8d34-4c14-8e5b-6d7c2b46f003',
      productName: 'Cloud Jogger',
      unitPrice: 78,
      quantity: 1,
      subtotal: 78,
      stockQuantity: 80,
      inStock: true,
    },
  ],
  totalAmount: 174,
  totalItems: 3,
};

export const mockOrders: GetUserOrderResponse = {
  page: 1,
  pageSize: 10,
  totalCount: 2,
  totalPages: 1,
  items: [
    {
      orderId: '5c9b57b0-8f09-4b5b-9e78-8edb541c2d01',
      orderStatus: 'Paid',
      totalAmount: 174,
      itemCount: 3,
      createdAt: '2026-01-12T14:20:00Z',
      paymentMethod: 'PayPal',
      paidAt: '2026-01-12T14:21:10Z',
    },
    {
      orderId: '5c9b57b0-8f09-4b5b-9e78-8edb541c2d02',
      orderStatus: 'Completed',
      totalAmount: 92,
      itemCount: 1,
      createdAt: '2025-12-22T10:05:00Z',
      paymentMethod: 'LinePay',
      paidAt: '2025-12-22T10:06:05Z',
    },
  ],
};

export const mockOrderDetail: GetOrderResponse = {
  orderId: '5c9b57b0-8f09-4b5b-9e78-8edb541c2d01',
  userId: '2d829d1b-9e2a-4b2c-9d45-59ed2b318a10',
  status: 'Completed',
  totalAmount: 174,
  paymentMethod: 'PayPal',
  paidAt: '2026-01-12T14:21:10Z',
  shippingName: 'Iris Chen',
  shippingAddress: 'No. 123, Zhongxiao Rd, Taipei',
  shippingPhone: '0912-345-678',
  createdAt: '2026-01-12T14:20:00Z',
  items: [
    {
      orderItemId: '8f1e2c11-1111-4444-9999-a11111111111',
      productId: '9f8b6e36-8d34-4c14-8e5b-6d7c2b46f001',
      productName: 'Everyday Tee',
      unitPrice: 48,
      quantity: 2,
      subTotal: 96,
    },
    {
      orderItemId: '8f1e2c11-2222-4444-9999-a22222222222',
      productId: '9f8b6e36-8d34-4c14-8e5b-6d7c2b46f003',
      productName: 'Cloud Jogger',
      unitPrice: 78,
      quantity: 1,
      subTotal: 78,
    },
  ],
};

export const mockMe: GetMeResponse = {
  userId: '2d829d1b-9e2a-4b2c-9d45-59ed2b318a10',
  name: 'Iris Chen',
  email: 'iris.chen@example.com',
  phone: '0912-345-678',
  address: 'No. 123, Zhongxiao Rd, Taipei',
  createdAt: '2025-07-18T09:15:00Z',
  avatarUrl: null,
  avatarPublicId: null,
  avatarFormat: null,
  avatarWidth: null,
  avatarHeight: null,
  avatarBytes: null,
  isSeller: true,
  isAdmin: false,
  sellerStatus: 'Approved',
  sellerAppliedAt: '2025-08-01T10:00:00Z',
  sellerApprovedAt: '2025-08-02T12:30:00Z',
  sellerRejectionReason: null,
};

// ========== 淘寶風格首頁資料 ==========

export const mockBanners: BannerSlide[] = [
  {
    id: 'banner-1',
    imageUrl: 'https://placehold.co/750x300/FF5000/FFFFFF?text=雙11狂歡節',
    title: '雙11狂歡節 全場5折起',
    link: '/products?promo=double11',
  },
  {
    id: 'banner-2',
    imageUrl: 'https://placehold.co/750x300/FF2D54/FFFFFF?text=限時秒殺',
    title: '限時秒殺 每日10點開搶',
    link: '/products?flash=true',
  },
  {
    id: 'banner-3',
    imageUrl: 'https://placehold.co/750x300/FFD700/333333?text=新品上市',
    title: '春季新品 搶先預購',
    link: '/products?q=new',
  },
];

export const mockCategories: CategoryEntry[] = [
  { id: 'cat-1', name: '服飾', icon: '👕', link: '/products?cat=clothing' },
  { id: 'cat-2', name: '美妝', icon: '💄', link: '/products?cat=beauty' },
  { id: 'cat-3', name: '生鮮', icon: '🍎', link: '/products?cat=fresh' },
  { id: 'cat-4', name: '數碼', icon: '📱', link: '/products?cat=digital' },
  { id: 'cat-5', name: '家居', icon: '🏠', link: '/products?cat=home' },
  { id: 'cat-6', name: '遊戲', icon: '🎮', link: '/products?cat=gaming' },
  { id: 'cat-7', name: '書籍', icon: '📚', link: '/products?cat=books' },
  { id: 'cat-8', name: '箱包', icon: '🎒', link: '/products?cat=bags' },
  { id: 'cat-9', name: '運動', icon: '⚽', link: '/products?cat=sports' },
  { id: 'cat-10', name: '更多', icon: '⋯', link: '/products' },
];

export const mockRecommendProducts: RecommendProduct[] = [
  {
    id: '9f8b6e36-8d34-4c14-8e5b-6d7c2b46f001',
    name: '經典款純棉T恤 舒適透氣 四季百搭',
    imageUrl: 'https://placehold.co/400x400',
    price: 48,
    originalPrice: 98,
    salesCount: 1234,
    shopName: '潮流服飾旗艦店',
  },
  {
    id: '9f8b6e36-8d34-4c14-8e5b-6d7c2b46f002',
    name: '柔軟針織衫 秋冬保暖 時尚休閒',
    imageUrl: 'https://placehold.co/400x400',
    price: 92,
    originalPrice: 168,
    salesCount: 856,
    shopName: '優質男裝專營店',
  },
  {
    id: '9f8b6e36-8d34-4c14-8e5b-6d7c2b46f003',
    name: '運動休閒褲 彈力面料 跑步健身必備',
    imageUrl: 'https://placehold.co/400x400',
    price: 78,
    originalPrice: 128,
    salesCount: 2341,
    shopName: '運動裝備專賣',
  },
  {
    id: '9f8b6e36-8d34-4c14-8e5b-6d7c2b46f004',
    name: '輕薄襯衫 商務休閒兩用 免燙處理',
    imageUrl: 'https://placehold.co/400x400',
    price: 64,
    originalPrice: 118,
    salesCount: 567,
    shopName: '紳士服飾館',
  },
  {
    id: '9f8b6e36-8d34-4c14-8e5b-6d7c2b46f005',
    name: '潮流帽T 加絨保暖 街頭風格',
    imageUrl: 'https://placehold.co/400x400',
    price: 128,
    originalPrice: 198,
    salesCount: 789,
    shopName: '街頭潮牌店',
  },
  {
    id: '9f8b6e36-8d34-4c14-8e5b-6d7c2b46f006',
    name: '牛仔外套 復古水洗 百搭單品',
    imageUrl: 'https://placehold.co/400x400',
    price: 168,
    originalPrice: 268,
    salesCount: 432,
    shopName: '丹寧專門店',
  },
];

export const mockFlashSale: FlashSaleData = {
  title: '限時秒殺 · 今晚 20:00',
  subtitle: '高併發搶購體驗，需登入才能參與。',
  startsAt: '2026-02-02T12:00:00Z',
  endsAt: '2026-02-02T23:59:59Z',
  slots: [
    {
      id: 'slot-1',
      label: '10:00',
      startsAt: '2026-02-01T10:00:00Z',
      status: 'ended',
    },
    {
      id: 'slot-2',
      label: '12:00',
      startsAt: '2026-02-01T12:00:00Z',
      status: 'live',
    },
    {
      id: 'slot-3',
      label: '14:00',
      startsAt: '2026-02-01T14:00:00Z',
      status: 'upcoming',
    },
    {
      id: 'slot-4',
      label: '16:00',
      startsAt: '2026-02-01T16:00:00Z',
      status: 'upcoming',
    },
  ],
  items: [
    {
      productId: '9f8b6e36-8d34-4c14-8e5b-6d7c2b46f001',
      name: 'Everyday Tee',
      imageUrl: 'https://placehold.co/320x320/FF5000/FFFFFF?text=秒殺',
      price: 29,
      originalPrice: 48,
      stockTotal: 500,
      stockRemaining: 120,
      badge: 'Hot',
    },
    {
      productId: '9f8b6e36-8d34-4c14-8e5b-6d7c2b46f002',
      name: 'Soft Knit Pullover',
      imageUrl: 'https://placehold.co/320x320/FF2D54/FFFFFF?text=限量',
      price: 59,
      originalPrice: 92,
      stockTotal: 300,
      stockRemaining: 60,
      badge: '限量',
    },
    {
      productId: '9f8b6e36-8d34-4c14-8e5b-6d7c2b46f003',
      name: 'Cloud Jogger',
      imageUrl: 'https://placehold.co/320x320/999999/FFFFFF?text=售罄',
      price: 49,
      originalPrice: 78,
      stockTotal: 400,
      stockRemaining: 0,
      badge: '售罄',
    },
    {
      productId: '9f8b6e36-8d34-4c14-8e5b-6d7c2b46f004',
      name: 'Relaxed Shirt',
      imageUrl: 'https://placehold.co/320x320/FFD700/333333?text=新品',
      price: 39,
      originalPrice: 64,
      stockTotal: 200,
      stockRemaining: 180,
      badge: '新品',
    },
  ],
  rules: [
    '需登入會員才能參與秒殺。',
    '商品售完即止，付款未完成將釋放庫存。',
    '每位會員每商品限購 1 件。',
  ],
};
