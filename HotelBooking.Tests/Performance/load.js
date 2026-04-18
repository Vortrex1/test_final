import http from 'k6/http'
import { check, group, sleep } from 'k6'

export const options = {
  scenarios: {
    room_list: {
      executor: 'constant-vus',
      vus: 15,
      duration: '45s',
      exec: 'getRooms',
    },
    available_room_check: {
      executor: 'constant-vus',
      vus: 8,
      duration: '45s',
      exec: 'getAvailableRooms',
    },
  },
  thresholds: {
    http_req_failed: ['rate<0.02'],
    'http_req_duration{scenario:room_list}': ['p(95)<800'],
    'http_req_duration{scenario:available_room_check}': ['p(95)<800'],
  },
}

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000'

export function getRooms() {
  group('Get all rooms', () => {
    const res = http.get(`${BASE_URL}/api/rooms`)

    check(res, {
      'status 200': r => r.status === 200,
      'response not empty': r => r.body && r.body.length > 0,
    })

    sleep(1)
  })
}

export function getAvailableRooms() {
  group('Get available rooms', () => {
    const now = new Date()
    const checkIn = new Date(now.getTime() + 24 * 60 * 60 * 1000).toISOString().split('T')[0]
    const checkOut = new Date(now.getTime() + 3 * 24 * 60 * 60 * 1000).toISOString().split('T')[0]

    const res = http.get(`${BASE_URL}/api/rooms/available?checkIn=${checkIn}&checkOut=${checkOut}`)

    check(res, {
      'status 200': r => r.status === 200,
      'response not empty': r => r.body && r.body.length > 0,
    })

    sleep(1)
  })
}
