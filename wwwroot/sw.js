const CACHE_NAME = 'briefx-cache-v1';
const urlsToCache = [
  '/',
  '/css/site.css',
  '/js/site.js',
  '/assets/img/briefxlogo.png'
];

// Service Worker kurulumu
self.addEventListener('install', event => {
  event.waitUntil(
    caches.open(CACHE_NAME)
      .then(cache => {
        console.log('Cache açıldı');
        return cache.addAll(urlsToCache);
      })
  );
});

// Cache'ten dosya getirme
self.addEventListener('fetch', event => {
  event.respondWith(
    caches.match(event.request)
      .then(response => {
        // Cache'te varsa cache'ten döndür
        if (response) {
          return response;
        }

        // Cache'te yoksa network'ten getir
        return fetch(event.request)
          .then(response => {
            // Geçerli response kontrolü
            if (!response || response.status !== 200 || response.type !== 'basic') {
              return response;
            }

            // Response'u clone'la ve cache'e ekle
            const responseToCache = response.clone();
            caches.open(CACHE_NAME)
              .then(cache => {
                cache.put(event.request, responseToCache);
              });

            return response;
          })
          .catch(() => {
            // Network hatası durumunda offline sayfası göster
            return caches.match('/offline.html');
          });
      })
  );
});

// Eski cache'leri temizle
self.addEventListener('activate', event => {
  event.waitUntil(
    caches.keys().then(cacheNames => {
      return Promise.all(
        cacheNames.map(cacheName => {
          if (cacheName !== CACHE_NAME) {
            return caches.delete(cacheName);
          }
        })
      );
    })
  );
});