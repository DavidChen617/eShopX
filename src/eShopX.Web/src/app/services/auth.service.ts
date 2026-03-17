import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { catchError, finalize, map, Observable, of, shareReplay, switchMap, tap, throwError } from 'rxjs';
import {
  ApiResponse,
  GetMeResponse,
  GoogleAuthRequest,
  GoogleAuthResponse,
  LoginRequest,
  LoginResponse,
  LineAuthRequest,
  LineAuthResponse,
  LogoutRequest,
  RefreshTokenRequest,
  RefreshTokenResponse,
  RegisterRequest,
  RegisterResponse,
  SendOtpRequest,
} from '../models/api.models';
import { apiUrl } from '../shared/api.config';

interface AuthSession {
  accessToken: string | null;
  refreshToken: string | null;
  expiresAt: string | null;
  userId: string | null;
  name: string | null;
  email: string | null;
  avatarUrl: string | null;
  roles: string[];
  isLocalUser: boolean;
}

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = apiUrl('/v1/auth');
  private readonly googleClientId = '81840048967-d6k4331ks8sllq08qac4morq5877t843.apps.googleusercontent.com';
  private readonly googleScope = 'openid email profile';
  private readonly lineChannelId = '2009031910';
  private readonly lineScope = 'openid profile email';
  private readonly accessTokenKey = 'eshopx.accessToken';
  private readonly refreshTokenKey = 'eshopx.refreshToken';
  private readonly expiresAtKey = 'eshopx.expiresAt';
  private readonly userIdKey = 'eshopx.userId';
  private readonly userNameKey = 'eshopx.userName';
  private readonly userEmailKey = 'eshopx.userEmail';
  private readonly userAvatarKey = 'eshopx.userAvatar';
  private readonly userRolesKey = 'eshopx.userRoles';
  private readonly isLocalUserKey = 'eshopx.isLocalUser';
  private readonly googleStateKey = 'google_oauth_state';
  private readonly googleVerifierKey = 'google_pkce_verifier';
  private readonly lineStateKey = 'line_oauth_state';
  private readonly lineVerifierKey = 'line_pkce_verifier';
  private readonly lineNonceKey = 'line_oauth_nonce';

  private refreshInFlight$: Observable<RefreshTokenResponse> | null = null;
  private readonly session = signal<AuthSession>(this.readSession());

  readonly isAuthenticated = computed(() => !!this.session().refreshToken);
  readonly userName = computed(() => this.session().name);
  readonly userId = computed(() => this.session().userId);
  readonly userEmail = computed(() => this.session().email);
  readonly userAvatarUrl = computed(() => this.session().avatarUrl);
  readonly roles = computed(() => this.session().roles);
  readonly isAdmin = computed(() => this.session().roles.includes('Admin'));

  constructor() {
    if (this.getRefreshToken()) {
      this.bootstrapCurrentUser().subscribe();
    }
  }

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http.post<ApiResponse<LoginResponse>>(`${this.baseUrl}/login`, request).pipe(
      map((response) => response.data),
      tap((response) => this.persistSession(response)),
      switchMap((response) => this.fetchCurrentUser().pipe(map(() => response)))
    );
  }

  sendOtp(email: string): Observable<void> {
    const request: SendOtpRequest = { email };
    return this.http.post<ApiResponse<unknown>>(`${this.baseUrl}/send-otp`, request).pipe(
      map(() => void 0)
    );
  }

  register(request: RegisterRequest): Observable<RegisterResponse> {
    return this.http.post<ApiResponse<RegisterResponse>>(`${this.baseUrl}/register`, request).pipe(
      map((response) => response.data)
    );
  }

  async loginWithGoogle(): Promise<void> {
    const codeVerifier = this.randomString(64);
    const codeChallenge = this.base64UrlEncode(await this.sha256(codeVerifier));
    const state = this.randomString(32);

    sessionStorage.setItem(this.googleVerifierKey, codeVerifier);
    sessionStorage.setItem(this.googleStateKey, state);

    const params = new URLSearchParams({
      client_id: this.googleClientId,
      redirect_uri: this.buildCallbackUrl('/auth/google/callback'),
      response_type: 'code',
      scope: this.googleScope,
      code_challenge: codeChallenge,
      code_challenge_method: 'S256',
      state,
    });

    window.location.href = `https://accounts.google.com/o/oauth2/v2/auth?${params.toString()}`;
  }

  exchangeGoogleCode(code: string, state: string): Observable<GoogleAuthResponse> {
    const savedState = sessionStorage.getItem(this.googleStateKey);
    const codeVerifier = sessionStorage.getItem(this.googleVerifierKey);

    if (!savedState || savedState !== state || !codeVerifier) {
      return throwError(() => new Error('Invalid Google OAuth state.'));
    }

    const request: GoogleAuthRequest = {
      code,
      codeVerifier,
      state,
    };

    return this.http.post<ApiResponse<GoogleAuthResponse>>(`${this.baseUrl}/google`, request).pipe(
      map((response) => response.data),
      tap((response) => this.persistSession(response)),
      switchMap((response) => this.fetchCurrentUser().pipe(map(() => response)))
    );
  }

  clearGoogleState(): void {
    sessionStorage.removeItem(this.googleStateKey);
    sessionStorage.removeItem(this.googleVerifierKey);
  }

  async loginWithLine(): Promise<void> {
    const codeVerifier = this.randomString(64);
    const codeChallenge = this.base64UrlEncode(await this.sha256(codeVerifier));
    const state = this.randomString(32);
    const nonce = this.randomString(32);

    sessionStorage.setItem(this.lineVerifierKey, codeVerifier);
    sessionStorage.setItem(this.lineStateKey, state);
    sessionStorage.setItem(this.lineNonceKey, nonce);

    const params = new URLSearchParams({
      response_type: 'code',
      client_id: this.lineChannelId,
      redirect_uri: this.buildCallbackUrl('/auth/line/callback'),
      state,
      scope: this.lineScope,
      nonce,
      code_challenge: codeChallenge,
      code_challenge_method: 'S256',
    });

    window.location.href = `https://access.line.me/oauth2/v2.1/authorize?${params.toString()}`;
  }

  exchangeLineCode(code: string, state: string): Observable<LineAuthResponse> {
    const savedState = sessionStorage.getItem(this.lineStateKey);
    const codeVerifier = sessionStorage.getItem(this.lineVerifierKey);
    const nonce = sessionStorage.getItem(this.lineNonceKey);

    if (!savedState || savedState !== state || !codeVerifier) {
      return throwError(() => new Error('Invalid LINE OAuth state.'));
    }

    const request: LineAuthRequest = {
      code,
      codeVerifier,
      nonce: nonce ?? undefined,
    };

    return this.http.post<ApiResponse<LineAuthResponse>>(`${this.baseUrl}/line`, request).pipe(
      map((response) => response.data),
      tap((response) => this.persistSession(response)),
      switchMap((response) => this.fetchCurrentUser().pipe(map(() => response)))
    );
  }

  clearLineState(): void {
    sessionStorage.removeItem(this.lineStateKey);
    sessionStorage.removeItem(this.lineVerifierKey);
    sessionStorage.removeItem(this.lineNonceKey);
  }

  logout(): Observable<void> {
    const refreshToken = this.getRefreshToken();
    const request$ = refreshToken
      ? this.http.post<void>(`${this.baseUrl}/logout`, { refreshToken } satisfies LogoutRequest)
      : of(void 0);

    return request$.pipe(
      catchError(() => of(void 0)),
      tap(() => this.clearSession())
    );
  }

  refreshSession(): Observable<RefreshTokenResponse> {
    const refreshToken = this.getRefreshToken();
    if (!refreshToken) {
      return throwError(() => new Error('Missing refresh token.'));
    }

    if (this.refreshInFlight$) {
      return this.refreshInFlight$;
    }

    this.refreshInFlight$ = this.http
      .post<ApiResponse<RefreshTokenResponse>>(
        `${this.baseUrl}/refresh`,
        { refreshToken } satisfies RefreshTokenRequest
      )
      .pipe(
        map((response) => response.data),
        tap((response) => this.persistSession(response)),
        switchMap((response) => this.fetchCurrentUser().pipe(map(() => response))),
        finalize(() => {
          this.refreshInFlight$ = null;
        }),
        shareReplay(1)
      );

    return this.refreshInFlight$;
  }

  getAccessToken(): string | null {
    return this.session().accessToken;
  }

  getRefreshToken(): string | null {
    return this.session().refreshToken;
  }

  isAccessTokenExpired(): boolean {
    const expiresAt = this.session().expiresAt;
    if (!expiresAt) {
      return true;
    }

    return new Date(expiresAt).getTime() <= Date.now();
  }

  clearSession(): void {
    this.writeSession({
      accessToken: null,
      refreshToken: null,
      expiresAt: null,
      userId: null,
      name: null,
      email: null,
      avatarUrl: null,
      roles: [],
      isLocalUser: false,
    });
  }

  fetchCurrentUser(): Observable<GetMeResponse> {
    return this.http.get<ApiResponse<GetMeResponse>>(apiUrl('/v1/users/me')).pipe(
      map((response) => response.data),
      tap((response) => this.persistCurrentUser(response))
    );
  }

  private bootstrapCurrentUser(): Observable<GetMeResponse | null> {
    return this.fetchCurrentUser().pipe(
      catchError((error: unknown) => {
        if (error instanceof HttpErrorResponse && error.status === 401) {
          this.clearSession();
        }

        return of(null);
      })
    );
  }

  private persistSession(
    response: LoginResponse | RefreshTokenResponse | GoogleAuthResponse | LineAuthResponse
  ): void {
    const avatarUrl =
      'avatarUrl' in response
        ? response.avatarUrl ?? this.session().avatarUrl
        : 'picture' in response
          ? response.picture ?? this.session().avatarUrl
          : this.session().avatarUrl;

    this.writeSession({
      accessToken: response.accessToken,
      refreshToken: response.refreshToken,
      expiresAt: response.expiresAt,
      userId: 'userId' in response ? response.userId : this.session().userId,
      name: 'name' in response ? response.name : this.session().name,
      email: 'email' in response ? response.email : this.session().email,
      avatarUrl,
      roles: this.session().roles,
      isLocalUser: this.session().isLocalUser,
    });
  }

  private persistCurrentUser(response: GetMeResponse): void {
    this.writeSession({
      ...this.session(),
      userId: response.id,
      name: response.name,
      email: response.email,
      avatarUrl: response.avatarUrl ?? null,
      roles: response.roles,
      isLocalUser: response.isLocalUser,
    });
  }

  private readSession(): AuthSession {
    if (typeof localStorage === 'undefined') {
      return {
        accessToken: null,
        refreshToken: null,
        expiresAt: null,
        userId: null,
        name: null,
        email: null,
        avatarUrl: null,
        roles: [],
        isLocalUser: false,
      };
    }

    return {
      accessToken: localStorage.getItem(this.accessTokenKey),
      refreshToken: localStorage.getItem(this.refreshTokenKey),
      expiresAt: localStorage.getItem(this.expiresAtKey),
      userId: localStorage.getItem(this.userIdKey),
      name: localStorage.getItem(this.userNameKey),
      email: localStorage.getItem(this.userEmailKey),
      avatarUrl: localStorage.getItem(this.userAvatarKey),
      roles: this.readRoles(),
      isLocalUser: localStorage.getItem(this.isLocalUserKey) === 'true',
    };
  }

  private writeSession(session: AuthSession): void {
    if (typeof localStorage !== 'undefined') {
      this.setStorage(this.accessTokenKey, session.accessToken);
      this.setStorage(this.refreshTokenKey, session.refreshToken);
      this.setStorage(this.expiresAtKey, session.expiresAt);
      this.setStorage(this.userIdKey, session.userId);
      this.setStorage(this.userNameKey, session.name);
      this.setStorage(this.userEmailKey, session.email);
      this.setStorage(this.userAvatarKey, session.avatarUrl);
      this.setStorage(this.userRolesKey, JSON.stringify(session.roles));
      this.setStorage(this.isLocalUserKey, String(session.isLocalUser));
    }

    this.session.set(session);
  }

  private setStorage(key: string, value: string | null): void {
    if (typeof localStorage === 'undefined') {
      return;
    }

    if (value) {
      localStorage.setItem(key, value);
      return;
    }

    localStorage.removeItem(key);
  }

  private readRoles(): string[] {
    if (typeof localStorage === 'undefined') {
      return [];
    }

    const raw = localStorage.getItem(this.userRolesKey);
    if (!raw) {
      return [];
    }

    try {
      const roles = JSON.parse(raw);
      return Array.isArray(roles) ? roles.filter((role): role is string => typeof role === 'string') : [];
    } catch {
      return [];
    }
  }

  private buildCallbackUrl(path: string): string {
    if (typeof window === 'undefined') {
      return path;
    }

    return new URL(path, window.location.origin).toString();
  }

  private randomString(length = 64): string {
    const charset = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-._~';
    const bytes = crypto.getRandomValues(new Uint8Array(length));
    return Array.from(bytes, (byte) => charset[byte % charset.length]).join('');
  }

  private async sha256(value: string): Promise<ArrayBuffer> {
    const data = new TextEncoder().encode(value);
    return crypto.subtle.digest('SHA-256', data);
  }

  private base64UrlEncode(buffer: ArrayBuffer): string {
    return btoa(String.fromCharCode(...new Uint8Array(buffer)))
      .replace(/\+/g, '-')
      .replace(/\//g, '_')
      .replace(/=+$/, '');
  }
}
