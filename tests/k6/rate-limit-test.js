import http from 'k6/http';
import { check, sleep } from 'k6';
import { Counter } from 'k6/metrics';

/**
 * Rate Limit Test
 * Validates: Requests exceeding limit return 429
 * Run: k6 run tests/k6/rate-limit-test.js
 */

const API_BASE = __ENV.API_BASE || 'http://localhost:5177';
const rejected = new Counter('rejected_requests');

export const options = {
    scenarios: {
        burst: {
            executor: 'constant-arrival-rate',
            rate: 150,           // 150 req/s (exceeds 100/s limit)
            timeUnit: '1s',
            duration: '10s',
            preAllocatedVUs: 50,
            maxVUs: 100,
        },
    },
    thresholds: {
        'rejected_requests': ['count>50'],
    },
};

export default function () {
    const res = http.get(`${API_BASE}/api/products`);

    if (res.status === 429) {
        rejected.add(1);
    }

    check(res, {
        'status is 200 or 429': (r) => r.status === 200 || r.status === 429,
    });
}

export function handleSummary(data) {
    const total = data.metrics.http_reqs.values.count;
    const rejected = data.metrics.rejected_requests?.values?.count || 0;

    console.log(`\n========== Rate Limit Test ==========`);
    console.log(`Total requests: ${total}`);
    console.log(`Rejected (429): ${rejected}`);
    console.log(`Rejection rate: ${((rejected / total) * 100).toFixed(2)}%`);
    console.log(`======================================\n`);

    return {};
}
