import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

import { LocalStorageService } from '../storage/local-storage.service';
import { ApiResponse } from './api.service';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResult {
  accessToken: string;
  refreshToken: string;
  userId: string;
  name: string;
  expiresAt: string;
}

export interface GoogleAuthResult {
  googleSub?: string;
  email?: string;
  name?: string;
  accessToken?: string;
  refreshToken?: string;
  expiresAt?: string;
}

export interface LinePayRequestResponse {
  returnCode: string;
  returnMessage: string;
  info?: {
    paymentUrl?: {
      web?: string;
      app?: string;
    };
    transactionId?: number;
    paymentAccessToken?: string;
  };
}

export interface PayPalCreateOrderResult {
  orderId: string;
  approveUrl: string;
}

export interface RegisterRequest {
  name: string;
  email: string;
  phone: string;
  password: string;
}

export interface RegisterResult {
  userId: string;
  email: string;
  createdAt: string;
}

export type { ApiResponse } from './api.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly accessTokenKey = 'eshopx.accessToken';
  private readonly refreshTokenKey = 'eshopx.refreshToken';
  private readonly expiresAtKey = 'eshopx.expiresAt';
  private readonly userIdKey = 'eshopx.userId';
  private readonly userNameKey = 'eshopx.userName';
  private readonly googleProfileKey = 'eshopx.googleProfile';
  private readonly authenticatedSignal = signal(false);
  readonly authState = this.authenticatedSignal.asReadonly();
  private readonly clientId = '81840048967-d6k4331ks8sllq08qac4morq5877t843.apps.googleusercontent.com';
  private readonly redirectUri = 'http://localhost:4200/auth/google/callback';
  private readonly scope = 'openid email profile';
  private readonly lineChannelId = '2009031910';
  private readonly lineRedirectUri = 'http://localhost:4200/auth/line/callback';
  private readonly lineScope = 'openid profile email';
  constructor(
    private readonly storage: LocalStorageService,
    private readonly http: HttpClient
  ) {
    this.authenticatedSignal.set(this.isAuthenticated());
  }

  authenticated() {
    return this.authenticatedSignal();
  }

  async login(request: LoginRequest): Promise<LoginResult> {
    const result = await firstValueFrom(
      this.http.post<LoginResult>('/api/auth/login', request)
    );

    this.storage.setItem(this.accessTokenKey, result.accessToken);
    this.storage.setItem(this.refreshTokenKey, result.refreshToken);
    this.storage.setItem(this.expiresAtKey, result.expiresAt);
    this.storage.setItem(this.userIdKey, result.userId);
    this.storage.setItem(this.userNameKey, result.name);
    this.storage.removeItem(this.googleProfileKey);
    this.authenticatedSignal.set(true);

    return result;
  }

  async register(request: RegisterRequest): Promise<RegisterResult> {
    const result = await firstValueFrom(
      this.http.post<RegisterResult>('/api/auth/register', request)
    );
    return result;
  }

  getUserId(): string | null {
    return this.storage.getItem(this.userIdKey);
  }

  getUserName(): string | null {
    return this.storage.getItem(this.userNameKey);
  }

  logout(): void {
    this.storage.removeItem(this.accessTokenKey);
    this.storage.removeItem(this.refreshTokenKey);
    this.storage.removeItem(this.expiresAtKey);
    this.storage.removeItem(this.userIdKey);
    this.storage.removeItem(this.userNameKey);
    this.storage.removeItem(this.googleProfileKey);
    this.authenticatedSignal.set(false);
  }

  isAuthenticated(): boolean {
    const token = this.storage.getItem(this.accessTokenKey);
    const expiresAt = this.storage.getItem(this.expiresAtKey);
    if (token && expiresAt) {
      return new Date(expiresAt).getTime() > Date.now();
    }
    return !!this.storage.getItem(this.googleProfileKey);
  }

  private base64UrlEncode(buffer: ArrayBuffer) {
    return btoa(String.fromCharCode(...new Uint8Array(buffer)))
      .replace(/\+/g, '-')
      .replace(/\//g, '_')
      .replace(/=+$/, '');
  }

  private async sha256(str: string) {
    const data = new TextEncoder().encode(str);
    return crypto.subtle.digest('SHA-256', data);
  }

  private randomString(len = 64) {
    const charset = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-._~';
    const bytes = crypto.getRandomValues(new Uint8Array(len));
    return Array.from(bytes, b => charset[b % charset.length]).join('');
  }

  exchangeGoogleCode(code: string, state: string) {
    const savedState = sessionStorage.getItem('google_oauth_state');
    const codeVerifier = sessionStorage.getItem('google_pkce_verifier');

    if (!savedState || savedState !== state || !codeVerifier) {
      throw new Error('Invalid OAuth state or missing code_verifier');
    }

    return this.http.post<GoogleAuthResult>('/api/auth/google/callback', {
      code,
      codeVerifier,
      state,
    });
  }

  exchangeLineCode(code: string, state: string) {
    const savedState = sessionStorage.getItem('line_oauth_state');
    const codeVerifier = sessionStorage.getItem('line_pkce_verifier');
    const nonce = sessionStorage.getItem('line_oauth_nonce');

    if (!savedState || savedState !== state || !codeVerifier) {
      throw new Error('Invalid LINE OAuth state or missing code_verifier');
    }

    return this.http.post<GoogleAuthResult>('/api/auth/line/callback', {
      code,
      codeVerifier,
      nonce,
    });
  }

  storeAuthResult(result: GoogleAuthResult | LoginResult) {
    if ('accessToken' in result && result.accessToken && result.expiresAt && result.refreshToken) {
      this.storage.setItem(this.accessTokenKey, result.accessToken);
      this.storage.setItem(this.refreshTokenKey, result.refreshToken);
      this.storage.setItem(this.expiresAtKey, result.expiresAt);
      if ('userId' in result && result.userId) {
        this.storage.setItem(this.userIdKey, result.userId);
      }
      if ('name' in result && result.name) {
        this.storage.setItem(this.userNameKey, result.name);
      }
      this.storage.removeItem(this.googleProfileKey);
      this.authenticatedSignal.set(true);
      return;
    }

    const googleProfile = {
      googleSub: (result as GoogleAuthResult).googleSub ?? '',
      email: (result as GoogleAuthResult).email ?? '',
      name: (result as GoogleAuthResult).name ?? '',
    };
    this.storage.setItem(this.googleProfileKey, JSON.stringify(googleProfile));
    this.authenticatedSignal.set(true);
  }

  clearTempGoogleState() {
    sessionStorage.removeItem('google_oauth_state');
    sessionStorage.removeItem('google_pkce_verifier');
  }

  clearTempLineState() {
    sessionStorage.removeItem('line_oauth_state');
    sessionStorage.removeItem('line_pkce_verifier');
    sessionStorage.removeItem('line_oauth_nonce');
  }
  async loginWithGoogle() {
    const codeVerifier = this.randomString(64);
    const codeChallenge = this.base64UrlEncode(await this.sha256(codeVerifier));

    const state = this.randomString(32);
    sessionStorage.setItem('google_pkce_verifier', codeVerifier);
    sessionStorage.setItem('google_oauth_state', state);

    const params = new URLSearchParams({
      client_id: this.clientId,
      redirect_uri: this.redirectUri,
      response_type: 'code',
      scope: this.scope,
      code_challenge: codeChallenge,
      code_challenge_method: 'S256',
      state
    });

    window.location.href =
      `https://accounts.google.com/o/oauth2/v2/auth?${params.toString()}`;
  }

  async loginWithLine() {
    const codeVerifier = this.randomString(64);
    const codeChallenge = this.base64UrlEncode(await this.sha256(codeVerifier));
    const state = this.randomString(32);
    const nonce = this.randomString(32);

    sessionStorage.setItem('line_pkce_verifier', codeVerifier);
    sessionStorage.setItem('line_oauth_state', state);
    sessionStorage.setItem('line_oauth_nonce', nonce);

    const params = new URLSearchParams({
      response_type: 'code',
      client_id: this.lineChannelId,
      redirect_uri: this.lineRedirectUri,
      state,
      scope: this.lineScope,
      nonce,
      code_challenge: codeChallenge,
      code_challenge_method: 'S256',
    });

    window.location.href =
      `https://access.line.me/oauth2/v2.1/authorize?${params.toString()}`;
  }

  async createLinePayOrder(
    amount: number,
    currency: string,
    items: { id: string; name: string; quantity: number; price: number }[],
    orderId?: string
  ): Promise<void> {
    const finalOrderId = orderId ?? `LP-${Date.now()}`;
    const userId = this.getUserId();
    const body = {
      amount,
      currency,
      orderId: finalOrderId,
      packages: [
        {
          id: 'pkg-1',
          amount,
          products: items.map((item) => ({
            id: item.id,
            name: item.name,
            quantity: item.quantity,
            price: item.price,
          })),
        },
      ],
      redirectUrls: {
        confirmUrl: `/pay/line/confirm?amount=${amount}&currency=${currency}&orderId=${finalOrderId}${userId ? `&userId=${userId}` : ''}`,
        cancelUrl: '/pay/line/cancel',
      },
    };

    const response = await firstValueFrom(
      this.http.post<LinePayRequestResponse>('/api/payments/line/request', body)
    );

    const url = response?.info?.paymentUrl?.web;
    if (url) {
      window.location.href = url;
      return;
    }

    throw new Error(response?.returnMessage || 'Line Pay request failed');
  }

  async createPayPalOrder(amount: number, currency: string, orderId?: string): Promise<void> {
    const userId = this.getUserId();
    const body = {
      amount,
      currency,
      orderId: orderId ?? `PP-${Date.now()}`,
      userId,
    };

    const response = await firstValueFrom(
      this.http.post<PayPalCreateOrderResult>('/api/payments/paypal/create-order', body)
    );

    const approveUrl = response?.approveUrl;
    if (approveUrl) {
      window.location.href = approveUrl;
      return;
    }

    throw new Error('PayPal create order failed');
  }

  async testPayPal(): Promise<void> {
    await this.createPayPalOrder(10, 'USD');
  }
}
