import http from 'k6/http';
import { check, sleep } from 'k6';
import { Counter, Trend } from 'k6/metrics';

/**
 * Flash Sale Test
 * Validates: 1000 users compete for 100 items, no overselling
 * Prerequisites: 1) Warm up stock to Redis  2) Valid JWT Token
 * Run: k6 run --env TOKEN=xxx --env FLASH_SALE_ITEM_ID=xxx tests/k6/flash-sale-test.js
 */

const API_BASE = __ENV.API_BASE || 'http://localhost:5177';
const TOKEN = __ENV.TOKEN || '';
const FLASH_SALE_ITEM_ID = __ENV.FLASH_SALE_ITEM_ID || '';

const successPurchases = new Counter('success_purchases');
const stockInsufficient = new Counter('stock_insufficient');
const limitExceeded = new Counter('limit_exceeded');
const otherErrors = new Counter('other_errors');
const purchaseTime = new Trend('purchase_time');

export const options = {
    scenarios: {
        flash_sale: {
            executor: 'ramping-vus',
            startVUs: 0,
            stages: [
                { duration: '2s', target: 100 },
                { duration: '5s', target: 1000 },
                { duration: '10s', target: 1000 },
                { duration: '3s', target: 0 },
            ],
        },
    },
    thresholds: {
        'http_req_duration': ['p(95)<500'],
        'success_purchases': ['count<=100'],
    },
};

export default function () {
    const payload = JSON.stringify({
        flashSaleItemId: FLASH_SALE_ITEM_ID,
        quantity: 1,
    });

    const params = {
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${TOKEN}`,
        },
    };

    const start = Date.now();
    const res = http.post(`${API_BASE}/api/flash-sale/purchase`, payload, params);
    purchaseTime.add(Date.now() - start);

    if (res.status === 202) {
        successPurchases.add(1);
    } else if (res.status === 409) {
        const body = JSON.parse(res.body);
        if (body.message?.includes('stock') || body.message?.includes('庫存')) {
            stockInsufficient.add(1);
        } else if (body.message?.includes('limit') || body.message?.includes('限購')) {
            limitExceeded.add(1);
        } else {
            otherErrors.add(1);
        }
    } else if (res.status === 401) {
        console.log('Invalid or expired token');
    } else if (res.status === 429) {
        // Rate limited - expected
    } else {
        otherErrors.add(1);
    }

    check(res, {
        'status is expected': (r) => [202, 409, 429, 401].includes(r.status),
    });
}

export function handleSummary(data) {
    const success = data.metrics.success_purchases?.values?.count || 0;
    const stockOut = data.metrics.stock_insufficient?.values?.count || 0;
    const limitOut = data.metrics.limit_exceeded?.values?.count || 0;
    const errors = data.metrics.other_errors?.values?.count || 0;
    const total = data.metrics.http_reqs?.values?.count || 0;

    console.log(`\n========== Flash Sale Test ==========`);
    console.log(`Total requests: ${total}`);
    console.log(`Successful: ${success}`);
    console.log(`Out of stock: ${stockOut}`);
    console.log(`Limit exceeded: ${limitOut}`);
    console.log(`Other errors: ${errors}`);
    console.log(`======================================`);

    if (success > 100) {
        console.log(`\nOverselling detected! ${success} > stock 100`);
    } else {
        console.log(`\nNo overselling. ${success} <= stock 100`);
    }

    return {};
}
