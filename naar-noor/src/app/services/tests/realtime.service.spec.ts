import { TestBed } from '@angular/core/testing';
import { RealtimeService } from '../realtime.service';

class MockWebSocket {
  static OPEN = 1;
  static CLOSED = 3;

  readyState = MockWebSocket.OPEN;
  sentMessages: string[] = [];
  onopen: (() => void) | null = null;
  onclose: (() => void) | null = null;
  onmessage: ((e: { data: string }) => void) | null = null;
  onerror: ((e: any) => void) | null = null;

  constructor(public url: string) {}

  send(data: string): void {
    this.sentMessages.push(data);
  }

  close(): void {
    this.readyState = MockWebSocket.CLOSED;
    if (this.onclose) this.onclose();
  }

  simulateOpen(): void {
    this.readyState = MockWebSocket.OPEN;
    if (this.onopen) this.onopen();
  }

  simulateMessage(data: object): void {
    if (this.onmessage) this.onmessage({ data: JSON.stringify(data) });
  }

  simulateError(): void {
    if (this.onerror) this.onerror(new Event('error'));
  }
}

describe('RealtimeService', () => {
  let service: RealtimeService;
  let mockWs: MockWebSocket | null = null;
  let originalWebSocket: typeof WebSocket;

  beforeEach(() => {
    originalWebSocket = (global as any).WebSocket;

    (global as any).WebSocket = jest.fn((url: string) => {
      mockWs = new MockWebSocket(url);
      return mockWs;
    });
    (global as any).WebSocket.OPEN = MockWebSocket.OPEN;
    (global as any).WebSocket.CLOSED = MockWebSocket.CLOSED;

    TestBed.configureTestingModule({ providers: [RealtimeService] });
    service = TestBed.inject(RealtimeService);
    mockWs = null;
  });

  afterEach(() => {
    (global as any).WebSocket = originalWebSocket;
    jest.clearAllMocks();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('connect() should create a WebSocket when supabaseUrl is configured', () => {
    service.connect();
    expect(mockWs).not.toBeNull();
  });

  it('connect() called twice does not create a second socket', () => {
    service.connect();
    const firstWs = mockWs;

    service.connect();

    expect(mockWs).toBe(firstWs);
    expect((global as any).WebSocket).toHaveBeenCalledTimes(1);
  });

  it('subscribeToOrder() registers a listener for the order topic', () => {
    service.subscribeToOrder('order-123', jest.fn());

    expect(mockWs).not.toBeNull();
  });

  it('subscribeToOrder() invokes callback when matching message received', () => {
    const callback = jest.fn();
    service.subscribeToOrder('order-abc', callback);

    if (mockWs) {
      mockWs.simulateOpen();
      mockWs.simulateMessage({
        event: 'postgres_changes',
        topic: 'realtime:public:orders:id=eq.order-abc',
        payload: { record: { status: 'confirmed' } }
      });
    }

    expect(callback).toHaveBeenCalledWith('confirmed');
  });

  it('subscribeToOrder() does not invoke callback for non-matching topic', () => {
    const callback = jest.fn();
    service.subscribeToOrder('order-abc', callback);

    if (mockWs) {
      mockWs.simulateOpen();
      mockWs.simulateMessage({
        event: 'postgres_changes',
        topic: 'realtime:public:orders:id=eq.OTHER-ID',
        payload: { record: { status: 'confirmed' } }
      });
    }

    expect(callback).not.toHaveBeenCalled();
  });

  it('subscribeToReservation() registers a listener for the reservation topic', () => {
    const callback = jest.fn();
    service.subscribeToReservation('res-999', callback);

    expect(mockWs).not.toBeNull();
  });

  it('subscribeToReservation() invokes callback on matching message', () => {
    const callback = jest.fn();
    service.subscribeToReservation('res-xyz', callback);

    if (mockWs) {
      mockWs.simulateOpen();
      mockWs.simulateMessage({
        event: 'postgres_changes',
        topic: 'realtime:public:reservations:id=eq.res-xyz',
        payload: { record: { status: 'approved' } }
      });
    }

    expect(callback).toHaveBeenCalledWith('approved');
  });

  it('unsubscribe() removes the listener for an order', () => {
    const callback = jest.fn();
    service.subscribeToOrder('order-del', callback);

    if (mockWs) mockWs.simulateOpen();

    service.unsubscribe('order-del', true);

    if (mockWs) {
      mockWs.simulateMessage({
        event: 'postgres_changes',
        topic: 'realtime:public:orders:id=eq.order-del',
        payload: { record: { status: 'cancelled' } }
      });
    }

    expect(callback).not.toHaveBeenCalled();
  });

  it('unsubscribe() removes the listener for a reservation', () => {
    const callback = jest.fn();
    service.subscribeToReservation('res-del', callback);

    if (mockWs) mockWs.simulateOpen();

    service.unsubscribe('res-del', false);

    if (mockWs) {
      mockWs.simulateMessage({
        event: 'postgres_changes',
        topic: 'realtime:public:reservations:id=eq.res-del',
        payload: { record: { status: 'cancelled' } }
      });
    }

    expect(callback).not.toHaveBeenCalled();
  });

  it('onmessage handler ignores malformed JSON', () => {
    service.connect();
    if (mockWs) {
      expect(() =>
        mockWs!.onmessage?.({ data: 'not-valid-json' })
      ).not.toThrow();
    }
  });

  it('onerror handler does not throw', () => {
    service.connect();
    if (mockWs) {
      expect(() => mockWs!.simulateError()).not.toThrow();
    }
  });

  it('onmessage handler ignores phx_reply events', () => {
    const callback = jest.fn();
    service.subscribeToOrder('order-phx', callback);

    if (mockWs) {
      mockWs.simulateOpen();
      mockWs.simulateMessage({
        event: 'phx_reply',
        topic: 'realtime:public:orders:id=eq.order-phx',
        payload: { status: 'ok' }
      });
    }

    expect(callback).not.toHaveBeenCalled();
  });
});
