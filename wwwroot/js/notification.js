document.addEventListener('DOMContentLoaded', function () {
    const badge = document.getElementById('notificationBadge');
    const list = document.getElementById('notificationItems');
    const markAllBtn = document.getElementById('markAllReadBtn');

    // Build SignalR Connection
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/notificationHub")
        .withAutomaticReconnect()
        .build();

    // Start Connection
    connection.start().then(function () {
        console.log("SignalR connected.");
        loadUnreadNotifications();
    }).catch(function (err) {
        return console.error(err.toString());
    });

    // Listen to ReceiveNotification
    connection.on("ReceiveNotification", function (data) {
        updateBadge(data.unreadCount);
        addNotificationToList(data, true);
        showToast(data.title, data.message, data.url);
    });

    // Load unread notifications on startup
    function loadUnreadNotifications() {
        fetch('/api/NotificationsApi/unread')
            .then(res => res.json())
            .then(data => {
                updateBadge(data.unreadCount);
                list.innerHTML = '';
                if (data.notifications.length === 0) {
                    list.innerHTML = '<li><span class="dropdown-item text-muted text-center py-3">Không có thông báo mới</span></li>';
                } else {
                    data.notifications.forEach(n => addNotificationToList(n, false));
                }
            })
            .catch(err => console.error("Error loading notifications:", err));
    }

    function updateBadge(count) {
        if (count > 0) {
            badge.textContent = count;
            badge.style.display = 'block';
        } else {
            badge.style.display = 'none';
        }
    }

    function addNotificationToList(notification, isNew) {
        const item = document.createElement('li');
        const link = notification.url ? notification.url : 'javascript:void(0)';
        
        // Remove "no notifications" message if it exists
        const noNotif = list.querySelector('.text-muted');
        if (noNotif) {
            list.innerHTML = '';
        }

        item.innerHTML = `
            <a class="dropdown-item text-wrap d-flex flex-column py-2 border-bottom" href="${link}" data-id="${notification.id}" onclick="markAsRead(${notification.id}, this, event)">
                <div class="d-flex w-100 justify-content-between align-items-center">
                    <strong class="mb-1 text-primary" style="font-size: 0.9rem;">${notification.title}</strong>
                    <small class="text-muted" style="font-size: 0.7rem;">${notification.createdAt}</small>
                </div>
                <span class="mb-1" style="font-size: 0.85rem; line-height: 1.2;">${notification.message}</span>
            </a>
        `;
        
        if (isNew) {
            list.prepend(item);
        } else {
            list.appendChild(item);
        }
    }

    window.markAsRead = function(id, element, event) {
        if (element.getAttribute('href') === 'javascript:void(0)') {
            event.preventDefault();
        }
        
        fetch(`/api/NotificationsApi/mark-read/${id}`, { method: 'POST' })
            .then(res => res.json())
            .then(data => {
                updateBadge(data.unreadCount);
                element.classList.add('opacity-50');
            })
            .catch(err => console.error("Error marking as read:", err));
    };

    if (markAllBtn) {
        markAllBtn.addEventListener('click', function(e) {
            e.preventDefault();
            e.stopPropagation();
            
            fetch('/api/NotificationsApi/mark-all-read', { method: 'POST' })
                .then(res => res.json())
                .then(data => {
                    updateBadge(0);
                    loadUnreadNotifications();
                });
        });
    }

    function showToast(title, message, url) {
        const container = document.createElement('div');
        container.className = 'toast-container-qlpt';
        container.style.position = 'fixed';
        container.style.bottom = '20px';
        container.style.right = '20px';
        container.style.zIndex = '9999';

        container.innerHTML = `
            <div class="toast-qlpt toast-success" style="opacity: 1; transition: opacity 0.4s; display: flex; align-items: start; padding: 12px 16px; background: white; border-left: 4px solid #0d6efd; box-shadow: 0 4px 12px rgba(0,0,0,0.15); border-radius: 4px; width: 300px;">
                <i class="bi bi-bell-fill text-primary" style="font-size: 1.2rem; margin-right: 12px; margin-top: 2px;"></i>
                <div class="d-flex flex-column">
                    <strong style="color: #333; font-size: 0.95rem; margin-bottom: 4px;">${title}</strong>
                    <span style="color: #666; font-size: 0.85rem; line-height: 1.3;">${message}</span>
                </div>
            </div>
        `;
        
        if (url) {
            container.style.cursor = 'pointer';
            container.onclick = () => window.location.href = url;
        }

        document.body.appendChild(container);

        setTimeout(() => {
            container.querySelector('.toast-qlpt').style.opacity = '0';
            setTimeout(() => container.remove(), 400);
        }, 5000);
    }
});
