// Banner 輪播
export interface BannerSlide {
  id: string;
  imageUrl: string;
  title: string;
  link: string;
}

export interface BannerManageItem {
  id: string;
  title: string;
  imageUrl: string;
  link: string;
  sortOrder?: number;
  isActive?: boolean;
  startsAt?: string | null;
  endsAt?: string | null;
}

// 分類入口
export interface CategoryEntry {
  id: string;
  name: string;
  icon: string; // nz-icon type 或 emoji
  link: string;
}

// 推薦商品
export interface RecommendProduct {
  id: string;
  name: string;
  imageUrl: string;
  price: number;
  originalPrice?: number;
  salesCount?: number;
  shopName?: string;
}
