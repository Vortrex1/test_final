import http from 'k6/http'
import { check, sleep } from 'k6'

export const options = {
  scenarios: {
    stress_test: {
      executor: 'ramping-vus',
      startVUs: 2,
      stages: [
        { duration: '20s', target: 20 },
        { duration: '40s', target: 40 },
        { duration: '30s', target: 50 },
        { duration: '30s', target: 50 },
        { duration: '20s', target: 5 },
      ],
      exec: 'stressScenario',
    },
  },
  thresholds: {
    http_req_failed: ['rate<0.05'],
    http_req_duration: ['p(95)<1200'],
  },
}

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000'

export function stressScenario() {
  const res = http.get(`${BASE_URL}/api/rooms`)

  check(res, {
    'status was 200': r => r.status === 200,
  })

  sleep(1)
}
