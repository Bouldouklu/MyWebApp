// Caution! Be sure you understand the caching strategy before changing it.
// Note: Consult the following PWA Surf blog post before changing this code
// https://pwasurfer.com/offline-app-strategies-for-blazor-wasm/

const CACHE_PREFIX = 'offline-cache-';
const CACHE_SUFFIX = '-v1';
const CACHE_NAME = `${CACHE_PREFIX}MyWebApp${CACHE_SUFFIX}`;
const offlineAssetsInclude = [ /\.dll$/, /\.pdb$/, /\.wasm/, /\.html/, /\.js$/, /\.json$/, /\.css$/, /\.woff$/, /\.png$/, /\.jpe?g$/, /\.gif$/, /\.ico$/, /\.blat$/, /\.dat$/ ];
const offlineAssetsExclude = [ /^service-worker\.js$/ ];

self.addEventListener('install', event => event.waitUntil(onInstall(event)));
self.addEventListener('activate', event => event.waitUntil(onActivate(event)));
self.addEventListener('fetch', event => event.respondWith(onFetch(event)));

const onInstall = async (event) => {
    console.info('Service worker: Install');

    // Fetch and cache all matching items from the assets manifest
    const assetsRequests = self.assetsManifest.assets
        .filter(asset => offlineAssetsInclude.some(pattern => pattern.test(asset.url)))
        .filter(asset => !offlineAssetsExclude.some(pattern => pattern.test(asset.url)))
        .map(asset => new Request(asset.url, { integrity: asset.hash, cache: 'no-cache' }));
    await caches.open(CACHE_NAME).then(cache => cache.addAll(assetsRequests));
};

const onActivate = async (event) => {
    console.info('Service worker: Activate');

    // Delete unused caches
    const cacheNames = await caches.keys();
    await Promise.all(cacheNames
        .filter(cacheName => cacheName.startsWith(CACHE_PREFIX) && cacheName !== CACHE_NAME)
        .map(cacheName => caches.delete(cacheName)));
};

const onFetch = async (event) => {
    let cachedResponse = null;
    if (event.request.method === 'GET') {
        // For all navigation requests, try to serve index.html from cache
        // If you need some URLs to be server-rendered, edit the following check to exclude those URLs
        const shouldServeIndexHtml = event.request.mode === 'navigate'
            && !event.request.url.includes('/Identity/');

        const request = shouldServeIndexHtml ? 'index.html' : event.request;
        const cache = await caches.open(CACHE_NAME);
        cachedResponse = await cache.match(request);
    }

    return cachedResponse || fetch(event.request);
};