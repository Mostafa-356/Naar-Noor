import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  stages: [
    { duration: '2m', target: 20 },   // Ramp-up: 0 to 20 VUs over 2 min
    { duration: '5m', target: 100 },  // Ramp-up: 20 to 100 VUs over 5 min
    { duration: '5m', target: 100 },  // Stay at 100 VUs for 5 min
    { duration: '2m', target: 0 },    // Ramp-down: 100 to 0 VUs over 2 min
  ],
  thresholds: {
    'http_req_duration': ['p(95)<200', 'p(99)<400'],  // 95th percentile < 200ms, 99th < 400ms
    'http_req_failed': ['rate<0.05'],                  // Error rate < 5%
  },
};

const API_URL = __ENV.API_URL || 'http://localhost:8080/api';

export default function () {
  // Test 1: Health check
  let healthRes = http.get(`${API_URL}/../health`);
  check(healthRes, {
    'health check status is 200': (r) => r.status === 200,
  });
  sleep(1);

  // Test 2: Get menu (read-heavy)
  let menuRes = http.get(`${API_URL}/menu`);
  check(menuRes, {
    'menu list status is 200': (r) => r.status === 200,
    'menu response time < 200ms': (r) => r.timings.duration < 200,
  });
  sleep(2);

  // Test 3: Create reservation (write operation)
  let reservationPayload = JSON.stringify({
    email: `test-${Date.now()}@test.com`,
    phone: '+1234567890',
    partySize: 4,
    reservationDate: new Date().toISOString(),
    specialRequests: 'Test reservation from load test',
  });

  let resRes = http.post(`${API_URL}/reservations`, reservationPayload, {
    headers: { 'Content-Type': 'application/json' },
  });
  check(resRes, {
    'create reservation status is 201 or 200': (r) => r.status === 201 || r.status === 200,
  });
  sleep(2);

  // Test 4: List reviews (paginated)
  let reviewRes = http.get(`${API_URL}/reviews?page=1&pageSize=10`);
  check(reviewRes, {
    'reviews list status is 200': (r) => r.status === 200,
  });
  sleep(1);
}
