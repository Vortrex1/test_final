import http from 'k6/http'
import { check, sleep } from 'k6'

export const options = {
  vus: 1,
  duration: '15s',
  thresholds: {
    http_req_failed: ['rate<0.01'],
    http_req_duration: ['p(95)<500'],
  },
}

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000'

export default function () {
  const res = http.get(`${BASE_URL}/api/rooms`)

  check(res, {
    'room list returned': r => r.status === 200,
    'has body': r => r.body && r.body.length > 0,
  })

  sleep(1)
}
