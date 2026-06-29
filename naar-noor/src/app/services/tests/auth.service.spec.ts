import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { AuthService } from '../auth.service';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;

  // Minimal valid JWT with { "sub": "user-123", "email": "test@example.com" }
  const fakeJwt = [
    'header',
    btoa(JSON.stringify({ sub: 'user-123', email: 'test@example.com' })),
    'signature',
  ].join('.');

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [AuthService],
    });
    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should not be logged in initially', () => {
    expect(service.isLoggedIn()).toBe(false);
    expect(service.currentUser()).toBeNull();
    expect(service.userEmail()).toBeNull();
  });

  it('login() should set session on success and return true', (done) => {
    service.login('test@example.com', 'password123').subscribe((result) => {
      expect(result).toBe(true);
      expect(service.isLoggedIn()).toBe(true);
      expect(service.userEmail()).toBe('test@example.com');
      done();
    });

    const req = httpMock.expectOne((r) => r.url.includes('/api/auth/login'));
    expect(req.request.method).toBe('POST');
    req.flush({ access_token: fakeJwt });
  });

  it('login() should return false on HTTP error', (done) => {
    service.login('bad@example.com', 'wrong').subscribe((result) => {
      expect(result).toBe(false);
      expect(service.isLoggedIn()).toBe(false);
      done();
    });

    const req = httpMock.expectOne((r) => r.url.includes('/api/auth/login'));
    req.flush({ error: 'Unauthorized' }, { status: 401, statusText: 'Unauthorized' });
  });

  it('register() should return true on success', (done) => {
    service.register('new@example.com', 'password').subscribe((result) => {
      expect(result).toBe(true);
      done();
    });

    const req = httpMock.expectOne((r) => r.url.includes('/api/auth/register'));
    expect(req.request.method).toBe('POST');
    req.flush({ userId: 'new-user-id' });
  });

  it('register() should return false on HTTP error', (done) => {
    service.register('taken@example.com', 'password').subscribe((result) => {
      expect(result).toBe(false);
      done();
    });

    const req = httpMock.expectOne((r) => r.url.includes('/api/auth/register'));
    req.flush({ error: 'Email taken' }, { status: 400, statusText: 'Bad Request' });
  });

  it('resetPassword() should return true on success', (done) => {
    service.resetPassword('user@example.com').subscribe((result) => {
      expect(result).toBe(true);
      done();
    });

    const req = httpMock.expectOne((r) => r.url.includes('/api/auth/reset-password'));
    expect(req.request.method).toBe('POST');
    req.flush({});
  });

  it('resetPassword() should return false on HTTP error', (done) => {
    service.resetPassword('unknown@example.com').subscribe((result) => {
      expect(result).toBe(false);
      done();
    });

    const req = httpMock.expectOne((r) => r.url.includes('/api/auth/reset-password'));
    req.flush({ error: 'Not found' }, { status: 404, statusText: 'Not Found' });
  });

  it('getToken() should return null when not logged in', () => {
    expect(service.getToken()).toBeNull();
  });

  it('getToken() should return the token after login', (done) => {
    service.login('test@example.com', 'password').subscribe(() => {
      expect(service.getToken()).toBe(fakeJwt);
      done();
    });

    const req = httpMock.expectOne((r) => r.url.includes('/api/auth/login'));
    req.flush({ access_token: fakeJwt });
  });

  it('logout() should clear session', (done) => {
    service.login('test@example.com', 'password').subscribe(() => {
      expect(service.isLoggedIn()).toBe(true);

      service.logout();
      expect(service.isLoggedIn()).toBe(false);
      expect(service.getToken()).toBeNull();
      done();
    });

    const loginReq = httpMock.expectOne((r) => r.url.includes('/api/auth/login'));
    loginReq.flush({ access_token: fakeJwt });

    const logoutReq = httpMock.expectOne((r) => r.url.includes('/api/auth/logout'));
    logoutReq.flush({});
  });

  it('logout() when not logged in should not throw', () => {
    expect(() => service.logout()).not.toThrow();
  });

  it('refreshUser() should return false when not logged in', (done) => {
    service.refreshUser().subscribe((result) => {
      expect(result).toBe(false);
      done();
    });
  });

  it('refreshUser() should return true on success', (done) => {
    service.login('test@example.com', 'password').subscribe(() => {
      service.refreshUser().subscribe((result) => {
        expect(result).toBe(true);
        done();
      });

      const meReq = httpMock.expectOne((r) => r.url.includes('/api/auth/me'));
      meReq.flush({ userId: 'user-123', email: 'test@example.com' });
    });

    const loginReq = httpMock.expectOne((r) => r.url.includes('/api/auth/login'));
    loginReq.flush({ access_token: fakeJwt });
  });
});
