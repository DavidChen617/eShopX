import http from 'k6/http';
import { check, sleep } from 'k6';
import { Counter } from 'k6/metrics';

/**
 * Optimistic Lock Test
 * Validates: 50 concurrent orders on product (stock=10), no overselling
 * Prerequisites: 1) Product with stock=10  2) Valid JWT Token
 * Run: k6 run --env TOKEN=xxx --env PRODUCT_ID=xxx tests/k6/optimistic-lock-test.js
 */

const API_BASE = __ENV.API_BASE || 'http://localhost:5177';
const TOKEN = __ENV.TOKEN || '';
const PRODUCT_ID = __ENV.PRODUCT_ID || '';

const successOrders = new Counter('success_orders');
const conflictErrors = new Counter('conflict_errors');
const rateLimited = new Counter('rate_limited');
const otherErrors = new Counter('other_errors');

export const options = {
    scenarios: {
        concurrent_orders: {
            executor: 'shared-iterations',
            vus: 50,
            iterations: 200,
            maxDuration: '30s',
        },
    },
    thresholds: {
        'success_orders': ['count<=10'],
    },
};

export default function () {
    const payload = JSON.stringify({
        userId: '019c3bf5-4221-72ae-9d34-88cc5d88373b',
        shippingName: 'Test User',
        shippingAddress: 'Test Address',
        shippingPhone: '0912345678',
        items: [
            {
                productId: PRODUCT_ID,
                quantity: 1,
            },
        ],
    });

    const params = {
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${TOKEN}`,
        },
    };

    const res = http.post(`${API_BASE}/api/orders`, payload, params);

    if (res.status === 201) {
        successOrders.add(1);
    } else if (res.status === 409) {
        conflictErrors.add(1);
    } else if (res.status === 429) {
        rateLimited.add(1);
    } else {
        otherErrors.add(1);
        console.log(`Unexpected status: ${res.status}, body: ${res.body}`);
    }

    check(res, {
        'status is expected': (r) => [201, 409, 429].includes(r.status),
    });
}

export function handleSummary(data) {
    const success = data.metrics.success_orders?.values?.count || 0;
    const conflicts = data.metrics.conflict_errors?.values?.count || 0;
    const limited = data.metrics.rate_limited?.values?.count || 0;
    const errors = data.metrics.other_errors?.values?.count || 0;

    console.log(`\n========== Optimistic Lock Test ==========`);
    console.log(`Successful orders: ${success}`);
    console.log(`Conflicts/Out of stock: ${conflicts}`);
    console.log(`Rate limited (429): ${limited}`);
    console.log(`Other errors: ${errors}`);
    console.log(`===========================================`);

    if (success > 10) {
        console.log(`\nOverselling detected! ${success} > stock 10`);
    } else {
        console.log(`\nNo overselling. ${success} <= stock 10`);
    }

    return {};
}
