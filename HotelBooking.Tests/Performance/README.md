# Performance tests with k6

This folder contains k6 scripts for API performance testing.

## Scripts

- `smoke.js` — quick smoke test to validate API availability
- `load.js` — load test for sustained traffic
- `stress.js` — stress test to push the API to its limits

## Run commands

```bash
k6 run --env BASE_URL=http://localhost:5000 HotelBooking.Tests/Performance/smoke.js
k6 run --env BASE_URL=http://localhost:5000 HotelBooking.Tests/Performance/load.js
k6 run --env BASE_URL=http://localhost:5000 HotelBooking.Tests/Performance/stress.js
```

If your API listens on a different address/port, set `BASE_URL` accordingly.
