export type FlashSaleStatus = 'upcoming' | 'live' | 'ended';

export interface FlashSaleSlot {
  id: string;
  label: string;
  startsAt: string;
  status: FlashSaleStatus;
}

export interface FlashSaleItem {
  productId: string;
  name: string;
  imageUrl: string;
  price: number;
  originalPrice: number;
  stockTotal: number;
  stockRemaining: number;
  badge?: string;
}

export interface FlashSaleData {
  title: string;
  subtitle: string;
  startsAt: string;
  endsAt: string;
  slots: FlashSaleSlot[];
  items: FlashSaleItem[];
  rules: string[];
}
