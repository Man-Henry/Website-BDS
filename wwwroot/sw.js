const CACHE_NAME = 'qlpt-v2';
const urlsToCache = [
    '/lib/bootstrap/dist/css/bootstrap.min.css',
    '/lib/bootstrap-icons/bootstrap-icons.min.css',
    '/manifest.json'
];

// Install — cache static assets only (not HTML pages)
self.addEventListener('install', event => {
    event.waitUntil(
        caches.open(CACHE_NAME)
            .then(cache => {
                return Promise.all(urlsToCache.map(url =>
                    cache.add(url).catch(err => console.log('Failed to cache', url, err))
                ));
            })
            .then(() => self.skipWaiting()) // Activate immediately
    );
});

// Activate — delete ALL old caches
self.addEventListener('activate', event => {
    event.waitUntil(
        caches.keys()
            .then(keys => Promise.all(
                keys.filter(k => k !== CACHE_NAME).map(k => caches.delete(k))
            ))
            .then(() => self.clients.claim()) // Take control of all pages
    );
});

// Fetch — NETWORK-FIRST strategy
// Always try network first, fall back to cache only when offline
self.addEventListener('fetch', event => {
    // Skip non-GET requests
    if (event.request.method !== 'GET') return;

    // For HTML pages — always network, never cache
    if (event.request.mode === 'navigate') {
        event.respondWith(
            fetch(event.request).catch(() => caches.match('/'))
        );
        return;
    }

    // For other assets — network-first with cache fallback
    event.respondWith(
        fetch(event.request)
            .then(response => {
                // Clone and update cache with fresh version
                const clone = response.clone();
                caches.open(CACHE_NAME).then(cache => cache.put(event.request, clone));
                return response;
            })
            .catch(() => caches.match(event.request))
    );
});
